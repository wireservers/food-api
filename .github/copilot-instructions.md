# Bring The Diet - AI Coding Agent Instructions

## Repository Overview

**Dual-Architecture Nutrition Platform:** This workspace contains two parallel implementations of a food/nutrition API platform:

1. **Node.js Monorepo** (`bring-the-diet/`): pnpm workspace with Web (+ Admin module)/Mobile apps + Express API
2. **.NET 10 API** (`food-api/`): Production-ready ASP.NET Core Web API

Both use MongoDB (Azure Cosmos DB compatible) but serve different deployment scenarios and client needs.

## Project Structure

```
├── bring-the-diet/              # Node.js monorepo (pnpm workspace)
│   ├── web/                     # Next.js site (port 3001)
│   │   ├── app/(public)/        # Public routes (/, /recipes, /diets, etc.)
│   │   └── app/admin/           # Admin console routes (/admin/*)
│   ├── mobile/                  # Expo React Native app
│   ├── api/                     # Express + TypeScript API (port 3000)
│   └── packages/
│       ├── shared/              # Shared types (ApiUser, DietType, etc.)
│       └── ui/                  # Shared UI primitives
└── food-api/                    # .NET 10 Web API (ports 5000/7000)
```

## Critical Development Workflows

### Node.js Monorepo Setup

```bash
cd bring-the-diet/
cp .env.example .env           # Configure MONGO_URI + OIDC settings
pnpm i                         # Install all workspace dependencies
pnpm dev                       # Run all apps in parallel
```

**Environment File Location:** `.env` lives at **monorepo root**, but the API loads it from parent directory:
```typescript
// api/src/index.ts
dotenv.config({ path: path.resolve(process.cwd(), "..", ".env") });
```

**Required Environment Variables:**
- `MONGO_URI`: MongoDB connection string
- `DB_NAME`: Database name (e.g., `nutrition`)
- `DB_FOODS_COLLECTION`, `DB_RECIPES_COLLECTION`, etc. (collection names)
- `OIDC_AUTHORITY`: Identity provider URL (e.g., `https://login.microsoftonline.com/{tenant}`)
- `OIDC_AUDIENCE`: API audience/client ID
- `ALLOW_DEV_USER_HEADER=true`: Enables `X-Dev-User: todd` header for local development (bypasses OIDC)

### .NET API Setup

```bash
cd food-api/
dotnet restore
dotnet build
dotnet run                     # Runs on ports 5000 (HTTP) and 7000 (HTTPS)
```

**Environment Priority:** `.env` variables override `appsettings.{Environment}.json`:
- Searches current directory for `.env`, then parent directory
- `MONGO_URI` and `DB_NAME` from env vars take precedence over appsettings

**VS Code Tasks (from workspace root):**
- `build`: Compiles `api/bring-the-diet.sln`
- `publish`: Publishes release artifacts
- `watch`: Runs API with hot reload

**Important:** VS Code tasks reference paths relative to **workspace root** (`/Users/toddclarkston/source/bringthe/`), not the `food-api/` subdirectory.

## Architecture Patterns

### Node.js API: CRUD Factory Pattern

The Express API auto-generates CRUD routes from configuration using a **factory function**:

```typescript
// bring-the-diet/api/src/routes/crud-factory.ts
const collections: CollectionConfig[] = [
  {
    name: "foods",
    envVar: "DB_FOODS_COLLECTION",
    permissions: { read: "foods:read", write: "foods:write", delete: "foods:delete" }
  }
];

for (const config of collections) {
  const router = createCrudRouter(config);  // Generates GET/POST/PUT/DELETE
  if (config.name === "recipes" || config.name === "blog-posts") {
    addPublishRoute(router, config);        // Adds POST /:id/publish
  }
  app.use(`/api/${config.name}`, router);
}
```

**Key Behaviors:**
- **Soft Delete:** All routes filter `{ deletedAt: { $exists: false } }`. Delete sets `deletedAt`, `deletedBy`.
- **Audit Fields:** Auto-populated on create/update: `createdAt`, `createdBy`, `updatedAt`, `updatedBy`
- **Pagination:** `?page=1&limit=20` (max 100 items per page)
- **ObjectId Handling:** Tries `new ObjectId(id)` first, falls back to string ID
- **Auth:** All routes protected by `authMiddleware` + `requirePermission()`

### .NET API: Repository Pattern with Count Caching

Repositories use **aggressive count caching** to avoid slow MongoDB `countDocuments()` calls:

```csharp
// food-api/Repositories/FoodRepository.cs
private long? _cachedTotalCount;
private DateTime _countCacheUpdatedAt = DateTime.MinValue;
private readonly TimeSpan _countCacheTtl = TimeSpan.FromMinutes(5);

// Cache adjusted on Create (+1) and Delete (-1)
private void AdjustCachedCount(long delta) {
    lock (_countCacheSync) {
        _cachedTotalCount = Math.Max(0, _cachedTotalCount.Value + delta);
    }
}
```

**Pattern applies to:** `FoodRepository`, `RecipeRepository`, `UserRepository`, `MealPlanRepository`

**Why:** MongoDB count operations can be slow on large collections; 5-minute cached count + delta adjustments optimize pagination.

### Authentication Strategies

**Node.js API (Production):**
1. **OIDC JWT Validation:** Uses `jwks-rsa` to validate tokens from `OIDC_AUTHORITY`
2. **Dev Mode Escape Hatch:** If `ALLOW_DEV_USER_HEADER=true`, accepts `X-Dev-User: todd` header (grants `*` permissions)

```typescript
// bring-the-diet/api/src/middleware/auth.ts
if (process.env.ALLOW_DEV_USER_HEADER === "true") {
  const devUser = req.headers["x-dev-user"];
  if (typeof devUser === "string") {
    req.user = { id: `dev-${devUser}`, roles: ["admin"], permissions: ["*"] };
  }
}
```

**.NET API:**
- Currently **no authentication** (see [food-api/README.md](food-api/README.md#L100) "Future Enhancements")
- Password hashing with BCrypt for `Users` collection
- CORS enabled for all origins (configure for production)

## Shared Types & Monorepo Conventions

**pnpm Workspace References:** Use `workspace:*` protocol in `package.json`:
```json
{
  "dependencies": {
    "@nutri/shared": "workspace:*",
    "@nutri/ui": "workspace:*"
  }
}
```

**Shared Types:** [packages/shared/src/index.ts](bring-the-diet/packages/shared/src/index.ts) defines platform-wide types:
- `ApiUser`, `UserRole`, `DietType`, `ID`
- `DIET_TYPES` const array (keto, paleo, mediterranean, vegan, etc.)

**Module Resolution:** All TypeScript packages use `"module": "NodeNext"` for ESM. Import statements require `.js` extensions:
```typescript
import { getDb } from "../db/mongo.js";  // Note .js extension
```

## Database Conventions

### Collection Names (Configurable via ENV)

**Node.js API (defaults):**
- `foundationfoods` (foods)
- `nutritionfacts`
- `recipes`
- `diettypes`
- `blogposts`
- `mealplans`
- `users`
- `roles`
- `permissions`
- `auditlogs`

**.NET API (defaults):** Same collection names configured in `appsettings.json` under `MongoDB.Collections.*`

### Document Patterns

**Soft Delete (Node.js only):**
- Find filter: `{ deletedAt: { $exists: false } }`
- Delete operation: `{ $set: { deletedAt: new Date(), deletedBy: user.id } }`

**Audit Fields:**
- **Node.js:** `createdAt`, `createdBy`, `updatedAt`, `updatedBy` (ISO date + user ID)
- **.NET:** `CreatedAt`, `UpdatedAt` (DateTime, no user tracking yet)

**ObjectId Handling:**
- **Node.js:** Try `new ObjectId(id)` first, fall back to string ID if invalid
- **.NET:** `[BsonRepresentation(BsonType.ObjectId)]` on `Id` properties

## Common Pitfalls & Solutions

1. **Environment File Location (Node.js):** API loads `.env` from **parent directory** of `api/` (monorepo root), not from `api/` itself
2. **Port Conflicts:** Web (3001), API (3000) – ensure ports aren't already bound
3. **MongoDB Connection:** Both APIs default to empty `MONGO_URI` – **must configure before running**
4. **OIDC Not Configured:** Node.js API rejects requests unless `ALLOW_DEV_USER_HEADER=true` or valid OIDC config provided
5. **pnpm Required:** Don't use npm/yarn in `bring-the-diet/` – workspace protocol requires pnpm (`packageManager: "pnpm@10.28.0"`)
6. **.NET Path Confusion:** VS Code tasks reference `api/` relative to **workspace root** (`/Users/toddclarkston/source/bringthe/`), not `food-api/` subdirectory
7. **ESM Import Extensions:** TypeScript files must import with `.js` extensions (NodeNext resolution)
8. **Cosmos DB Connection:** Must include `retrywrites=false` in connection string (Cosmos DB limitation)
9. **ObjectId vs String IDs:** Node.js API tries ObjectId first, .NET uses BsonRepresentation – ensure consistency
10. **Soft Delete Filtering:** Always include `{ deletedAt: { $exists: false } }` in Node.js queries or get deleted records

## Environment Configuration Reference

### Development (.env file)

**Location:**
- Node.js: `bring-the-diet/.env` (monorepo root)
- .NET: `food-api/.env` or parent directory

**Required Variables:**
```bash
# Database (both APIs)
MONGO_URI="mongodb+srv://wsadmin:{password}@ws-cloud-mongo.global.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000"
DB_NAME="foods-test"  # Use different names for dev/test/prod

# Collections (both APIs)
DB_FOODS_COLLECTION="foundationfoods"
DB_NUTRITION_COLLECTION="nutritionfacts"
DB_RECIPES_COLLECTION="recipes"
DB_DIETS_COLLECTION="diettypes"
DB_BLOG_COLLECTION="blogposts"
DB_MEALPLANS_COLLECTION="mealplans"
DB_USERS_COLLECTION="users"
DB_ROLES_COLLECTION="roles"
DB_PERMISSIONS_COLLECTION="permissions"
DB_AUDIT_COLLECTION="auditlogs"

# Node.js API only
PORT=3000
NODE_ENV="development"
OIDC_AUTHORITY="https://login.microsoftonline.com/{tenant-id}/v2.0"
OIDC_AUDIENCE="api://{client-id}"
ALLOW_DEV_USER_HEADER=true  # Set to false in production!
```

### Production (Azure App Service)

Set environment variables in Azure Portal → Configuration → Application Settings:
- Never commit `.env` to source control
- Use Key Vault references for sensitive values: `@Microsoft.KeyVault(SecretUri=...)`
- Override `ALLOW_DEV_USER_HEADER` to `false` or omit entirely

## Key Files to Reference

- **CRUD Factory:** [bring-the-diet/api/src/routes/crud-factory.ts](bring-the-diet/api/src/routes/crud-factory.ts) - Router generation logic
- **Auth Middleware:** [bring-the-diet/api/src/middleware/auth.ts](bring-the-diet/api/src/middleware/auth.ts) - OIDC + dev mode
- **.NET Startup:** [food-api/Program.cs](food-api/Program.cs) - DI setup, MongoDB client config
- **Repository Example:** [food-api/Repositories/FoodRepository.cs](food-api/Repositories/FoodRepository.cs) - Count caching pattern
- **Shared Types:** [bring-the-diet/packages/shared/src/index.ts](bring-the-diet/packages/shared/src/index.ts) - Platform-wide type definitions
- **API Examples:** [food-api/API_EXAMPLES.md](food-api/API_EXAMPLES.md) - Example HTTP requests with pagination

## Testing & CI/CD

### Testing Strategy

**Node.js Monorepo:**
- **Framework:** To be implemented (Jest recommended for API, Vitest for apps)
- **Test Types:**
  - Unit tests for API middleware (auth, CRUD factory logic)
  - Integration tests for MongoDB operations
  - E2E tests for critical user flows (recipe creation, meal planning)
- **Run:** `pnpm test` (when implemented)
- **Coverage:** Target 80%+ for API routes and business logic

**.NET API:**
- **Framework:** Built-in `dotnet test` with xUnit/NUnit
- **Test Types:**
  - Unit tests for repositories (mock MongoDB driver)
  - Integration tests with MongoDB test container
  - Controller tests for API endpoints
- **Run:** `dotnet test food-api/BringTheDiet.Api.csproj`
- **Status:** No test files present yet (create `BringTheDiet.Tests.csproj`)
- **Patterns:**
  - Mock `IMongoCollection` for unit tests
  - Use `[Theory]` with `[InlineData]` for parameterized tests
  - Test count cache behavior (create/delete adjustments)

### CI/CD Workflows

**GitHub Actions (configured):**

1. **`.github/workflows/dotnet-build.yml`**
   - Triggers: Push/PR to `main`/`dev` (food-api path changes)
   - Steps: Restore → Build → Test (Release config)
   - Continues on test failures (no tests yet)

2. **`.github/workflows/node-build.yml`**
   - Triggers: Push/PR to `main`/`dev` (bring-the-diet path changes)
   - Steps: pnpm install → typecheck → lint → build
   - Uses pnpm@10.28.0 with cache optimization

3. **`.github/workflows/azure-deploy-dotnet.yml`**
   - Triggers: Push to `dev` branch (food-api changes) + manual dispatch
   - Target: Azure App Service `bring-the-diet-api-dev`
   - Auth: OIDC federation (no secrets in artifacts)
   - Steps: Build → Publish → Azure login → Deploy

**Required GitHub Secrets (for Azure deploy):**
- `AZURE_CLIENT_ID`: Service principal client ID
- `AZURE_TENANT_ID`: Microsoft Entra tenant ID
- `AZURE_SUBSCRIPTION_ID`: Target subscription ID

### Azure Deployment

**.NET API Configuration:**
- **App Service Name:** `bring-the-diet-api-dev`
- **Runtime:** .NET 10 on Linux
- **Region:** (configure in Azure Portal)
- **Pricing Tier:** B1 Basic or higher (for production workloads)
- **Environment Variables (set in Azure):**
  - `MONGO_URI`: Azure Cosmos DB connection string
    - Format: `mongodb+srv://{user}:{pass}@{cluster}.global.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000`
  - `DB_NAME`: Database name (e.g., `foods-test`, `foods-prod`)
  - Collection variables: `DB_FOODS_COLLECTION`, `DB_RECIPES_COLLECTION`, etc.
- **Deployment Slots:** Use staging slot for blue/green deployments
- **Health Check:** Configure `/health` endpoint in Azure Portal
- **Logging:** Enable Application Insights for diagnostics

**Azure Cosmos DB for MongoDB:**
- **Connection String Pattern:** Includes `retrywrites=false` (Cosmos requirement)
- **Auth Mechanism:** SCRAM-SHA-256
- **Performance Optimization:**
  - Repository count caching reduces RU consumption
  - Index on `deletedAt` field for soft delete queries
  - Compound indexes on frequently queried fields
- **Scaling:** Configure RU/s in Azure Portal (400 minimum for dev)

**Monitoring & Diagnostics:**
- **Application Insights:** Automatic exception tracking, request metrics
- **Log Stream:** Real-time logs via Azure Portal or `az webapp log tail`
- **Kudu Console:** Access at `https://bring-the-diet-api-dev.scm.azurewebsites.net`

## Next Steps (per Roadmaps)

1. Implement RBAC persistence (map OIDC `sub` → user roles/permissions)
2. Wire Admin tables to API (pagination, filtering, sorting)
3. Add rich UX for recipe/diet/meal plan details
4. Implement SEO, caching, observability
5. Add JWT authentication to .NET API
6. Add comprehensive test suites to both APIs
