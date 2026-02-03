# Example API Requests

This file contains example requests for testing the API using REST Client extension or similar tools.

## Pagination

All `GET` endpoints that return lists support pagination with the following query parameters:
- `page` - Page number (1-based, default: 1)
- `pageSize` - Items per page (default: 20, max: 100)

Example paginated response format:
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Foods API

### Create a Food
```http
POST http://localhost:5000/api/foods
Content-Type: application/json

{
  "fdcId": 123456,
  "description": "Organic Banana",
  "dataType": "foundation_food",
  "publicationDate": "2026-01-22T00:00:00Z",
  "brandOwner": "Fresh Foods Co",
  "ingredients": "Banana",
  "servingSize": 118,
  "servingSizeUnit": "g",
  "foodCategory": "Fruits"
}
```

### Get All Foods (Paginated)
```http
# Default pagination (page 1, 20 items)
GET http://localhost:5000/api/foods

# Specific page and page size
GET http://localhost:5000/api/foods?page=2&pageSize=50

# Large page size (max 100)
GET http://localhost:5000/api/foods?page=1&pageSize=100
```

### Get Food by ID
```http
GET http://localhost:5000/api/foods/{id}
```

### Search Foods
```http
GET http://localhost:5000/api/foods/search?term=banana
```

### Update a Food
```http
PUT http://localhost:5000/api/foods/{id}
Content-Type: application/json
{
  "description": "Organic Banana - Updated",
  "brandOwner": "Fresh Foods Co",
  "ingredients": "100% Organic Banana",
  "servingSize": 120,
  "servingSizeUnit": "g",
  "foodCategory": "Fruits"
}
```

### Delete a Food
```http
DELETE http://localhost:5000/api/foods/{id}
```

## Recipes API

### Create a Recipe
```http
POST http://localhost:5000/api/recipes
Content-Type: application/json

{
  "name": "Banana Smoothie",
  "description": "A healthy and delicious banana smoothie",
  "ingredients": [
    {
      "name": "Banana",
      "quantity": 2,
      "unit": "pieces"
    },
    {
      "name": "Milk",
      "quantity": 1,
      "unit": "cup"
    },
    {
      "name": "Honey",
      "quantity": 1,
      "unit": "tablespoon"
    }
  ],
  "instructions": [
    "Peel the bananas",
    "Add all ingredients to a blender",
    "Blend until smooth",
    "Serve immediately"
  ],
  "prepTime": 5,
  "cookTime": 0,
  "servings": 2,
  "cuisine": "American",
  "difficulty": "Easy",
  "imageUrl": "https://example.com/banana-smoothie.jpg"
}
```

### Get All Recipes
```http
GET http://localhost:5000/api/recipes
```

### Get Recipe by ID
```http
GET http://localhost:5000/api/recipes/{id}
```

### Search Recipes
```http
GET http://localhost:5000/api/recipes/search?term=smoothie
```

### Update a Recipe
```http
PUT http://localhost:5000/api/recipes/{id}
Content-Type: application/json

{
  "name": "Banana Berry Smoothie",
  "description": "A healthy banana smoothie with berries",
  "prepTime": 7,
  "servings": 2
}
```

### Delete a Recipe
```http
DELETE http://localhost:5000/api/recipes/{id}
```

## Users API

### Create a User
```http
POST http://localhost:5000/api/users
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!",
  "preferences": {
    "dietType": "vegetarian",
    "allergies": ["peanuts", "shellfish"],
    "dislikedIngredients": ["olives"],
    "calorieGoal": 2000
  }
}
```

### Get All Users
```http
GET http://localhost:5000/api/users
```

### Get User by ID
```http
GET http://localhost:5000/api/users/{id}
```

### Get User by Username
```http
GET http://localhost:5000/api/users/username/johndoe
```

### Update a User
```http
PUT http://localhost:5000/api/users/{id}
Content-Type: application/json

{
  "email": "john.updated@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "preferences": {
    "dietType": "vegan",
    "allergies": ["peanuts"],
    "dislikedIngredients": ["olives", "mushrooms"],
    "calorieGoal": 1800
  }
}
```

### Delete a User
```http
DELETE http://localhost:5000/api/users/{id}
```

## Meal Plans API

### Create a Meal Plan
```http
POST http://localhost:5000/api/mealplans
Content-Type: application/json

{
  "userId": "{userId}",
  "name": "Weekly Meal Plan - January 2026",
  "startDate": "2026-01-22T00:00:00Z",
  "endDate": "2026-01-29T00:00:00Z",
  "meals": [
    {
      "date": "2026-01-22T08:00:00Z",
      "type": "Breakfast",
      "recipeName": "Banana Smoothie",
      "notes": "Use almond milk instead"
    },
    {
      "date": "2026-01-22T12:00:00Z",
      "type": "Lunch",
      "recipeName": "Greek Salad",
      "notes": ""
    },
    {
      "date": "2026-01-22T18:00:00Z",
      "type": "Dinner",
      "recipeName": "Grilled Chicken",
      "notes": "Extra veggies"
    }
  ]
}
```

### Get All Meal Plans
```http
GET http://localhost:5000/api/mealplans
```

### Get Meal Plan by ID
```http
GET http://localhost:5000/api/mealplans/{id}
```

### Get Meal Plans by User ID
```http
GET http://localhost:5000/api/mealplans/user/{userId}
```

### Update a Meal Plan
```http
PUT http://localhost:5000/api/mealplans/{id}
Content-Type: application/json

{
  "name": "Updated Weekly Meal Plan",
  "meals": [
    {
      "date": "2026-01-22T08:00:00Z",
      "type": "Breakfast",
      "recipeName": "Oatmeal Bowl",
      "notes": "With fresh berries"
    }
  ]
}
```

### Delete a Meal Plan
```http
DELETE http://localhost:5000/api/mealplans/{id}
```

## Notes

- Replace `{id}`, `{userId}` with actual ObjectId values from your database
- Adjust the base URL (`http://localhost:5000`) based on your actual port number
- The API uses MongoDB ObjectId format for IDs (24 character hexadecimal strings)
- All dates are in ISO 8601 format (UTC)
