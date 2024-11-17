using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorAdmin.Services;
using BlazorShared.Interfaces;
using BlazorShared.Models;

namespace BlazorAdmin.Pages.CatalogItemPage;

public partial class List : BlazorComponent
{
    [Microsoft.AspNetCore.Components.Inject]
    public CatalogItemService CatalogItemService { get; set; }

    [Microsoft.AspNetCore.Components.Inject]
    public ICatalogLookupDataService<CatalogBrand> CatalogBrandService { get; set; }

    [Microsoft.AspNetCore.Components.Inject]
    public ICatalogLookupDataService<CatalogType> CatalogTypeService { get; set; }

    private List<CatalogItem> catalogItems = new List<CatalogItem>();
    private List<CatalogType> catalogTypes = new List<CatalogType>();
    private List<CatalogBrand> catalogBrands = new List<CatalogBrand>();

    private Edit EditComponent { get; set; }
    private Delete DeleteComponent { get; set; }
    private Details DetailsComponent { get; set; }
    private Create CreateComponent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Kalder .List metoden for hver Service for at hente objekterne fra
        // vores CatalogService API.
        catalogItems = await CatalogItemService.List();
        catalogTypes = await CatalogTypeService.List();
        catalogBrands = await CatalogBrandService.List();

        // Her mapper vi hvert catalogItem med CatalogType og CatalogBrand navne
        // Vi benytter os af catalogItem.catalogTypeId og catalogItem.catalogBrandId
        // Disse benyttes til at finde navnet på typen og brandet
        foreach (var item in catalogItems)
        {
            item.CatalogType = catalogTypes.FirstOrDefault(t => t.Id == item.CatalogTypeId)?.Name;
            item.CatalogBrand = catalogBrands.FirstOrDefault(b => b.Id == item.CatalogBrandId)?.Name;
        }
        
        // Kalder StateHasChanged for at opdatere UI
        StateHasChanged();
    }

    private async Task DetailsClick(int id)
    {
        await DetailsComponent.Open(id);
    }

    private async Task CreateClick()
    {
        await CreateComponent.Open();
    }

    private async Task EditClick(int id)
    {
        await EditComponent.Open(id);
    }

    private async Task DeleteClick(int id)
    {
        await DeleteComponent.Open(id);
    }

    private async Task ReloadCatalogItems()
    {
        catalogItems = await CatalogItemService.List();

        // Map CatalogType og CatalogBrand navne igen, hvis det er blevet opdateret
        foreach (var item in catalogItems)
        {
            item.CatalogType = catalogTypes.FirstOrDefault(t => t.Id == item.CatalogTypeId)?.Name;
            item.CatalogBrand = catalogBrands.FirstOrDefault(b => b.Id == item.CatalogBrandId)?.Name;
        }

        StateHasChanged();
    }
}
