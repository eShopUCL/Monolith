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

    // Benytter et nyt HttpClient objekt, da HttpService stadig skal
    // Benytte det gamle URL til at hente andet data, som f.eks
    // Orders, identity, osv. Det går nok lidt imod de forudsatte principper
    // I fht. Clean code og performance, men det er lettere indtil vi har resten
    // af vores microservices oppe at køre
    private readonly HttpClient _httpClient = new HttpClient();
    
    // Vi hardcoder også vores baseUrl for nu, men i fremtiden
    // skal vi benytte vores API Gateway, så det bliver mere Dynamisk :)
    private const string _baseUrl = "http://localhost:5229/api/catalog/items";

    public CatalogItemService(ICatalogLookupDataService<CatalogBrand> brandService,
        ICatalogLookupDataService<CatalogType> typeService,
        HttpService httpService,
        HttpClient httpClient,
        ILogger<CatalogItemService> logger)
    {
        _httpClient = httpClient;
        _brandService = brandService;
        _typeService = typeService;
        _httpService = httpService;
        _logger = logger;
    }

    public async Task<CatalogItem> Create(CreateCatalogItemRequest catalogItem)
    {
        var url = $"{_baseUrl}/";

        // Serializer indtastet catalogItem til json
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(catalogItem),
            Encoding.UTF8,
            "application/json");

        // Sender get request med httpclient
        // Og tjekker om der er fejl
        var response = await _httpClient.PostAsync(url, jsonContent);
        response.EnsureSuccessStatusCode();

        // Deserialize til et CatalogItem objekt
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdItem = JsonSerializer.Deserialize<CatalogItem>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return createdItem;
    }

    //TODO: Implementer Edit metoden
    public async Task<CatalogItem> Edit(CatalogItem catalogItem)
    {
        return (await _httpService.HttpPut<EditCatalogItemResult>("catalog-items", catalogItem)).CatalogItem;
    }

    //TODO: Implementer Delete metoden
    public async Task<string> Delete(int catalogItemId)
    {
        return (await _httpService.HttpDelete<DeleteCatalogItemResponse>("catalog-items", catalogItemId)).Status;
    }

    //TODO: Implementer GetById metoden
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
        // Opret det URL vi skal bruge med vores baseUrl + variabel
        var url = $"{_baseUrl}?PageSize={pageSize}";


        // Sender get request med httpclient
        // Og tjekker om der er fejl
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        // Deserialize til et PagedCatalogItemResponse objekt
        var responseContent = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedCatalogItemResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var items = pagedResponse.CatalogItems;

        return items;
    }

    public async Task<List<CatalogItem>> List()
    {
        // Vi hardcoder pagesize for nu
        // TODO: Get-all endpoint i fremtiden
        const int pageSize = 12;

        // Opret det URL vi skal bruge med vores baseUrl + variabel
        var url = $"{_baseUrl}?PageSize={pageSize}";

        // Send get request med httpclient
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        // Deserialize til et PagedCatalogItemResponse objekt
        var responseContent = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedCatalogItemResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var items = pagedResponse.CatalogItems;

        return items;
    }

}
