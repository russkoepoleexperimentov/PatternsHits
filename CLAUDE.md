# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**EduBank** is a microservices-based banking platform built with ASP.NET Core 9.0 and a React 19 frontend. It uses Domain-Driven Design, MassTransit (RabbitMQ), Entity Framework Core with PostgreSQL, and Duende IdentityServer.

## Build & Run Commands

### Full stack via Docker (recommended)
```bash
cd EduBank
docker-compose up
```
Services: AuthWeb `:2280`, Core `:2281`, Credit `:5001`, Currency `:5002`, Options `:5004`  
PostgreSQL `:1488` (user: `appuser`, pass: `123456`), RabbitMQ Management `:15673`

### Individual services
```bash
dotnet build EduBank/EduBank.sln
dotnet run --project EduBank/WebApplication1/AuthWeb.csproj
dotnet run --project EduBank/Core.Web/Core.Web.csproj
dotnet run --project CreditWeb/CreditWeb.csproj
dotnet run --project ConcurrencyService/ConcurrencyService.csproj
dotnet run --project OptionsService/OptionsService.csproj
```

### Database migrations
```bash
dotnet ef database update --project CreditWeb/CreditWeb.csproj
dotnet ef database update --project EduBank/Core.Web/Core.Web.csproj
```

### Frontend
```bash
cd frontend && npm install && npm start   # dev server on :3000
npm run build
```

## Architecture

The solution (`EduBank/EduBank.sln`) has five services, each with its own database:

| Service | Project root | Responsibility |
|---|---|---|
| **AuthWeb** | `EduBank/WebApplication1/` | Duende IdentityServer – OAuth 2.0 / OIDC token issuer, user management |
| **Core** | `EduBank/Core.Web/` | Accounts, transactions, WebSocket real-time updates, master account init |
| **Credit** | `CreditWeb/` | Loans, tariffs, payments, credit ratings, overdue detection (Quartz jobs) |
| **Currency** | `ConcurrencyService/` | Exchange rates, currency conversion, scheduled rate-update jobs |
| **Options** | `OptionsService/` | Financial options management |
| **Monitoring** | `MonitoringService/` | Receives traces from all services, serves web dashboard at `/` |

### Shared libraries (EduBank solution)
- `EduBank/Common/` – Middleware (`ExceptionCatchMiddleware`, `IdempotencyMiddleware`, `UnstableServiceMiddleware`), shared options/config
- `EduBank/Domain/` – `ApplicationUser` (ASP.NET Identity), `RefreshToken`

### Layer pattern (each service)
```
<Service>Domain/       – Entities only
<Service>Application/  – Services, DTOs, FluentValidation validators,
                         AutoMapper profiles, MassTransit consumers, Quartz jobs
<Service>Infrastructure/ – EF Core DbContext
<Service>Web/          – Controllers, Migrations, Program.cs
```

### Cross-service communication
- **Async**: MassTransit + RabbitMQ (`Consumers/` in Application layer)
- **Sync**: HTTP calls between services (JWT-authenticated)
- **Auth**: All non-auth services validate JWTs against AuthWeb (`:2280`)

### Resilience
- Polly retry + circuit-breaker policies are applied to HTTP clients
- `IdempotencyMiddleware` in Common deduplicates requests
- `UnstableServiceMiddleware` simulates instability for testing resilience patterns

### Frontend
React 19 + TypeScript, OIDC Client TS for OAuth login, Ant Design UI, React Router 7. Located in `frontend/`.

## Key Configuration Sections (appsettings.json per service)
- `ConnectionStrings` – Postgres connection (one DB per service)
- `RabbitMq` – broker host/credentials
- `Auth` – JWT authority/audience pointing to AuthWeb
- `CurrencyApi` – external ExchangeRate-API v6 key/base URL

## Tracing & Monitoring

Every service (except MonitoringService itself) sends a telemetry event to MonitoringService after each HTTP request via `TracingMiddleware` (in `Common/Middleware/`). The middleware is fire-and-forget — it never blocks the main request.

Key pieces in `Common`:
- `Common/Middleware/TracingMiddleware.cs` — captures TraceId, method, path, status, duration; skips `/swagger`
- `Common/Services/Implementations/TracingClient.cs` — singleton, HTTP POST to monitoring, swallows all errors
- `Common/Extensions/TracingExtensions.cs` — `services.AddTracing(config)` registers everything

Each service registers tracing with one line in `Program.cs`:
```csharp
builder.Services.AddTracing(builder.Configuration);
// and in the middleware pipeline (first):
app.UseMiddleware<TracingMiddleware>();
```

Config section in each service's `appsettings.json`:
```json
"Monitoring": { "ServiceName": "CoreService", "BaseUrl": "http://monitoringweb:8080" }
```

Dashboard: `http://localhost:5005` — auto-refreshes every 15 s, shows request rate, error %, response time charts, per-service summary table and recent traces list.

## No test projects exist in this repo currently.
