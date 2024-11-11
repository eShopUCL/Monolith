using BlazorAdmin.Services;
using BlazorShared.Interfaces;
using BlazorShared.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin;

public static class ServicesConfiguration
{
    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {
        //Vi bruger CatalogLookupDataService her i stedet for cached, da der ikke er behov for at benytte localstorage (for lidt tid)
        services.AddScoped<ICatalogLookupDataService<CatalogBrand>, CatalogLookupDataService<CatalogBrand, CatalogBrandResponse>>();
        services.AddScoped<CatalogLookupDataService<CatalogBrand, CatalogBrandResponse>>();
        // Vi gør det samme her, da der ikke er behov for at benytte localstorage
        services.AddScoped<ICatalogLookupDataService<CatalogType>, CatalogLookupDataService<CatalogType, CatalogTypeResponse>>();
        services.AddScoped<CatalogLookupDataService<CatalogType, CatalogTypeResponse>>();
        // Vi gør det samme her, da der ikke er behov for at benytte localstorage
        services.AddScoped<ICatalogItemService, CatalogItemService>();
        services.AddScoped<CatalogItemService>();

        return services;
    }
}
