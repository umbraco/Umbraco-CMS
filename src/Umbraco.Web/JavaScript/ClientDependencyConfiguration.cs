using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Linq;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using Semver;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web.JavaScript
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
        /// Changes the version number in ClientDependency.config to a hashed value for the version and the DateTime.Day
        /// </summary>
        /// <param name="version">The <see cref="SemVersion">version</see> of Umbraco we're upgrading to</param>
        /// <param name="date">A <see cref="DateTime">date</see> value to use in the hash to prevent this method from updating the version on each startup</param>
        /// <param name="dateFormat">Allows the developer to specify the <see cref="string">date precision</see> for the hash (i.e. "yyyyMMdd" would be a precision for the day)</param>
        /// <returns>Boolean to indicate successful update of the ClientDependency.config file</returns>
        public bool UpdateVersionNumber(SemVersion version, DateTime date, string dateFormat)
        {
            var byteContents = Encoding.Unicode.GetBytes(version + date.ToString(dateFormat));
            
            //This is a way to convert a string to a long
            //see https://www.codeproject.com/Articles/34309/Convert-String-to-bit-Integer
            //We could much more easily use MD5 which would create us an INT but since that is not compliant with
            //hashing standards we have to use SHA
            int intHash;
            using (var hash = SHA256.Create())
            {
                var bytes = hash.ComputeHash(byteContents);

                var longResult = new[] { 0, 8, 16, 24 }
                    .Select(i => BitConverter.ToInt64(bytes, i))
                    .Aggregate((x, y) => x ^ y);

                //CDF requires an INT, and although this isn't fail safe, it will work for our purposes. We are not hashing for crypto purposes
                //so there could be some collisions with this conversion but it's not a problem for our purposes
                //It's also important to note that the long.GetHashCode() implementation in .NET is this: return (int) this ^ (int) (this >> 32);
                //which means that this value will not change per AppDomain like some GetHashCode implementations.
                intHash = longResult.GetHashCode();
            }

            try
            {
                var clientDependencyConfigXml = XDocument.Load(_fileName, LoadOptions.PreserveWhitespace);
                if (clientDependencyConfigXml.Root != null)
                {

                    var versionAttribute = clientDependencyConfigXml.Root.Attribute("version");

                    //Set the new version to the hashcode of now
                    var oldVersion = versionAttribute.Value;
                    var newVersion = Math.Abs(intHash).ToString();

                    //don't update if it's the same version
                    if (oldVersion == newVersion)
                        return false;

                    versionAttribute.SetValue(newVersion);
                    clientDependencyConfigXml.Save(_fileName, SaveOptions.DisableFormatting);

                    _logger.Info<ClientDependencyConfiguration, string, string>("Updated version number from {OldVersion} to {NewVersion}", oldVersion, newVersion);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error<ClientDependencyConfiguration>(ex, "Couldn't update ClientDependency version number");
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
                var fullPath = XmlFileMapper.FileMapDefaultFolder.StartsWith("~/")
                    ? currentHttpContext.Server.MapPath(XmlFileMapper.FileMapDefaultFolder)
                    : XmlFileMapper.FileMapDefaultFolder;
                if (fullPath != null)
                {
                    cdfTempDirectories.Add(fullPath);
                }
            }
            catch (Exception ex)
            {
                //invalid path format or something... try/catch to be safe
                _logger.Error<ClientDependencyConfiguration>(ex, "Could not get path from ClientDependency.config");
            }

            var success = true;
            foreach (var directory in cdfTempDirectories)
            {
                try
                {
                    if (!Directory.Exists(directory))
                        continue;

                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    // Something could be locking the directory or the was another error, making sure we don't break the upgrade installer
                    _logger.Error<ClientDependencyConfiguration>(ex, "Could not clear temp files");
                    success = false;
                }
            }

            return success;
        }
    }
}
