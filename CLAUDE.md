# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ASP.NET Core 10 Web API for the nutrition platform. Production-ready with Swagger, repository pattern, and Azure Cosmos DB (MongoDB API) support. Deployed to Azure App Service `bring-the-diet-api-dev`.

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

Resources: Foods, Recipes, Users, MealPlans, Nutrients, Diets, BlogPosts, AuditLogs, Roles, Permissions.

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

- **`.github/workflows/dotnet-build.yml`** — build + test on push/PR to `main`/`dev`
- **`.github/workflows/azure-deploy-dotnet.yml`** — deploys to `bring-the-diet-api-dev` on push to `dev` branch or manual dispatch via OIDC federation (no secrets stored in artifacts)

Required GitHub secrets: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`.
