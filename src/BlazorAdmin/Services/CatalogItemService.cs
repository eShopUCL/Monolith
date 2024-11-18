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

    // Vi hardcoder også vores baseUrl for nu, men i fremtiden, når alle microservices er
    // på plads, skal vi lave base url'et fra HttpService om, så det afspejler vores
    // url for API-Gatewayen.

    // Brug denne for lokal udvikling
    //private const string _baseUrl = "http://localhost:5229/api/catalog/items";

    //Brug denne for deployment til prod
    private const string _baseUrl = "http://4.207.200.245/apigateway/api/catalog/items";

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
        // Serializer indtastet catalogItem til json
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(catalogItem),
            Encoding.UTF8,
            "application/json");

        // Sender get request med httpclient
        // Og tjekker om der er fejl
        var response = await _httpClient.PostAsync(_baseUrl, jsonContent);
        response.EnsureSuccessStatusCode();

        // Deserialize til et CatalogItem objekt
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdItem = JsonSerializer.Deserialize<CatalogItem>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return createdItem;
    }

    public async Task<CatalogItem> Edit(CatalogItem catalogItem)
    {
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(catalogItem),
            Encoding.UTF8,
            "application/json");

        // Opret base url variabel med baseUrl og catalog item id appended
        // Prøvede at gøre det rent igennem body, men ID blev lavet om til 0
        // af en eller anden grund
        var url = $"{_baseUrl}/{catalogItem.Id}";

        var response = await _httpClient.PutAsync(url, jsonContent);

        // Tjekker om response indeholder noContent (success)
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return catalogItem;
        }

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedItem = JsonSerializer.Deserialize<CatalogItem>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return updatedItem;
    }

    public async Task<string> Delete(int catalogItemId)
    {
        // Sender delete request til _baseUrl med catalog item id appended
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{catalogItemId}");
        response.EnsureSuccessStatusCode();

        // Returner status på response
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }

    public async Task<CatalogItem> GetById(int id)
    {
        // Hent CatalogItem fra CatalogService
        var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
        response.EnsureSuccessStatusCode();

        // Deserialize til et CatalogItem objekt
        var responseContent = await response.Content.ReadAsStringAsync();
        var catalogItem = JsonSerializer.Deserialize<CatalogItem>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Hent brands og types via brand og type service
        var brandListTask = _brandService.List();
        var typeListTask = _typeService.List();
        await Task.WhenAll(brandListTask, typeListTask);

        // Map brand og type navne til CatalogItem, da de i frontenden forventes
        // at være strings, og ikke hele objekter, som de er i vores CatalogService Microservice
        var brands = brandListTask.Result;
        var types = typeListTask.Result;
        catalogItem.CatalogBrand = brands.FirstOrDefault(b => b.Id == catalogItem.CatalogBrandId)?.Name;
        catalogItem.CatalogType = types.FirstOrDefault(t => t.Id == catalogItem.CatalogTypeId)?.Name;

        return catalogItem;
    }


    // Hent en liste af catalog items (paged)
    public async Task<List<CatalogItem>> ListPaged(int pageSize)
    {
        // Sender get request med httpclient
        // Og tjekker om der er fejl
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();

        // Deserialize til et PagedCatalogItemResponse objekt
        var responseContent = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedCatalogItemResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var items = pagedResponse.CatalogItems;

        return items;
    }

    // Hent en liste af ALLE catalog items
    public async Task<List<CatalogItem>> List()
    {
        // Send get request for at hente alle items
        var response = await _httpClient.GetAsync($"{_baseUrl}");
        response.EnsureSuccessStatusCode();

        // Deserialize the response into a list of CatalogItems
        var responseContent = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<CatalogItem>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return items;
    }

}
