namespace Umbraco.Core.Configuration
{
    public interface IConnectionStrings
    {
        ConfigConnectionString this[string key]
        {
            get;
        }

        void RemoveConnectionString(string umbracoConnectionName);
        void SaveConnectionString(string connectionString, string providerName);
    }
}
