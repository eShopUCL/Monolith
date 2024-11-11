using System.Text.Json.Serialization;
using BlazorShared.Attributes;

namespace BlazorShared.Models;

[Endpoint(Name = "catalog-brands")]
public class CatalogBrand : LookupData
{
    //Test
    [JsonPropertyName("brand")] // Map "brand" from JSON to "Name"
    public new string Name { get; set; }
}
