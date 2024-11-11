using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared;
using BlazorShared.Attributes;
using BlazorShared.Interfaces;
using BlazorShared.Models;
using FluentValidation.Validators;
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
    private const string _baseUrl = "http://localhost:5229/api/catalog";


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

        // Vi tager direkte fat i CatalogBrand og CatalogType, da vi har implementeret
        // vores CatalogService for disse, og denne har et andet URL end den oprindelige API (_baseUrl)
        // På den måde sikrer vi, at alt det andet data fortsat hentes fra den oprindelige API (_apiUrl)
        // I fremtiden skal vi have ændret dette url til at benytte vores API gateway (Når alle services er implementeret)
        if (typeof(TLookupData) == typeof(CatalogBrand))
        {
            url = $"{_baseUrl}/catalogbrands";
        }
        else if (typeof(TLookupData) == typeof(CatalogType))
        {
            url = $"{_baseUrl}/catalogbrands";
        }
        else
        {
            var endpointName = typeof(TLookupData).GetCustomAttribute<EndpointAttribute>().Name;
            url = $"{_apiUrl}{endpointName}";
        }

        // Vi deserializer til et TLookupData objekt og returnerer dette
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var response = await _httpClient.GetFromJsonAsync<List<TLookupData>>(url, options);

        return response;
    }

}
