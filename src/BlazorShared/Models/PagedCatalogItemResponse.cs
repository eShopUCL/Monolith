using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorShared.Models;

public class PagedCatalogItemResponse
{
    // Vi angiver navnet på json properties, så vi kan lave dem om til C# objekter
    [JsonPropertyName("data")]
    public List<CatalogItem> CatalogItems { get; set; } = new List<CatalogItem>();

    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
