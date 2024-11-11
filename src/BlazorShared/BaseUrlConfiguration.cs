namespace BlazorShared;

public class BaseUrlConfiguration
{
    public const string CONFIG_NAME = "baseUrls";

    // Skal laves om i fremtiden, når alle microservices er på plads, hentes fra appsettings filerne
    // Laves om til url på vores gateway når denne er på plads
    public string ApiBase { get; set; }
    public string WebBase { get; set; }
}
