using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Migrations;

public interface INutrientMigrationService
{
    Task<MigrationResult> ExtractAndNormalizeNutrientsAsync(string sourceCollection = "foundationfoods");
    Task<MigrationResult> CreateIndexesAsync();
    Task<DataExplorationResult> ExploreDataStructureAsync(string sourceCollection = "foundationfoods");
}

public class MigrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int FoodsProcessed { get; set; }
    public int UniqueNutrientsFound { get; set; }
    public int FoodNutrientRelationshipsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> SampleNutrients { get; set; } = new();
}

public class DataExplorationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long TotalDocuments { get; set; }
    public List<string> CollectionNames { get; set; } = new();
    public List<string> SampleDocumentFields { get; set; } = new();
    public List<BsonDocument> SampleDocuments { get; set; } = new();
    public string RawSampleJson { get; set; } = string.Empty;
}

/// <summary>
/// Helper class to track unique nutrients during extraction
/// </summary>
public class ExtractedNutrient
{
    public int NutrientNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class NutrientMigrationService : INutrientMigrationService
{
    private readonly IDatabaseService _databaseService;
    private readonly INutrientRepository _nutrientRepository;
    private readonly IFoodNutrientRepository _foodNutrientRepository;
    private readonly MongoDbSettings _settings;
    private readonly ILogger<NutrientMigrationService> _logger;

    public NutrientMigrationService(
        IDatabaseService databaseService,
        INutrientRepository nutrientRepository,
        IFoodNutrientRepository foodNutrientRepository,
        IOptions<MongoDbSettings> settings,
        ILogger<NutrientMigrationService> logger)
    {
        _databaseService = databaseService;
        _nutrientRepository = nutrientRepository;
        _foodNutrientRepository = foodNutrientRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Explores the data structure to understand what fields exist
    /// </summary>
    public async Task<DataExplorationResult> ExploreDataStructureAsync(string sourceCollection = "foundationfoods")
    {
        var result = new DataExplorationResult();

        try
        {
            // List all collections in the database
            var database = _databaseService.GetDatabase();
            var collectionsCursor = await database.ListCollectionNamesAsync();
            result.CollectionNames = await collectionsCursor.ToListAsync();

            // Get the source collection
            var collection = _databaseService.GetCollection<BsonDocument>(sourceCollection);

            // Count documents
            result.TotalDocuments = await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);

            // Get sample documents
            var samples = await collection.Find(FilterDefinition<BsonDocument>.Empty)
                .Limit(3)
                .ToListAsync();

            result.SampleDocuments = samples;

            if (samples.Count > 0)
            {
                // Get field names from first document
                result.SampleDocumentFields = samples[0].Names.ToList();

                // Pretty print first document
                result.RawSampleJson = samples[0].ToJson(new MongoDB.Bson.IO.JsonWriterSettings
                {
                    Indent = true
                });
            }

            result.Success = true;
            result.Message = $"Found {result.TotalDocuments} documents in '{sourceCollection}'. Collections: {string.Join(", ", result.CollectionNames)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exploring data structure");
            result.Success = false;
            result.Message = $"Failed to explore data: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Extracts all unique nutrients from the database and creates normalized structure.
    /// Does NOT use predefined nutrients - extracts everything from actual data.
    /// </summary>
    public async Task<MigrationResult> ExtractAndNormalizeNutrientsAsync(string sourceCollection = "foundationfoods")
    {
        var result = new MigrationResult();

        try
        {
            // Step 1: Get source collection
            var collection = _databaseService.GetCollection<BsonDocument>(sourceCollection);

            // Step 2: Find all documents
            var cursor = await collection.FindAsync(FilterDefinition<BsonDocument>.Empty);
            var foods = await cursor.ToListAsync();

            _logger.LogInformation("Found {Count} documents in {Collection}", foods.Count, sourceCollection);

            // Step 3: Extract all unique nutrients from the data
            var uniqueNutrients = new Dictionary<int, ExtractedNutrient>();
            var foodNutrientData = new List<(string FoodId, int NutrientNumber, double Amount, string? Derivation)>();

            int sortOrder = 0;

            foreach (var food in foods)
            {
                result.FoodsProcessed++;

                try
                {
                    var foodId = food["_id"].AsObjectId.ToString();

                    // Try different possible field names for nutrients
                    BsonArray? nutrientsArray = null;

                    if (food.Contains("foodNutrients") && food["foodNutrients"].IsBsonArray)
                    {
                        nutrientsArray = food["foodNutrients"].AsBsonArray;
                    }
                    else if (food.Contains("nutrients") && food["nutrients"].IsBsonArray)
                    {
                        nutrientsArray = food["nutrients"].AsBsonArray;
                    }

                    if (nutrientsArray == null) continue;

                    foreach (var nutrientDoc in nutrientsArray)
                    {
                        if (!nutrientDoc.IsBsonDocument) continue;

                        var nutrientBson = nutrientDoc.AsBsonDocument;

                        // Extract nutrient metadata
                        int nutrientNumber = 0;
                        string nutrientName = "";
                        string nutrientUnit = "";

                        // Try nested nutrient object first (FDA format)
                        if (nutrientBson.Contains("nutrient") && nutrientBson["nutrient"].IsBsonDocument)
                        {
                            var nutrientInfo = nutrientBson["nutrient"].AsBsonDocument;

                            if (nutrientInfo.Contains("number"))
                                nutrientNumber = nutrientInfo["number"].ToInt32();
                            else if (nutrientInfo.Contains("id"))
                                nutrientNumber = nutrientInfo["id"].ToInt32();

                            if (nutrientInfo.Contains("name"))
                                nutrientName = nutrientInfo["name"].AsString;

                            if (nutrientInfo.Contains("unitName"))
                                nutrientUnit = nutrientInfo["unitName"].AsString;
                            else if (nutrientInfo.Contains("unit"))
                                nutrientUnit = nutrientInfo["unit"].AsString;
                        }
                        else
                        {
                            // Flat structure
                            if (nutrientBson.Contains("nutrientId"))
                                nutrientNumber = nutrientBson["nutrientId"].ToInt32();
                            else if (nutrientBson.Contains("nutrientNumber"))
                                nutrientNumber = nutrientBson["nutrientNumber"].ToInt32();
                            else if (nutrientBson.Contains("id"))
                                nutrientNumber = nutrientBson["id"].ToInt32();

                            if (nutrientBson.Contains("nutrientName"))
                                nutrientName = nutrientBson["nutrientName"].AsString;
                            else if (nutrientBson.Contains("name"))
                                nutrientName = nutrientBson["name"].AsString;

                            if (nutrientBson.Contains("unitName"))
                                nutrientUnit = nutrientBson["unitName"].AsString;
                            else if (nutrientBson.Contains("unit"))
                                nutrientUnit = nutrientBson["unit"].AsString;
                        }

                        // Skip if we couldn't extract a nutrient identifier
                        if (nutrientNumber == 0 && string.IsNullOrEmpty(nutrientName))
                            continue;

                        // Use name hash as number if no number provided
                        if (nutrientNumber == 0)
                            nutrientNumber = nutrientName.GetHashCode();

                        // Add to unique nutrients dictionary
                        if (!uniqueNutrients.ContainsKey(nutrientNumber))
                        {
                            sortOrder++;
                            uniqueNutrients[nutrientNumber] = new ExtractedNutrient
                            {
                                NutrientNumber = nutrientNumber,
                                Name = nutrientName,
                                Unit = nutrientUnit,
                                Category = CategorizeNutrient(nutrientName)
                            };
                        }

                        // Extract amount
                        double amount = 0;
                        if (nutrientBson.Contains("amount"))
                            amount = GetDoubleValue(nutrientBson["amount"]);
                        else if (nutrientBson.Contains("value"))
                            amount = GetDoubleValue(nutrientBson["value"]);

                        string? derivation = null;
                        if (nutrientBson.Contains("derivationDescription"))
                            derivation = nutrientBson["derivationDescription"].AsString;

                        // Store for later
                        foodNutrientData.Add((foodId, nutrientNumber, amount, derivation));
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing food {result.FoodsProcessed}: {ex.Message}");
                }
            }

            result.UniqueNutrientsFound = uniqueNutrients.Count;
            result.SampleNutrients = uniqueNutrients.Values.Take(20).Select(n => $"{n.NutrientNumber}: {n.Name} ({n.Unit})").ToList();

            _logger.LogInformation("Extracted {Count} unique nutrients from {Foods} foods",
                uniqueNutrients.Count, result.FoodsProcessed);

            // Step 4: Create nutrient records in database
            var nutrientsToCreate = uniqueNutrients.Values.Select((n, idx) => new Nutrient
            {
                NutrientNumber = n.NutrientNumber,
                Name = n.Name,
                Unit = n.Unit,
                Category = n.Category,
                SortOrder = idx + 1
            }).ToList();

            if (nutrientsToCreate.Count > 0)
            {
                await _nutrientRepository.CreateManyAsync(nutrientsToCreate);
                _logger.LogInformation("Created {Count} nutrient records", nutrientsToCreate.Count);
            }

            // Step 5: Get the mapping of nutrient numbers to IDs
            var nutrientMap = await _nutrientRepository.GetNutrientNumberToIdMapAsync();

            // Step 6: Create food-nutrient relationships
            var foodNutrientsToCreate = new List<FoodNutrient>();
            int batchSize = 1000;

            foreach (var (foodId, nutrientNumber, amount, derivation) in foodNutrientData)
            {
                if (!nutrientMap.TryGetValue(nutrientNumber, out var nutrientId))
                    continue;

                foodNutrientsToCreate.Add(new FoodNutrient
                {
                    FoodId = foodId,
                    NutrientId = nutrientId,
                    Amount = amount,
                    DerivationDescription = derivation
                });

                // Batch insert
                if (foodNutrientsToCreate.Count >= batchSize)
                {
                    await _foodNutrientRepository.CreateManyAsync(foodNutrientsToCreate);
                    result.FoodNutrientRelationshipsCreated += foodNutrientsToCreate.Count;
                    foodNutrientsToCreate.Clear();
                }
            }

            // Insert remaining
            if (foodNutrientsToCreate.Count > 0)
            {
                await _foodNutrientRepository.CreateManyAsync(foodNutrientsToCreate);
                result.FoodNutrientRelationshipsCreated += foodNutrientsToCreate.Count;
            }

            result.Success = true;
            result.Message = $"Extracted {result.UniqueNutrientsFound} unique nutrients from {result.FoodsProcessed} foods. " +
                             $"Created {result.FoodNutrientRelationshipsCreated} food-nutrient relationships.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting and normalizing nutrients");
            result.Success = false;
            result.Message = $"Migration failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }

        return result;
    }

    /// <summary>
    /// Creates indexes for efficient queries
    /// </summary>
    public async Task<MigrationResult> CreateIndexesAsync()
    {
        var result = new MigrationResult();

        try
        {
            // Create indexes on nutrients collection
            var nutrientsCollection = _databaseService.GetCollection<Nutrient>(_settings.Collections.Nutrients);

            try
            {
                await nutrientsCollection.Indexes.CreateOneAsync(
                    new CreateIndexModel<Nutrient>(
                        Builders<Nutrient>.IndexKeys.Ascending(n => n.NutrientNumber),
                        new CreateIndexOptions { Unique = true, Name = "idx_nutrientNumber" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Code == 85 || ex.Code == 86)
            {
                _logger.LogWarning("Index already exists on nutrients.nutrientNumber");
            }

            // Create indexes on foodnutrients collection
            var foodNutrientsCollection = _databaseService.GetCollection<FoodNutrient>(_settings.Collections.FoodNutrients);

            try
            {
                await foodNutrientsCollection.Indexes.CreateManyAsync(new[]
                {
                    new CreateIndexModel<FoodNutrient>(
                        Builders<FoodNutrient>.IndexKeys.Ascending(fn => fn.FoodId),
                        new CreateIndexOptions { Name = "idx_foodId" }
                    ),
                    new CreateIndexModel<FoodNutrient>(
                        Builders<FoodNutrient>.IndexKeys.Ascending(fn => fn.NutrientId),
                        new CreateIndexOptions { Name = "idx_nutrientId" }
                    ),
                    new CreateIndexModel<FoodNutrient>(
                        Builders<FoodNutrient>.IndexKeys.Ascending(fn => fn.FoodId).Ascending(fn => fn.NutrientId),
                        new CreateIndexOptions { Unique = true, Name = "idx_foodId_nutrientId" }
                    )
                });
            }
            catch (MongoCommandException ex) when (ex.Code == 85 || ex.Code == 86)
            {
                _logger.LogWarning("One or more indexes already exist on foodnutrients");
            }

            result.Success = true;
            result.Message = "Successfully created indexes";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating indexes");
            result.Success = false;
            result.Message = $"Failed to create indexes: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    private static double GetDoubleValue(BsonValue value)
    {
        if (value.IsDouble) return value.AsDouble;
        if (value.IsInt32) return value.AsInt32;
        if (value.IsInt64) return value.AsInt64;
        if (value.IsDecimal128) return (double)value.AsDecimal128;
        return 0;
    }

    private static string? CategorizeNutrient(string name)
    {
        var lowerName = name.ToLowerInvariant();

        if (lowerName.Contains("energy") || lowerName.Contains("calor"))
            return "Energy";
        if (lowerName.Contains("protein"))
            return "Macronutrient";
        if (lowerName.Contains("fat") || lowerName.Contains("lipid") || lowerName.Contains("fatty") || lowerName.Contains("cholesterol"))
            return "Lipid";
        if (lowerName.Contains("carbohydrate") || lowerName.Contains("fiber") || lowerName.Contains("sugar") || lowerName.Contains("starch"))
            return "Carbohydrate";
        if (lowerName.Contains("vitamin"))
            return "Vitamin";
        if (lowerName.Contains("calcium") || lowerName.Contains("iron") || lowerName.Contains("magnesium") ||
            lowerName.Contains("phosphorus") || lowerName.Contains("potassium") || lowerName.Contains("sodium") ||
            lowerName.Contains("zinc") || lowerName.Contains("copper") || lowerName.Contains("manganese") ||
            lowerName.Contains("selenium") || lowerName.Contains("mineral"))
            return "Mineral";
        if (lowerName.Contains("thiamin") || lowerName.Contains("riboflavin") || lowerName.Contains("niacin") ||
            lowerName.Contains("folate") || lowerName.Contains("choline") || lowerName.Contains("betaine"))
            return "Vitamin";

        return null;
    }
}
