# KTC Monitoring — ATM Network Management Platform

A full-stack web application for real-time monitoring and management of ATM networks, built for banking operations teams.

## Overview

KTC Monitoring centralizes ATM supervision into a single platform: live status tracking, cash cassette levels, electronic/video journals, remote command dispatch, ticket management, and marketing campaign scheduling — all with role-based access control and real-time push updates via SignalR.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8, C#, Entity Framework Core |
| Database | SQL Server (with `SqlDependency` change listeners) |
| Real-time | SignalR (WebSocket) |
| Frontend | Angular 21, TypeScript |
| Auth | JWT + Active Directory, RBAC (Superviseur / Support) |
| Export | jsPDF, xlsx |

## Key Features

- **Live dashboard** — NOC-style overview of the entire ATM fleet with real-time status updates
- **ATM detail pages** — hardware info, software versions, XFS/app counters, certificates, schedules
- **Cash cassette monitoring** — cassette levels and replenishment history
- **Electronic & video journals** — searchable logs per terminal
- **Transaction audit** — transaction search and history
- **Remote commands** — dispatch actions to ATMs directly from the UI
- **Ticket management** — create, search, and track incident tickets
- **Campaign management** — schedule and control marketing content displayed on ATMs
- **Map view** — geographic visualization of the ATM network
- **Availability reports** — uptime metrics per terminal
- **Export** — download reports as Excel or PDF
- **Multi-level organization** — group ATMs by Region, Branch, and Business entity
- **RBAC** — read-only Superviseur role and full-access Support role

## Project Structure

```
Monitoring-main/
├── Backend/                        # ASP.NET Core Web API
│   ├── Controllers/                # REST endpoints
│   ├── Services/                   # Business logic
│   ├── Repositories/               # Data access layer
│   ├── Hubs/                       # SignalR hub
│   ├── Infrastructure/             # DB context, background listeners
│   └── DTOs/ Models/               # Data contracts
├── Backend.IntegrationTests/       # xUnit integration tests (in-memory test server)
│   ├── Fixtures/                   # KtcWebFactory — WebApplicationFactory setup
│   └── Tests/                      # AtmReadTests, CampaignReadTests, GroupReadTests, MiddlewareTests
└── ktc-frontend/                   # Angular SPA
    └── src/app/
        ├── features/               # Feature modules (atm, dashboard, campaign, …)
        ├── core/                   # Auth, guards, interceptors, SignalR service
        └── shared/                 # Reusable components, directives
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+ / npm 10+
- SQL Server instance with the KTC database

### Backend

```bash
cd Backend
# Set your connection string in appsettings.Development.json
dotnet run
# Swagger available at https://localhost:<port>/swagger
```

### Frontend

```bash
cd ktc-frontend
npm install
npm start
# App available at http://localhost:4200
```

## Testing

### Backend — Integration Tests

The `Backend.IntegrationTests` project uses **xUnit** and `Microsoft.AspNetCore.Mvc.Testing` to spin up the real ASP.NET Core pipeline in memory against the development database.

**How it works**

- `KtcWebFactory` — a custom `WebApplicationFactory<Program>` that:
  - Replaces JWT authentication with a `TestAuthHandler` that auto-authenticates every request as a **Support** user (full access)
  - Removes `IHostedService` registrations so `SqlTableDependency` background listeners don't start during tests
  - Inherits the connection string from `appsettings.Development.json` (real database, read-only queries only)

**Test suites**

| File | What it covers |
|---|---|
| `AtmReadTests` | GET endpoints for clients, regions, businesses, hardware types, command types, transaction lookups |
| `CampaignReadTests` | GET list / GET by id / 404 on unknown id for campaigns |
| `GroupReadTests` | GET list / GET by id / 404 on unknown id for groups |
| `MiddlewareTests` | Validation middleware — empty-name payloads return 400 for campaign and group creation |

**Run the tests**

```bash
cd Backend.IntegrationTests
dotnet test
```

> Requires a reachable SQL Server instance with the KTC database (same connection string used for local development).

### Frontend — Unit Tests

Vitest is available as a dev dependency. No spec files exist yet — the setup is in place for future unit tests.

```bash
cd ktc-frontend
npm test
```

## Authentication

Users authenticate via Active Directory credentials. The API issues a JWT token scoped to one of two roles:

- **Superviseur** — read-only access across the platform
- **Support** — full read/write access including remote commands and configuration 
