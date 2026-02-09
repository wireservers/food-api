using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BringTheDiet.Api.Migrations;

namespace BringTheDiet.Api.Controllers;

/// <summary>
/// Database migration operations for normalizing nutrition data
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MigrationsController : ControllerBase
{
    private readonly INutrientMigrationService _migrationService;
    private readonly ILogger<MigrationsController> _logger;

    public MigrationsController(
        INutrientMigrationService migrationService,
        ILogger<MigrationsController> logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// Explores the data structure to understand what fields exist in the source collection
    /// </summary>
    [HttpGet("explore")]
    [SwaggerOperation(Summary = "Explore data structure", Description = "Shows sample documents and field names from the source collection")]
    [ProducesResponseType(typeof(DataExplorationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataExplorationResult>> ExploreData([FromQuery] string sourceCollection = "foundationfoods")
    {
        _logger.LogInformation("Exploring data structure in {Collection}", sourceCollection);
        var result = await _migrationService.ExploreDataStructureAsync(sourceCollection);
        return Ok(result);
    }

    /// <summary>
    /// Extracts all unique nutrients from existing data and creates normalized structure
    /// </summary>
    /// <param name="sourceCollection">Source collection with embedded nutrients (default: foundationfoods)</param>
    [HttpPost("extract-and-normalize")]
    [SwaggerOperation(Summary = "Extract and normalize nutrients",
        Description = "Reads all documents, extracts unique nutrients, deduplicates them, and creates the junction table")]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<MigrationResult>> ExtractAndNormalize([FromQuery] string sourceCollection = "foundationfoods")
    {
        _logger.LogInformation("Starting nutrient extraction and normalization from {Collection}", sourceCollection);
        var result = await _migrationService.ExtractAndNormalizeNutrientsAsync(sourceCollection);
        return Ok(result);
    }

    /// <summary>
    /// Creates database indexes for optimized queries
    /// </summary>
    [HttpPost("create-indexes")]
    [SwaggerOperation(Summary = "Create indexes", Description = "Creates MongoDB indexes on nutrients and foodnutrients collections")]
    [ProducesResponseType(typeof(MigrationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<MigrationResult>> CreateIndexes()
    {
        _logger.LogInformation("Creating database indexes");
        var result = await _migrationService.CreateIndexesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Runs the full migration: extract nutrients, deduplicate, create junction table, add indexes
    /// </summary>
    [HttpPost("run-all")]
    [SwaggerOperation(Summary = "Run full migration",
        Description = "Extracts unique nutrients from data, creates normalized structure, and adds indexes")]
    [ProducesResponseType(typeof(FullMigrationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<FullMigrationResult>> RunFullMigration([FromQuery] string sourceCollection = "foundationfoods")
    {
        _logger.LogInformation("Running full migration from {Collection}", sourceCollection);

        var results = new FullMigrationResult();

        // First, create indexes
        results.CreateIndexes = await _migrationService.CreateIndexesAsync();

        // Then extract and normalize
        results.ExtractAndNormalize = await _migrationService.ExtractAndNormalizeNutrientsAsync(sourceCollection);

        results.Success = results.CreateIndexes.Success && results.ExtractAndNormalize.Success;
        results.Message = results.Success
            ? $"Migration complete. {results.ExtractAndNormalize.UniqueNutrientsFound} unique nutrients, {results.ExtractAndNormalize.FoodNutrientRelationshipsCreated} relationships."
            : "Migration had errors - check individual results";

        return Ok(results);
    }
}

public class FullMigrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MigrationResult CreateIndexes { get; set; } = new();
    public MigrationResult ExtractAndNormalize { get; set; } = new();
}
