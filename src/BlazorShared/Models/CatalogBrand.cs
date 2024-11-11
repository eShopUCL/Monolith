using System.Text.Json.Serialization;
using BlazorShared.Attributes;

namespace BlazorShared.Models;

[Endpoint(Name = "catalog-brands")]
public class CatalogBrand : LookupData
{
    // Vi angiver navnet på json properties, så vi kan lave dem om til C# objekter
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("brand")]
    public string Name { get; set; }
    
}
