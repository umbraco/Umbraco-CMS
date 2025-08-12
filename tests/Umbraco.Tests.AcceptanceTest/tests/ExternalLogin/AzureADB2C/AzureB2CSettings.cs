namespace Umbraco.Cms.Tests.AcceptanceTest.ExternalLogin.AzureADB2C
{
    public class AzureB2CSettings
    {
        public string Domain { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Policy { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
