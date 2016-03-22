using System.Collections.Specialized;
using System.IO;

namespace UmbracoExamine.LocalStorage
{
    /// <summary>
    /// Used to resolve the local storage folder
    /// </summary>
    public interface ILocalStorageDirectory
    {
        DirectoryInfo GetLocalStorageDirectory(NameValueCollection config, string configuredPath);
    }
}