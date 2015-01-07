using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Exposes the XDocument sources from files for the default localization text service and ensure caching is taken care of
    /// </summary>
    public class LocalizedTextServiceFileSources
    {
        private readonly IRuntimeCacheProvider _cache;
        private readonly DirectoryInfo _fileSourceFolder;

        public LocalizedTextServiceFileSources(IRuntimeCacheProvider cache, DirectoryInfo fileSourceFolder)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (fileSourceFolder == null) throw new ArgumentNullException("fileSourceFolder");
            _cache = cache;

            if (fileSourceFolder.Exists == false)
            {
                LogHelper.Warn<LocalizedTextServiceFileSources>("The folder does not exist: {0}, therefore no sources will be discovered", () => fileSourceFolder.FullName);
            }
            else
            {
                _fileSourceFolder = fileSourceFolder;    
            }
        }

        /// <summary>
        /// returns all xml sources for all culture files found in the folder
        /// </summary>
        /// <returns></returns>
        public IDictionary<CultureInfo, Lazy<XDocument>> GetXmlSources()
        {
            var result = new Dictionary<CultureInfo, Lazy<XDocument>>();

            if (_fileSourceFolder == null) return result;

            foreach (var fileInfo in _fileSourceFolder.GetFiles("*.xml"))
            {
                var localCopy = fileInfo;
                var filename = Path.GetFileNameWithoutExtension(localCopy.FullName).Replace("_", "-");
                var culture = CultureInfo.GetCultureInfo(filename);
                //get the lazy value from cache                
                result.Add(culture, new Lazy<XDocument>(() => _cache.GetCacheItem<XDocument>(
                    string.Format("{0}-{1}", typeof (LocalizedTextServiceFileSources).Name, culture.TwoLetterISOLanguageName), () =>
                    {
                        using (var fs = localCopy.OpenRead())
                        {
                            return XDocument.Load(fs);
                        }
                    }, isSliding: true, timeout: TimeSpan.FromMinutes(10), dependentFiles: new[] {localCopy.FullName})));
            }
            return result;
        }
    }
}