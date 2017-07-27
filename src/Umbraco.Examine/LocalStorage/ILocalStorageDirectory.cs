using System.Collections.Specialized;
using System.IO;

namespace Umbraco.Examine.LocalStorage
{
    /// <summary>
    /// Used to resolve the local storage folder
    /// </summary>
    public interface ILocalStorageDirectory
    {
        DirectoryInfo GetLocalStorageDirectory(NameValueCollection config, string configuredPath);
    }
}
