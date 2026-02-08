using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

[BsonIgnoreExtraElements]
public class Recipe
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("image")]
    public string? Image { get; set; }

    [BsonElement("diet")]
    public string? Diet { get; set; }

    [BsonElement("dietSlug")]
    public string? DietSlug { get; set; }

    [BsonElement("prepTime")]
    public int? PrepTime { get; set; }

    [BsonElement("calories")]
    public int? Calories { get; set; }

    [BsonElement("isFavorite")]
    public bool IsFavorite { get; set; }

    [BsonElement("featured")]
    public bool Featured { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("ingredients")]
    [BsonSerializer(typeof(IngredientListSerializer))]
    public List<RecipeIngredient>? Ingredients { get; set; }

    [BsonElement("instructions")]
    public List<string>? Instructions { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class RecipeIngredient
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public double? Quantity { get; set; }

    [BsonElement("unit")]
    public string? Unit { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Handles mixed ingredient data: deserializes both plain strings and structured objects.
/// Plain strings are converted to RecipeIngredient with just the Name field.
/// </summary>
public class IngredientListSerializer : IBsonSerializer<List<RecipeIngredient>?>
{
    public Type ValueType => typeof(List<RecipeIngredient>);

    object? IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => Deserialize(context, args);

    public List<RecipeIngredient>? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        if (bsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null;
        }

        var list = new List<RecipeIngredient>();
        context.Reader.ReadStartArray();
        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            if (context.Reader.CurrentBsonType == BsonType.String)
            {
                list.Add(new RecipeIngredient { Name = context.Reader.ReadString() });
            }
            else if (context.Reader.CurrentBsonType == BsonType.Document)
            {
                list.Add(BsonSerializer.Deserialize<RecipeIngredient>(context.Reader));
            }
            else
            {
                context.Reader.SkipValue();
            }
        }
        context.Reader.ReadEndArray();
        return list;
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, List<RecipeIngredient>? value)
    {
        if (value == null)
        {
            context.Writer.WriteNull();
            return;
        }
        context.Writer.WriteStartArray();
        foreach (var ingredient in value)
        {
            BsonSerializer.Serialize(context.Writer, ingredient);
        }
        context.Writer.WriteEndArray();
    }

    void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        => Serialize(context, args, value as List<RecipeIngredient>);
}
