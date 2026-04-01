# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Start the PostgreSQL database
docker compose up -d

# Run the API (development, with hot reload)
dotnet watch run

# Build
dotnet build Finrex-App.csproj

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --output-dir Infra/Data/Migrations

# Apply pending migrations
dotnet ef database update
```

The API runs on `http://localhost:5023` with Swagger at `/swagger`.

## Architecture

This is a **Clean Architecture** ASP.NET Core 8 Web API for personal finance management, backed by PostgreSQL.

**Layer structure:**
- `Domain/Entities/` — Plain entity classes (`User`, `MonthlyIncome`, `MonthlySpending`, `MFinanceFactors`). No dependencies.
- `Application/` — Business logic: services (`Services/`), DTOs (`DTOs/Requests|Responses/`), FluentValidation validators (`Validators/`), financial calculation helpers (`Helpers/`), and JWT generation (`JwtGenerate/`).
- `Infra/Api/Controllers/` — Four controllers (login/auth, transactions, reports, finance factors). All return `ApiResponse<T>`.
- `Infra/Data/` — `AppDbContext` (EF Core code-first) and migrations.
- `Extensions/` — DI wiring (`ServiceCollectionExtensions.cs`) and JWT claims helpers (`ClaimsExtensions.cs`).

**API versioning:** All routes are prefixed `/api/v1.0/`.

**Standard response shape:** Every endpoint returns `ApiResponse<T>` with `Success`, `Data`, `Message`, and `Errors` fields.

## Authentication

Dual-auth system:
1. **JWT Bearer** — HS256 tokens, valid 8 hours, stored as HTTP-only cookie (`finrex.auth`) and/or `Authorization: Bearer` header.
2. **Google OAuth 2.0** — Initiates at `GET /google-login`, callback at `GET /google-signin-callback`; auto-creates user on first login. On success, redirects to `{FrontendBaseUrl}/insights?token={token}`.

Protected endpoints use `[Authorize]`. The authenticated user's ID is extracted via `User.GetUserId()` (from `Extensions/ClaimsExtensions.cs`).

## Database

- **ORM:** Entity Framework Core 8 (Npgsql provider)
- **DB:** PostgreSQL 14 (local: `localhost:5432`, db `finrexdb`, user/pass `postgres/postgres`)
- `DateOnly` fields use a custom value converter for PostgreSQL compatibility (configured in `AppDbContext`).
- All income/spending/factor records belong to a `User` via `UsuarioId` FK.

## Key Patterns

- **Validation:** FluentValidation validators are auto-registered and run via the error handling middleware (`Infra/Api/Middleware/ErrorHandlingMiddleware.cs`), which catches `ValidationException` and formats it as `ApiResponse`.
- **Mapping:** Mapster is used for DTO ↔ entity mapping.
- **CORS:** Configured for `http://localhost:3000` with credentials.
- **No test project** exists in this repository yet.
