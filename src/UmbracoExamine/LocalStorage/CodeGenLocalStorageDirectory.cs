using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace UmbracoExamine.LocalStorage
{
    /// <summary>
    /// Use the ASP.Net CodeGen folder to store the index files
    /// </summary>
    /// <remarks>
    /// This is the default implementation - but it comes with it's own limitations - the CodeGen folder is cleared whenever new
    /// DLLs are changed in the /bin folder (among other circumstances) which means the index would be re-synced (or rebuilt) there.
    /// </remarks>
    public sealed class CodeGenLocalStorageDirectory : ILocalStorageDirectory
    {
        public DirectoryInfo GetLocalStorageDirectory(NameValueCollection config, string configuredPath)
        {
            var codegenPath = HttpRuntime.CodegenDir;
            var path = Path.Combine(codegenPath, 
                //ensure the temp path is consistent with the configured path location
                configuredPath.TrimStart('~', '/').Replace("/", "\\"));
            return new DirectoryInfo(path);
        }
    }
}