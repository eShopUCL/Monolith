using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class UriComposer : IUriComposer
{
    private readonly CatalogSettings _catalogSettings;

    public UriComposer(CatalogSettings catalogSettings) => _catalogSettings = catalogSettings;

    public string ComposePicUri(string uriTemplate)
    {
        if (string.IsNullOrEmpty(uriTemplate))
        {
            return string.Empty;
        }

        return uriTemplate.Replace("http://4.207.200.245/eshopwebmvc/images", _catalogSettings.CatalogBaseUrl);
    }
}
