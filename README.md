# Food API

[![.NET](https://github.com/todd-ws/food-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/todd-ws/food-api/actions/workflows/dotnet.yml)

A comprehensive food and nutrition API repository featuring two implementations:
- **Bring The Diet API** - A .NET 10 Web API with CRUD operations for foods, recipes, users, and meal plans
- **Food API (Split Collections)** - A Node.js API with separate collections for foods and nutrients

## 📁 Repository Structure

This repository contains two distinct API implementations:

### 1. [Bring The Diet API](/api) (.NET 10)

A RESTful .NET 10 Web API with full CRUD operations using MongoDB (Azure Cosmos DB compatible).

**Features:**
- ✅ RESTful API with Foods, Recipes, Users, and Meal Plans endpoints
- ✅ MongoDB integration with repository pattern
- ✅ Swagger/OpenAPI documentation
- ✅ Password hashing with BCrypt
- ✅ Comprehensive pagination support
- ✅ Environment variable configuration

**Quick Start:**
```bash
cd api
dotnet restore
dotnet build
dotnet run
```

📖 [Full Documentation](/api/README.md) | 📝 [API Examples](/api/API_EXAMPLES.md)

### 2. [Food API (Split Collections)](/Azure%20Cosmos%20DB) (Node.js)

A Node.js implementation with separate collections for foods and nutrients, including data import scripts and E2E tests.

**Features:**
- ✅ Split collections design (Foods ↔ Nutrients)
- ✅ CRUD operations for both entities
- ✅ Nested relation endpoints
- ✅ Data import scripts with chunking
- ✅ Jest E2E tests
- ✅ Postman collection included

**Quick Start:**
```bash
cd "Azure Cosmos DB"
cp .env.example .env
npm install
npm run start
```

📖 [Full Documentation](/Azure%20Cosmos%20DB/README.md)

## 🚀 Getting Started

1. **Choose your implementation:**
   - For a full-featured meal planning and recipe API → Use the [.NET API](/api)
   - For a nutrition-focused foods and nutrients API → Use the [Node.js API](/Azure%20Cosmos%20DB)

2. **Set up MongoDB/Azure Cosmos DB:**
   - Both implementations require a MongoDB or Azure Cosmos DB instance
   - Configure connection strings in `.env` files

3. **Follow the specific README** for your chosen implementation

## 🛠️ Technologies

### .NET API
- .NET 10
- MongoDB.Driver 3.2.0
- Swagger/OpenAPI
- BCrypt for password hashing

### Node.js API
- Node.js
- MongoDB/Azure Cosmos DB
- Jest for testing
- Express.js

## 📚 API Endpoints

### .NET API Endpoints
- Foods: CRUD, search, pagination
- Recipes: CRUD, search, meal planning
- Users: User management, preferences, dietary restrictions
- Meal Plans: User-specific meal planning and scheduling

### Node.js API Endpoints
- Foods: CRUD, search, with optional nutrient inclusion
- Nutrients: CRUD, filtering by food or nutrient ID
- Nested endpoints for food-nutrient relationships

## 🔒 Security

- Both APIs use environment variables for sensitive configuration
- .NET API implements password hashing with BCrypt
- `.env` files are gitignored to protect credentials
- GitHub Actions workflow for continuous integration

## 📦 CI/CD

This repository includes a GitHub Actions workflow that:
- Builds the .NET project on push and pull requests
- Runs automated tests
- Ensures code quality

## 🤝 Contributing

When contributing to this repository:
1. Follow the existing code style and patterns
2. Update documentation for any API changes
3. Ensure tests pass before submitting pull requests
4. Keep both implementations' documentation in sync with changes

## 📄 License

See individual project directories for licensing information.

## 🔗 Related Resources

- [MongoDB Documentation](https://docs.mongodb.com/)
- [Azure Cosmos DB for MongoDB](https://learn.microsoft.com/en-us/azure/cosmos-db/mongodb/introduction)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Node.js Documentation](https://nodejs.org/en/docs/)
