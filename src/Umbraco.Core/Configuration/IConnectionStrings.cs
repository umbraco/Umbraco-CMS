using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public interface IConnectionStrings
    {
        ConfigConnectionString this[string key]
        {
            get;
        }

        void RemoveConnectionString(string umbracoConnectionName, IIOHelper ioHelper);
        void SaveConnectionString(string connectionString, string providerName, IIOHelper ioHelper);
    }
}
