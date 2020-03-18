namespace Umbraco.Core.Configuration
{
    public class ConfigConnectionString
    {
        public ConfigConnectionString(string connectionString, string providerName, string name)
        {
            ConnectionString = connectionString;
            ProviderName = providerName;
            Name = name;
        }

        public string ConnectionString { get; }
        public string ProviderName { get; }
        public string Name { get; }
    }
}
