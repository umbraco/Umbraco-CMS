using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Web;
using Umbraco.Core;

namespace UmbracoExamine.LocalStorage
{
    /// <summary>
    /// When running on Azure websites, we can use the local user's storage space
    /// </summary>
    [Obsolete("This has been superceded by IDirectoryFactory in Examine Core and should not be used")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AzureLocalStorageDirectory : ILocalStorageDirectory
    {
        public DirectoryInfo GetLocalStorageDirectory(NameValueCollection config, string configuredPath)
        {
            var appDomainHash = HttpRuntime.AppDomainAppId.GenerateHash();
            var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "LuceneDir", 
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash,
                //ensure the temp path is consistent with the configured path location
                configuredPath.TrimStart('~', '/').Replace("/", "\\"));
            var azureDir = new DirectoryInfo(cachePath);
            if (azureDir.Exists == false)
                azureDir.Create();
            return azureDir;
        }
    }
}