# StockSync

StockSync is a **cloud-native, microservices-based inventory management system** built with **ASP.NET Core** and orchestrated using **ASP.NET Core Aspire** – Microsoft’s official stack for distributed .NET applications.

This demonstrates modern development practices including service orchestration, observability, resilience, caching, background processing, and containerization.

## Architecture Overview

- **Supplier Service** (.NET Web API)  
  Handles supplier management, JWT authentication/authorization, Hangfire background jobs, and caching.  
  Database: **Microsoft SQL Server** (via Entity Framework Core)

- **Item Service** (.NET Web API)  
  Manages inventory items with flexible querying.  
  Database: **MongoDB**

- **Shared Library**  
  Contains common models, DTOs, user roles, middleware (Serilog logging, global exception handling)

- **Orchestration**  
  **ASP.NET Core Aspire** (AppHost + ServiceDefaults)  
  Provides service discovery, automatic connection string injection, health checks, OpenTelemetry, and a beautiful dashboard.

- **Supporting Services**  
  - Redis (distributed caching & optional Redis Commander UI)  
  - Hangfire (background jobs with SQL Server storage)

## Key Features

- Microservices architecture with clean separation of concerns
- JWT-based authentication and role-based authorization (`Admin` / `User`)
- Centralized request/response logging with **Serilog**
- Global exception handling
- Supplier → Item relationship with **cached Item details** for fast reads
- **Hangfire** scheduled jobs for periodic supplier-item synchronization
- **In-memory + optional Redis distributed caching**
- Full **observability** via OpenTelemetry (traces, metrics, logs)
- Automatic health checks and resilience policies (via Aspire ServiceDefaults)
- Docker containerization
- **Aspire Dashboard** for live monitoring of all resources

## Getting Started (Local Development with Aspire)

### Prerequisites

- .NET 9 SDK
- Docker Desktop (running in the background – Aspire will start containers automatically)

### Run the Entire Application

```bash
git clone https://github.com/MadurangaNamal/StockSync.git
cd StockSync

# Run the Aspire AppHost (orchestrates everything)
dotnet run --project StockSync.AppHost