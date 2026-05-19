<div align="center">

<!-- Replace with your actual banner image -->
<img src="docs/assets/banner.png" alt="ATM Supervision Platform Banner" width="100%" />

# 🏧 ATM Supervision Platform

**A modern, web-based multi-vendor ATM monitoring platform built as a replacement for the legacy KTC Operator Access desktop application.**

<br/>

[![Angular](https://img.shields.io/badge/Angular-19-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![SignalR](https://img.shields.io/badge/SignalR-Real--Time-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Grafana](https://img.shields.io/badge/Grafana-Dashboards-F46800?style=for-the-badge&logo=grafana&logoColor=white)](https://grafana.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

<br/>

> Final Year Project (PFE) — EHEI · b4ps  
> Replacing a legacy desktop supervision tool with a zero-installation web platform for real-time ATM fleet monitoring.

</div>

---

## 📋 Table of Contents

- [About the Project](#-about-the-project)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Installation & Setup](#-installation--setup)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Screenshots](#-screenshots)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [Acknowledgements](#-acknowledgements)
- [Contact](#-contact)
- [License](#-license)

---

## 🎯 About the Project

### Description

The **ATM Supervision Platform** is a full-stack web application designed to give NOC (Network Operations Center) operators a unified, real-time view of an entire ATM fleet — regardless of vendor. It replaces the legacy **KTC Operator Access** desktop client, which required local installation on every operator workstation and lacked modern UX, remote access capabilities, and integrated analytics.

### Academic & Professional Context

| | |
|---|---|
| **Institution** | EHEI — École des Hautes Études d'Ingénierie |
| **Host Company** | b4ps — Payment Solutions Integrator (KAL & GRGBanking partner) |
| **Project Type** | Projet de Fin d'Études (PFE) — Final Year Engineering Project |
| **Year** | 2025–2026 |

b4ps specializes in deploying and maintaining ATM management systems for banks. Their fleet spans multiple ATM vendors (NCR, Diebold, GRG, etc.) and was previously managed through a desktop-only, vendor-specific interface that did not scale well in multi-operator environments.

### Problem Statement

| Pain Point | Impact |
|---|---|
| Desktop-only installation required | Blocks remote monitoring; high IT maintenance cost |
| No unified multi-vendor view | Operators switch between tools per vendor |
| No integrated analytics | Manual reporting, delayed incident detection |
| Legacy UI/UX | High learning curve, low operator efficiency |

### Solution

A zero-installation, browser-based platform that aggregates ATM status data in real time, embeds Grafana analytics dashboards, and delivers instant alerts through SignalR — accessible from any device without software deployment.

<!-- Replace with actual demo GIF -->
> 📸 **Demo:** `docs/assets/demo.gif` *(add your recording here)*

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 🛠 Tech Stack

### Frontend
| Technology | Purpose |
|---|---|
| [![Angular](https://img.shields.io/badge/Angular_19-DD0031?logo=angular&logoColor=white)](https://angular.io/) | SPA framework (standalone components) |
| [![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/) | Type-safe application logic |
| [![SignalR Client](https://img.shields.io/badge/@microsoft/signalr-512BD4?logo=dotnet&logoColor=white)](https://www.npmjs.com/package/@microsoft/signalr) | Real-time push notifications |
| [![Grafana](https://img.shields.io/badge/Grafana_Embed-F46800?logo=grafana&logoColor=white)](https://grafana.com/) | Embedded monitoring dashboards |

### Backend
| Technology | Purpose |
|---|---|
| [![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/) | REST API & business logic |
| [![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet) | Web API framework |
| [![SignalR](https://img.shields.io/badge/SignalR_Hub-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/signalr) | Real-time WebSocket server |
| [![TCP Sockets](https://img.shields.io/badge/TCP_Sockets-C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/api/system.net.sockets) | ATM communication layer |
| [![Active Directory](https://img.shields.io/badge/Active_Directory-0078D4?logo=microsoft&logoColor=white)](https://learn.microsoft.com/windows-server/identity/ad-ds/) | Authentication / LDAP |

### Database & Monitoring
| Technology | Purpose |
|---|---|
| [![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server) | Primary relational database |
| [![Grafana](https://img.shields.io/badge/Grafana-F46800?logo=grafana&logoColor=white)](https://grafana.com/) | Metrics visualization & dashboards |

### Testing & Tooling
| Technology | Purpose |
|---|---|
| [![Kalignite SIM Central](https://img.shields.io/badge/Kalignite_SIM_Central-ATM_Simulator-0052CC)](https://www.kal.com/) | ATM device simulation |
| [![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/) | API documentation & testing |
| [![xUnit](https://img.shields.io/badge/xUnit-Integration_Tests-239120?logo=dotnet&logoColor=white)](https://xunit.net/) | Backend integration tests |

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 🏗 Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                            │
│              Angular SPA (Browser — Zero Install)               │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐ │
│   │  ATM Monitor │  │   Grafana    │  │  Incident / Ticket   │ │
│   │  Dashboard   │  │  Embedded    │  │     Management       │ │
│   └──────┬───────┘  └──────┬───────┘  └──────────┬───────────┘ │
│          │  HTTP/SignalR    │  iframe               │  HTTP      │
└──────────┼─────────────────┼──────────────────────┼────────────┘
           │                 │                        │
┌──────────▼─────────────────▼────────────────────────▼──────────┐
│                       API LAYER (.NET 8)                        │
│  ┌────────────┐  ┌──────────────┐  ┌──────────┐  ┌──────────┐  │
│  │    REST    │  │  SignalR Hub │  │   Auth   │  │ Campaign │  │
│  │ Controllers│  │  (KtcMonitor)│  │   (AD)   │  │  Mgmt    │  │
│  └────────────┘  └──────────────┘  └──────────┘  └──────────┘  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │               Service & Repository Layer                   │ │
│  └────────────────────────────────────────────────────────────┘ │
│  ┌──────────────────────────┐                                   │
│  │    TCP Socket Manager    │  ← ATM Communication              │
│  └──────────────┬───────────┘                                   │
└─────────────────┼───────────────────────────────────────────────┘
                  │ TCP/IP
┌─────────────────▼───────────────────────────────────────────────┐
│                       ATM FLEET (KAL / XFS)                     │
│         NCR │ Diebold │ GRGBanking │ Other vendors              │
└─────────────────────────────────────────────────────────────────┘
```

<!-- Replace with your actual architecture diagram -->
> 📐 Full diagram: `docs/assets/architecture.png`

### Layer Descriptions

| Layer | Responsibility |
|---|---|
| **Angular SPA** | Renders the operator UI; consumes REST APIs and listens to SignalR events |
| **REST Controllers** | Expose CRUD endpoints for ATMs, groups, campaigns, tickets, NOC dashboard |
| **SignalR Hub** | Broadcasts real-time ATM status changes to all connected clients |
| **TCP Socket Layer** | Maintains persistent connections to each ATM; parses XFS/KAL protocol messages |
| **Service Layer** | Orchestrates business logic between repositories and external services |
| **SQL Server** | Stores ATM configuration, historical status, counters, incidents, campaigns |
| **Grafana** | Reads directly from SQL Server; dashboards embedded in the Angular UI via iframe |

### ATM Communication Flow

```
ATM Device  ──TCP──►  AtmApplicationService  ──►  SignalR Hub  ──WebSocket──►  Browser
                              │                         │
                              ▼                         ▼
                         SQL Server              Angular State
```

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## ✨ Features

- 🖥️ **Real-Time ATM Monitoring** — Live status updates for the full ATM fleet via SignalR, with per-device state (In Service, Out of Service, Warning, Unknown)
- 🏢 **Multi-Vendor Support** — Unified view across NCR, Diebold, GRGBanking, and other XFS-compatible ATMs
- 📊 **Grafana Dashboards** — Embedded analytics dashboards for transaction volumes, uptime KPIs, and cash cassette levels
- 💬 **Incident & Ticket Management** — Create, search, and track incidents with full history and search filters
- 💰 **Cash Cassette Tracking** — Monitor cash levels and cassette status per ATM
- 📢 **Campaign Management** — Configure and deploy on-screen marketing campaigns to ATM fleets
- 👥 **Group & Region Management** — Organize ATMs into logical groups, branches, regions, and businesses
- 🔐 **Active Directory Authentication** — Single Sign-On via corporate AD/LDAP; role-based access control
- 🔔 **Real-Time Alerts** — Push notifications for critical ATM events delivered instantly to all connected operators
- 🌐 **Zero-Installation** — Runs entirely in the browser; no client software to install or maintain
- 📋 **NOC Dashboard** — Dedicated Network Operations Center view with fleet health summary

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 📦 Prerequisites

Ensure the following are installed before setting up the project:

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18+ LTS | https://nodejs.org/ |
| npm | 9+ | *(bundled with Node.js)* |
| Angular CLI | 19+ | `npm install -g @angular/cli` |
| SQL Server | 2019+ | https://www.microsoft.com/sql-server |
| Grafana | 10+ | https://grafana.com/grafana/download |

> **Optional:** Kalignite SIM Central (ATM simulator) — required only for end-to-end ATM protocol testing.

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 🚀 Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/<your-username>/ATM-Supervision-Platform.git
cd ATM-Supervision-Platform
```

---

### 2. Backend Setup (.NET 8)

```bash
cd Backend
```

**Configure application settings:**

Copy and edit the development settings file:

```bash
cp appsettings.Development.json.example appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KtcMonitoringDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "KtcWeb",
    "Audience": "KtcWebClient"
  },
  "ActiveDirectory": {
    "Domain": "your.domain.local",
    "LdapPath": "LDAP://your.domain.local"
  },
  "Grafana": {
    "BaseUrl": "http://localhost:3000"
  }
}
```

**Apply database migrations:**

```bash
dotnet ef database update
```

**Run the API:**

```bash
dotnet run
```

The API will be available at `https://localhost:7xxx` (port shown in console output).

---

### 3. Frontend Setup (Angular)

```bash
cd ktc-frontend
npm install
```

**Configure the proxy** (for local development, `proxy.conf.js` is pre-configured):

```bash
# Verify proxy.conf.js points to your backend URL
# Default: http://localhost:5000 or https://localhost:7xxx
```

**Run the development server:**

```bash
ng serve
```

The application will be available at `http://localhost:4200`.

---

### 4. Database Setup

```bash
# Connect to SQL Server and run migrations (already handled by dotnet ef above)
# To seed initial data (if a seed script is provided):
sqlcmd -S localhost -d KtcMonitoringDb -i docs/sql/seed.sql
```

---

### 5. Grafana Setup

**Start Grafana** (or use the bundled `grafana.ini` at the project root):

```bash
# Using the project grafana.ini
grafana-server --config ./grafana.ini
```

**Import the dashboard:**

1. Open Grafana at `http://localhost:3000` (default credentials: `admin` / `admin`)
2. Go to **Dashboards → Import**
3. Upload `docs/grafana/atm-dashboard.json`
4. Select your SQL Server data source and click **Import**

---

### 6. Running the Full Stack

Open three terminals:

```bash
# Terminal 1 — Backend
cd Backend && dotnet run

# Terminal 2 — Frontend
cd ktc-frontend && ng serve

# Terminal 3 — Grafana (if not running as a service)
grafana-server --config ./grafana.ini
```

Navigate to `http://localhost:4200` and log in with your Active Directory credentials.

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 📁 Project Structure

```
ATM-Supervision-Platform/
│
├── Backend/                          # .NET 8 Web API
│   ├── Controllers/                  # REST API endpoints
│   │   ├── AtmController.cs
│   │   ├── AuthController.cs
│   │   ├── CampaignController.cs
│   │   ├── CashCassetteController.cs
│   │   ├── GroupController.cs
│   │   ├── NocDashboardController.cs
│   │   └── TicketController.cs
│   ├── DTOs/                         # Request / Response models
│   ├── Hubs/
│   │   └── KtcMonitoringHub.cs       # SignalR hub
│   ├── Infrastructure/               # Cross-cutting concerns
│   ├── Middleware/                   # Custom ASP.NET middleware
│   ├── Models/                       # Domain entities
│   │   └── Monitoring/
│   ├── Repositories/                 # Data access layer
│   │   ├── Interfaces/
│   │   └── Implementations/
│   ├── Services/                     # Business logic
│   │   ├── Interfaces/
│   │   └── Implementations/
│   │       ├── AtmApplicationService.cs   # TCP ↔ ATM communication
│   │       ├── ActiveDirectoryService.cs
│   │       └── ...
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
├── Backend.IntegrationTests/         # xUnit integration test suite
│
├── ktc-frontend/                     # Angular 19 SPA
│   └── src/
│       └── app/
│           ├── core/
│           │   ├── guards/           # Route guards
│           │   ├── interceptors/     # HTTP interceptors
│           │   └── services/         # Singleton services
│           ├── features/             # Feature modules (standalone)
│           │   ├── atm/              # ATM monitoring views
│           │   ├── auth/             # Login / authentication
│           │   ├── campaign/         # Campaign management
│           │   ├── dashboard/        # NOC dashboard
│           │   ├── group/            # Group management
│           │   ├── ticket-search/    # Incident search
│           │   ├── admin/
│           │   ├── branch/
│           │   ├── business/
│           │   └── region/
│           └── shared/               # Reusable components & pipes
│
├── grafana.ini                       # Grafana configuration
├── docs/
│   ├── assets/                       # Screenshots, banner, diagrams
│   ├── grafana/                      # Dashboard JSON exports
│   └── sql/                          # Seed scripts
├── KtcWeb.sln                        # Visual Studio solution file
└── README.md
```

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 📖 API Documentation

The backend exposes a Swagger UI for interactive API exploration.

**Swagger UI:** `https://localhost:<port>/swagger`

### Key Endpoints

| Module | Method | Endpoint | Description |
|---|---|---|---|
| **Auth** | `POST` | `/api/auth/login` | Authenticate via Active Directory |
| **ATM** | `GET` | `/api/atm` | List all ATMs with current status |
| **ATM** | `GET` | `/api/atm/{id}` | Get single ATM detail |
| **NOC Dashboard** | `GET` | `/api/nocdashboard` | Fleet health summary |
| **Cash Cassette** | `GET` | `/api/cashcassette/{atmId}` | Cassette levels for an ATM |
| **Campaign** | `GET/POST` | `/api/campaign` | List / create marketing campaigns |
| **Group** | `GET/POST` | `/api/group` | Manage ATM groups |
| **Tickets** | `GET/POST` | `/api/ticket` | Incident management |

> All protected endpoints require a valid JWT Bearer token obtained from `/api/auth/login`.

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 📸 Screenshots

> *(Replace placeholders below with actual screenshots)*

### Login Page
![Login](docs/assets/screenshots/login.png)

### NOC Dashboard
![NOC Dashboard](docs/assets/screenshots/noc-dashboard.png)

### Real-Time ATM Monitoring
![ATM Monitor](docs/assets/screenshots/atm-monitor.png)

### Grafana Analytics Dashboard
![Grafana](docs/assets/screenshots/grafana-dashboard.png)

### Incident / Ticket Management
![Tickets](docs/assets/screenshots/ticket-management.png)

### Campaign Management
![Campaigns](docs/assets/screenshots/campaigns.png)

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 🗺 Roadmap

- [x] Real-time ATM status monitoring via SignalR
- [x] Multi-vendor TCP connection management
- [x] NOC dashboard with fleet health summary
- [x] Grafana dashboard integration
- [x] Cash cassette tracking
- [x] Campaign management
- [x] Active Directory authentication
- [x] Incident / ticket management
- [x] Group & region organization
- [ ] Interactive ATM map view (geographic layout)
- [ ] Email / SMS alert notifications
- [ ] Role-based dashboard customization
- [ ] Export reports to PDF / Excel
- [ ] Mobile-responsive layout improvements
- [ ] Multi-language support (FR / EN / AR)

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 🤝 Contributing

This project is an academic PFE submission. External contributions are not expected, but feedback is welcome.

If you wish to fork and build on this work:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add: your feature description'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

Please follow the existing code style and ensure the integration test suite passes before submitting.

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 🙏 Acknowledgements

This project would not have been possible without the guidance and support of:

| Name | Role |
|---|---|
| **M. Barboucha Mohammed** | Company Supervisor — b4ps |
| **M. Chikhaoui Saad** | Academic Supervisor — EHEI |
| **M. Ismail Zin El Abedine** | Technical Lead — b4ps |
| **M. Mohamed Ouhami** | Technical Collaborator — b4ps |

Special thanks to the b4ps team for providing access to the KAL environment, ATM simulators, and production architecture documentation.

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 📬 Contact

**Soufiane [Your Last Name]**

[![Email](https://img.shields.io/badge/Email-soufianexelotmani@gmail.com-D14836?style=flat&logo=gmail&logoColor=white)](mailto:soufianexelotmani@gmail.com)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-your--profile-0077B5?style=flat&logo=linkedin&logoColor=white)](https://linkedin.com/in/your-profile)
[![GitHub](https://img.shields.io/badge/GitHub-your--username-181717?style=flat&logo=github&logoColor=white)](https://github.com/your-username)

> *EHEI — École des Hautes Études d'Ingénierie | Promotion 2026*

<p align="right"><a href="#-table-of-contents">↑ Back to top</a></p>

---

## 📄 License

Distributed under the MIT License. See [LICENSE](LICENSE) for more information.

---

<div align="center">
  <sub>Built with ❤️ at b4ps · EHEI Final Year Project 2025–2026</sub>
</div>
