# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ASP.NET Core 10 Web API for the nutrition platform. Production-ready with Swagger, repository pattern, and Azure Cosmos DB (MongoDB API) support. Deployed to Azure App Services `wireservers-food-api-dev` (dev) and `wireservers-food-api` (prod).

## Commands

```bash
dotnet restore
dotnet build
dotnet run             # HTTP :5000 / HTTPS :7000
dotnet test            # No tests yet; creates BringTheDiet.Tests.csproj when ready
```

Swagger UI is available at `/swagger` when running.

**VS Code tasks** (`build`, `publish`, `watch`) reference paths relative to the **workspace root** (`/Users/toddclarkston/source/bringthe/`), not this directory.

## Environment Setup

`.env` in this directory or its parent directory. Environment variables override `appsettings.{Environment}.json`. Required:

```bash
MONGO_URI="mongodb+srv://{user}:{pass}@{cluster}.global.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000"
DB_NAME="foods-test"
```

Collection names are configured under `MongoDB.Collections.*` in `appsettings.json`.

**Cosmos DB requirement:** Connection string must include `retrywrites=false`.

## Architecture: Repository Pattern

`Controllers` → `Services` → `Repositories` → `MongoDB.Driver`

Resources: Foods, Recipes, Users, MealPlans, Nutrients, Diets, AuditLogs, Roles, Permissions.

### Count Caching (critical pattern)

All repositories cache `countDocuments()` for 5 minutes to avoid expensive Cosmos DB RU consumption on paginated requests:

```csharp
private long? _cachedTotalCount;
private DateTime _countCacheUpdatedAt = DateTime.MinValue;
private readonly TimeSpan _countCacheTtl = TimeSpan.FromMinutes(5);
```

Create increments the cache by +1; Delete decrements by -1. The cache is **in-memory per process** — a restart triggers a live count on the next paginated request. Do not bypass this pattern when adding new repositories.

### Pagination

`?page=1&pageSize=20` (max 100). Uses cached count for total pages.

### Authentication

Currently **no auth enforcement** — BCrypt password hashing exists only for the Users collection. CORS is open for all origins. JWT/Azure AD authentication is planned as a future enhancement.

### Document Conventions

- `Id` properties use `[BsonRepresentation(BsonType.ObjectId)]`
- `CreatedAt`, `UpdatedAt` (DateTime) on all documents — no user tracking yet
- No soft delete (unlike Node.js API)

## CI/CD

- **`.github/workflows/develop_wireservers-food-api-dev.yml`** — builds and deploys to `wireservers-food-api-dev` on push to `develop` or manual dispatch
- **`.github/workflows/main_wireservers-food-api.yml`** — builds and deploys to `wireservers-food-api` on push to `main` or manual dispatch

Both use OIDC federation (no secrets stored in artifacts).

Required GitHub secrets (dev): `AZUREAPPSERVICE_CLIENTID_44EB82A5CC014AAE9E36591D673BC770`, `AZUREAPPSERVICE_TENANTID_4CCEE8B078FB41F58AC09876DC6777E5`, `AZUREAPPSERVICE_SUBSCRIPTIONID_81B48C5F90E54B60972F2F5A665EC84F`

Required GitHub secrets (prod): `AZUREAPPSERVICE_CLIENTID_A8533FA2F56D48EFA8F8DD96B56D0BC8`, `AZUREAPPSERVICE_TENANTID_439DC8A532FC420189E5280C50C9B696`, `AZUREAPPSERVICE_SUBSCRIPTIONID_407FB1216C6044F2A27A3D0CA61CE747`

### Environment Variables (Azure App Service)

Set on both App Services via Configuration → Application Settings:

- `MONGO_URI` — Cosmos DB connection string (required)
- `DB_NAME` — database name (`foods-test` for dev, `foods-production` for prod)
- `AZURE_AD_TENANT_ID` — Azure AD tenant ID
- `AZURE_AD_CLIENT_ID` — Azure AD client/application ID
- `AZURE_AD_AUDIENCE` — Azure AD audience URI
