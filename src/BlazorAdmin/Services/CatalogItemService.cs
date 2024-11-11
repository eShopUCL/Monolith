using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using BlazorShared.Interfaces;
using BlazorShared.Models;
using Microsoft.Extensions.Logging;
using System;


namespace BlazorAdmin.Services;

public class CatalogItemService : ICatalogItemService
{
    private readonly ICatalogLookupDataService<CatalogBrand> _brandService;
    private readonly ICatalogLookupDataService<CatalogType> _typeService;
    private readonly HttpService _httpService;
    private readonly ILogger<CatalogItemService> _logger;

    public CatalogItemService(ICatalogLookupDataService<CatalogBrand> brandService,
        ICatalogLookupDataService<CatalogType> typeService,
        HttpService httpService,
        ILogger<CatalogItemService> logger)
    {
        _brandService = brandService;
        _typeService = typeService;
        _httpService = httpService;
        _logger = logger;
    }

    public async Task<CatalogItem> Create(CreateCatalogItemRequest catalogItem)
    {
        var httpClient = new HttpClient();
        var url = "http://localhost:5229/api/catalog/items/";

        // Serialize the catalogItem to JSON
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(catalogItem),
            Encoding.UTF8,
            "application/json");

        // Send the POST request
        var response = await httpClient.PostAsync(url, jsonContent);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Deserialize the response content to a CatalogItem
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdItem = JsonSerializer.Deserialize<CatalogItem>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return createdItem;
    }

    public async Task<CatalogItem> Edit(CatalogItem catalogItem)
    {
        return (await _httpService.HttpPut<EditCatalogItemResult>("catalog-items", catalogItem)).CatalogItem;
    }

    public async Task<string> Delete(int catalogItemId)
    {
        return (await _httpService.HttpDelete<DeleteCatalogItemResponse>("catalog-items", catalogItemId)).Status;
    }

    public async Task<CatalogItem> GetById(int id)
    {
        var brandListTask = _brandService.List();
        var typeListTask = _typeService.List();
        var itemGetTask = _httpService.HttpGet<EditCatalogItemResult>($"catalog-items/{id}");
        await Task.WhenAll(brandListTask, typeListTask, itemGetTask);
        var brands = brandListTask.Result;
        var types = typeListTask.Result;
        var catalogItem = itemGetTask.Result.CatalogItem;
        catalogItem.CatalogBrand = brands.FirstOrDefault(b => b.Id == catalogItem.CatalogBrandId)?.Name;
        catalogItem.CatalogType = types.FirstOrDefault(t => t.Id == catalogItem.CatalogTypeId)?.Name;
        return catalogItem;
    }

    public async Task<List<CatalogItem>> ListPaged(int pageSize)
    {
        _logger.LogInformation($"Fetching catalog items from API with PageSize={pageSize}.");

        var httpClient = new HttpClient();
        var url = $"http://localhost:5229/api/catalog/items?PageSize={pageSize}";

        // Send the GET request
        var response = await httpClient.GetAsync(url);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Deserialize the response content
        var responseContent = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedCatalogItemResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var brands = await _brandService.List();
        var types = await _typeService.List();
        var items = pagedResponse.CatalogItems;

        foreach (var item in items)
        {
            item.CatalogBrand = brands.FirstOrDefault(b => b.Id == item.CatalogBrandId)?.Name;
            item.CatalogType = types.FirstOrDefault(t => t.Id == item.CatalogTypeId)?.Name;
        }

        return items;
    }

    public async Task<List<CatalogItem>> List()
{
    _logger.LogInformation("Fetching catalog items from API.");
    var httpClient = new HttpClient();
    var url = "http://localhost:5229/api/catalog/items";

    // Send the GET request
    var response = await httpClient.GetAsync(url);

    // Ensure the request was successful
    response.EnsureSuccessStatusCode();

    // Deserialize the response content
    var responseContent = await response.Content.ReadAsStringAsync();
    var pagedResponse = JsonSerializer.Deserialize<PagedCatalogItemResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Console.WriteLine("Catalog Items fetched from API:");
    foreach (var item in pagedResponse.CatalogItems)
    {
        Console.WriteLine($"- ID: {item.Id}, Name: {item.Name}, Brand ID: {item.CatalogBrandId}, Type ID: {item.CatalogTypeId}");
    }

    var brands = await _brandService.List();
    Console.WriteLine("Catalog Brands fetched:");
    foreach (var brand in brands)
    {
        Console.WriteLine($"- Brand ID: {brand.Id}, Name: {brand.Name}");
    }

    var types = await _typeService.List();
    Console.WriteLine("Catalog Types fetched:");
    foreach (var type in types)
    {
        Console.WriteLine($"- Type ID: {type.Id}, Name: {type.Name}");
    }

    var items = pagedResponse.CatalogItems;

    Console.WriteLine("Mapping Catalog Items with Brands and Types:");
    foreach (var item in items)
    {
        // Attempt to find matching brand and type names
        var brandName = brands.FirstOrDefault(b => b.Id == item.CatalogBrandId)?.Name;
        var typeName = types.FirstOrDefault(t => t.Id == item.CatalogTypeId)?.Name;

        // Log each match or missing entry
        if (brandName != null)
        {
            Console.WriteLine($"- Item ID: {item.Id}, Brand Matched: {brandName}");
        }
        else
        {
            Console.WriteLine($"- Item ID: {item.Id}, Brand not found for Brand ID: {item.CatalogBrandId}");
        }

        if (typeName != null)
        {
            Console.WriteLine($"- Item ID: {item.Id}, Type Matched: {typeName}");
        }
        else
        {
            Console.WriteLine($"- Item ID: {item.Id}, Type not found for Type ID: {item.CatalogTypeId}");
        }

        // Assign the names to the item
        item.CatalogBrand = brandName;
        item.CatalogType = typeName;
    }

    return items;
}

}
