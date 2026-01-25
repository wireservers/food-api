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

// Configure MongoDB settings from appsettings (environment-specific) with env var override
builder.Services.Configure<MongoDbSettings>(options =>
{
    // Priority: Environment Variables > appsettings.{Environment}.json > appsettings.json
    var mongoSection = builder.Configuration.GetSection("MongoDB");

    options.ConnectionString = Environment.GetEnvironmentVariable("MONGO_URI")
        ?? mongoSection["ConnectionString"]
        ?? "";

    options.DatabaseName = Environment.GetEnvironmentVariable("DB_NAME")
        ?? mongoSection["DatabaseName"]
        ?? "";

    options.Collections.Foods = mongoSection["Collections:Foods"] ?? "foundationfoods";
    options.Collections.Nutrition = mongoSection["Collections:Nutrition"] ?? "nutritionfacts";
    options.Collections.Recipes = mongoSection["Collections:Recipes"] ?? "recipes";
    options.Collections.Diets = mongoSection["Collections:Diets"] ?? "diettypes";
    options.Collections.Blog = mongoSection["Collections:Blog"] ?? "blogposts";
    options.Collections.MealPlans = mongoSection["Collections:MealPlans"] ?? "mealplans";
    options.Collections.Users = mongoSection["Collections:Users"] ?? "users";
    options.Collections.Roles = mongoSection["Collections:Roles"] ?? "roles";
    options.Collections.Permissions = mongoSection["Collections:Permissions"] ?? "permissions";
    options.Collections.AuditLogs = mongoSection["Collections:AuditLogs"] ?? "auditlogs";

    Console.WriteLine($"[INFO] Environment: {builder.Environment.EnvironmentName}");
    Console.WriteLine($"[INFO] Database: {options.DatabaseName}");
    Console.WriteLine($"[INFO] Connection String: {(string.IsNullOrEmpty(options.ConnectionString) ? "NOT SET" : "SET")}");
});

// Register MongoDB client as singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoConfig = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    var connectionString = mongoConfig.ConnectionString;

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("MongoDB connection string is not configured. Set MONGO_URI environment variable or configure in appsettings.json");
    }

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

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5001;
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

app.MapGet("/", () => Results.Ok("Welcome to Bring The Diet APIß"));


app.Run();
