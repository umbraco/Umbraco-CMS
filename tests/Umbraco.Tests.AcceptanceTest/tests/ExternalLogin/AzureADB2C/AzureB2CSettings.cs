namespace Umbraco.Cms.Tests.AcceptanceTest.ExternalLogin.AzureADB2C
{
    public class AzureB2CSettings
    {
        public string Domain { get; set; } = Environment.GetEnvironmentVariable("AZUREADB2CDOMAIN") ?? string.Empty;
        public string Tenant { get; set; } = Environment.GetEnvironmentVariable("AZUREADB2CTENANT") ?? string.Empty;
        public string Policy { get; set; } = Environment.GetEnvironmentVariable("AZUREADB2CPOLICY") ?? string.Empty;
        public string ClientId { get; set; } = Environment.GetEnvironmentVariable("AZUREADB2CCLIENTID") ?? string.Empty;
        public string ClientSecret { get; set; } = Environment.GetEnvironmentVariable("AZUREADB2CCLIENTSECRET") ?? string.Empty;
    }
}
