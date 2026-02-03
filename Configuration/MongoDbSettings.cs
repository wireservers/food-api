namespace BringTheDiet.Api.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public CollectionNames Collections { get; set; } = new();
}

public class CollectionNames
{
    public string Foods { get; set; } = "foods";
    public string Nutrients { get; set; } = "nutrients";
    public string FoodNutrients { get; set; } = "foodnutrients";
    public string Recipes { get; set; } = "recipes";
    public string Diets { get; set; } = "diettypes";
    public string Blog { get; set; } = "blogposts";
    public string MealPlans { get; set; } = "mealplans";
    public string Users { get; set; } = "users";
    public string Roles { get; set; } = "roles";
    public string Permissions { get; set; } = "permissions";
    public string AuditLogs { get; set; } = "auditlogs";
}
