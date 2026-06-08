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
├── Backend/                  # ASP.NET Core Web API
│   ├── Controllers/          # REST endpoints
│   ├── Services/             # Business logic
│   ├── Repositories/         # Data access layer
│   ├── Hubs/                 # SignalR hub
│   ├── Infrastructure/       # DB context, background listeners
│   └── DTOs/ Models/         # Data contracts
└── ktc-frontend/             # Angular SPA
    └── src/app/
        ├── features/         # Feature modules (atm, dashboard, campaign, …)
        ├── core/             # Auth, guards, interceptors, SignalR service
        └── shared/           # Reusable components, directives
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

## Authentication

Users authenticate via Active Directory credentials. The API issues a JWT token scoped to one of two roles:

- **Superviseur** — read-only access across the platform
- **Support** — full read/write access including remote commands and configuration 
