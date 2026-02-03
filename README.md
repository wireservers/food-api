# Bring The Diet API

A RESTful .NET 10 Web API with CRUD operations using MongoDB (Azure Cosmos DB) and Entity Framework Core concepts.

## Features

- ✅ RESTful API with full CRUD operations
- ✅ MongoDB integration (Azure Cosmos DB compatible)
- ✅ Repository pattern for data access
- ✅ Swagger/OpenAPI documentation
- ✅ Environment variable configuration (.env file)
- ✅ Password hashing with BCrypt
- ✅ CORS enabled
- ✅ Comprehensive error handling and logging

## Project Structure

```
├── Configuration/       # Application settings and configuration
├── Controllers/         # API endpoints (Foods, Recipes, Users, MealPlans)
├── DTOs/               # Data Transfer Objects
├── Models/             # Database entity models
├── Repositories/       # Data access layer
├── Services/           # Business logic and database services
├── Program.cs          # Application entry point
└── appsettings.json    # Configuration file
```

## API Endpoints

All list endpoints support **pagination** with query parameters:
- `?page={number}` - Page number (1-based, default: 1)
- `?pageSize={number}` - Items per page (default: 20, max: 100)

Example: `GET /api/foods?page=2&pageSize=50`

### Foods
- `GET /api/foods?page={page}&pageSize={pageSize}` - Get paginated foods
- `GET /api/foods/{id}` - Get food by ID
- `GET /api/foods/search?term={term}` - Search foods by description
- `POST /api/foods` - Create a new food
- `PUT /api/foods/{id}` - Update a food
- `DELETE /api/foods/{id}` - Delete a food

### Recipes
- `GET /api/recipes?page={page}&pageSize={pageSize}` - Get paginated recipes
- `GET /api/recipes/{id}` - Get recipe by ID
- `GET /api/recipes/search?term={term}` - Search recipes by name
- `POST /api/recipes` - Create a new recipe
- `PUT /api/recipes/{id}` - Update a recipe
- `DELETE /api/recipes/{id}` - Delete a recipe

### Users
- `GET /api/users?page={page}&pageSize={pageSize}` - Get paginated users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/username/{username}` - Get user by username
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user

### Meal Plans
- `GET /api/mealplans?page={page}&pageSize={pageSize}` - Get paginated meal plans
- `GET /api/mealplans/{id}` - Get meal plan by ID
- `GET /api/mealplans/user/{userId}` - Get meal plans by user ID
- `POST /api/mealplans` - Create a new meal plan
- `PUT /api/mealplans/{id}` - Update a meal plan
- `DELETE /api/mealplans/{id}` - Delete a meal plan

## Getting Started

### Prerequisites

- .NET 10 SDK
- MongoDB or Azure Cosmos DB for MongoDB account

### Installation

1. Clone the repository
2. Ensure your `.env` file is properly configured with your MongoDB connection string
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7xxx` (HTTPS) and `http://localhost:5xxx` (HTTP).

### Swagger Documentation

Once running, navigate to `https://localhost:7xxx/swagger` to view the interactive API documentation.

## Environment Variables

The application reads the following variables from the `.env` file:

- `MONGO_URI` - MongoDB connection string
- `DB_NAME` - Database name
- `DB_FOODS_COLLECTION` - Foods collection name
- `DB_RECIPES_COLLECTION` - Recipes collection name
- `DB_USERS_COLLECTION` - Users collection name
- `DB_MEALPLANS_COLLECTION` - Meal plans collection name

## Database Models

### Food
- Foundation foods from USDA database
- Includes nutritional information, serving sizes, and ingredients

### Recipe
- Custom recipes with ingredients and instructions
- Prep time, cook time, and serving information
- Cuisine and difficulty level

### User
- User authentication and profile information
- Dietary preferences and restrictions
- Allergy information

### Meal Plan
- User-specific meal planning
- Date range and meal scheduling
- Links to recipes

## Technologies Used

- .NET 10
- MongoDB.Driver 3.2.0
- Swashbuckle.AspNetCore 7.2.0 (Swagger)
- DotNetEnv 3.1.1
- BCrypt.Net-Next 4.0.3

## Development

The API uses the repository pattern for data access, making it easy to:
- Unit test business logic
- Swap data providers if needed
- Maintain clean separation of concerns

## Security Notes

- Passwords are hashed using BCrypt
- CORS is currently set to allow all origins (configure appropriately for production)
- Consider adding authentication/authorization middleware for production use
- The `.env` file is gitignored to protect sensitive credentials

## Future Enhancements

- [ ] Add JWT authentication
- [ ] Implement pagination for list endpoints
- [ ] Add data validation attributes
- [ ] Implement caching
- [ ] Add rate limiting
- [ ] Add comprehensive unit and integration tests
- [ ] Add health check endpoints
