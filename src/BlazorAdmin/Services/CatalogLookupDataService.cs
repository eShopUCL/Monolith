using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using BlazorShared;
using BlazorShared.Attributes;
using BlazorShared.Interfaces;
using BlazorShared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorAdmin.Services;

public class CatalogLookupDataService<TLookupData, TReponse>
    : ICatalogLookupDataService<TLookupData>
    where TLookupData : LookupData
    where TReponse : ILookupDataResponse<TLookupData>
{

    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogLookupDataService<TLookupData, TReponse>> _logger;
    private readonly string _apiUrl;

    public CatalogLookupDataService(HttpClient httpClient,
        IOptions<BaseUrlConfiguration> baseUrlConfiguration,
        ILogger<CatalogLookupDataService<TLookupData, TReponse>> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiUrl = baseUrlConfiguration.Value.ApiBase;
    }

    public async Task<List<TLookupData>> List()
    {
        string url;

        // Speciel lookup til både catalogbrands og catalogtype, da det på nuværende tidspunkt er de eneste vi har 'stranglet' ud af monolitten.
        if (typeof(TLookupData) == typeof(CatalogBrand))
        {
            url = "http://localhost:5229/api/catalog/catalogbrands"; // Scuffed løsning, da vi fortsat bruger _apiUrl til andre dele af monolitten.
        }
        else if (typeof(TLookupData) == typeof(CatalogType))
        {
            url = "http://localhost:5229/api/catalog/catalogtypes"; // Scuffed løsning, da vi fortsat bruger _apiUrl til andre dele af monolitten.
        }
        else
        {
            // For other types, build the URL dynamically as before
            var endpointName = typeof(TLookupData).GetCustomAttribute<EndpointAttribute>().Name;
            url = $"{_apiUrl}{endpointName}";
        }

        _logger.LogInformation($"Fetching {typeof(TLookupData).Name} from API. Endpoint: {url}");


        // Fetch data from the resolved URL
        var response = await _httpClient.GetFromJsonAsync<TReponse>(url);

        Console.WriteLine($"Fetched {typeof(TLookupData).Name} List:");
        foreach (var item in response.List)
        {
            Console.WriteLine(item); // Assuming item.ToString() provides useful output
        }
        return response.List;
    }
}
