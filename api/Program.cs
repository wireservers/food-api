using DotNetEnv;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Repositories;
using BringTheDiet.Api.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file in current directory
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
Console.WriteLine($"[DEBUG] Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"[DEBUG] Looking for .env at: {envPath}");
Console.WriteLine($"[DEBUG] .env exists: {File.Exists(envPath)}");

if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"[DEBUG] Loaded .env from: {envPath}");
}
else
{
    // Try loading from parent directory as fallback
    var parentEnvPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", ".env");
    if (File.Exists(parentEnvPath))
    {
        Env.Load(parentEnvPath);
        Console.WriteLine($"[DEBUG] Loaded .env from parent: {parentEnvPath}");
    }
    else
    {
        Env.Load();
        Console.WriteLine("[DEBUG] No .env found, using defaults");
    }
}

var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
Console.WriteLine($"[DEBUG] MONGO_URI: {(string.IsNullOrEmpty(mongoUri) ? "NOT SET" : "SET (length: " + mongoUri.Length + ")")}");
Console.WriteLine($"[DEBUG] DB_NAME: {dbName ?? "NOT SET"}");

// Configure MongoDB settings
builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MONGO_URI") ?? "";
    options.DatabaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "";
    options.Collections.Foods = Environment.GetEnvironmentVariable("DB_FOODS_COLLECTION") ?? "foundationfoods";
    options.Collections.Nutrition = Environment.GetEnvironmentVariable("DB_NUTRITION_COLLECTION") ?? "nutritionfacts";
    options.Collections.Recipes = Environment.GetEnvironmentVariable("DB_RECIPES_COLLECTION") ?? "recipes";
    options.Collections.Diets = Environment.GetEnvironmentVariable("DB_DIETS_COLLECTION") ?? "diettypes";
    options.Collections.Blog = Environment.GetEnvironmentVariable("DB_BLOG_COLLECTION") ?? "blogposts";
    options.Collections.MealPlans = Environment.GetEnvironmentVariable("DB_MEALPLANS_COLLECTION") ?? "mealplans";
    options.Collections.Users = Environment.GetEnvironmentVariable("DB_USERS_COLLECTION") ?? "users";
    options.Collections.Roles = Environment.GetEnvironmentVariable("DB_ROLES_COLLECTION") ?? "roles";
    options.Collections.Permissions = Environment.GetEnvironmentVariable("DB_PERMISSIONS_COLLECTION") ?? "permissions";
    options.Collections.AuditLogs = Environment.GetEnvironmentVariable("DB_AUDIT_COLLECTION") ?? "auditlogs";
});

// Register MongoDB client as singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("MONGO_URI") ?? "";
    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(60);
    settings.ConnectTimeout = TimeSpan.FromSeconds(10);
    settings.SocketTimeout = TimeSpan.FromSeconds(60);
    settings.MaxConnectionPoolSize = 100;
    settings.RetryWrites = false;

    return new MongoClient(settings);
});

// Register database service
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Register repositories
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMealPlanRepository, MealPlanRepository>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Bring The Diet API", 
        Version = "v1",
        Description = "A RESTful API for managing foods, recipes, users, and meal plans with MongoDB backend",
        Contact = new() 
        { 
            Name = "Bring The Diet", 
            Url = new Uri("https://github.com/yourusername/bring-the-diet") 
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Enable annotations for better Swagger UI
    c.EnableAnnotations();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable developer exception page only in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger for testing (in both Development and Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bring The Diet API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Bring The Diet API";
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => new { Message = "Welcome to Bring The Diet APIß" });


app.Run();
