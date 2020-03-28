using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public interface IConfigManipulator
    {
        void RemoveConnectionString();
        void SaveConnectionString(string connectionString, string providerName);
    }
}
