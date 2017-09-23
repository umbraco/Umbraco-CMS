using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml.Linq;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// A utility class for working with CDF config and cache files - use sparingly!
    /// </summary>
    public class ClientDependencyConfiguration
    {
        private readonly ILogger _logger;
        private readonly string _fileName;

        public ClientDependencyConfiguration(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
            _fileName = IOHelper.MapPath(string.Format("{0}/ClientDependency.config", SystemDirectories.Config));
        }

        /// <summary>
        /// Changes the version number in ClientDependency.config to a random value to avoid stale caches
        /// </summary>
        public bool IncreaseVersionNumber()
        {
            try
            {
                var clientDependencyConfigXml = XDocument.Load(_fileName, LoadOptions.PreserveWhitespace);
                if (clientDependencyConfigXml.Root != null)
                {

                    var versionAttribute = clientDependencyConfigXml.Root.Attribute("version");

                    //Set the new version to the hashcode of now
                    var oldVersion = versionAttribute.Value;
                    var newVersion = Math.Abs(DateTime.UtcNow.GetHashCode());

                    versionAttribute.SetValue(newVersion);
                    clientDependencyConfigXml.Save(_fileName, SaveOptions.DisableFormatting);

                    _logger.Info<ClientDependencyConfiguration>(string.Format("Updated version number from {0} to {1}", oldVersion, newVersion));
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error<ClientDependencyConfiguration>("Couldn't update ClientDependency version number", ex);
            }

            return false;
        }

        /// <summary>
        /// Clears the temporary files stored for the ClientDependency folder
        /// </summary>
        /// <param name="currentHttpContext"></param>
        public bool ClearTempFiles(HttpContextBase currentHttpContext)
        {
            var cdfTempDirectories = new HashSet<string>();
            foreach (BaseCompositeFileProcessingProvider provider in ClientDependencySettings.Instance
                .CompositeFileProcessingProviderCollection)
            {
                var path = provider.CompositeFilePath.FullName;
                cdfTempDirectories.Add(path);
            }

            try
            {
                var fullPath = currentHttpContext.Server.MapPath(XmlFileMapper.FileMapVirtualFolder);
                if (fullPath != null)
                {
                    cdfTempDirectories.Add(fullPath);
                }
            }
            catch (Exception ex)
            {
                //invalid path format or something... try/catch to be safe
                _logger.Error<ClientDependencyConfiguration>("Could not get path from ClientDependency.config", ex);
            }

            var success = true;
            foreach (var directory in cdfTempDirectories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.Exists == false)
                    continue;

                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception ex)
                {
                    // Something could be locking the directory or the was another error, making sure we don't break the upgrade installer
                    _logger.Error<ClientDependencyConfiguration>("Could not clear temp files", ex);
                    success = false;
                }
            }

            return success;
        }
    }
}
