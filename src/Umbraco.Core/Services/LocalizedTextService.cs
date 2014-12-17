using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Cache;

namespace Umbraco.Core.Services
{
    //TODO: Convert all of this over to Niels K's localization framework one day

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
            _fileSourceFolder = fileSourceFolder;
        }

        /// <summary>
        /// returns all xml sources for all culture files found in the folder
        /// </summary>
        /// <returns></returns>
        public IDictionary<CultureInfo, Lazy<XDocument>> GetXmlSources()
        {
            var result = new Dictionary<CultureInfo, Lazy<XDocument>>();
            foreach (var fileInfo in _fileSourceFolder.GetFiles("*.xml"))
            {
                var localCopy = fileInfo;
                var filename = Path.GetFileNameWithoutExtension(localCopy.FullName);
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

    public class LocalizedTextService : ILocalizedTextService
    {
        private readonly IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> _dictionarySource;
        private readonly IDictionary<CultureInfo, Lazy<XDocument>> _xmlSource;

        /// <summary>
        /// Initializes with a file sources instance
        /// </summary>
        /// <param name="fileSources"></param>
        public LocalizedTextService(LocalizedTextServiceFileSources fileSources)
        {
            _xmlSource = fileSources.GetXmlSources();
        }

        /// <summary>
        /// Initializes with an XML source
        /// </summary>
        /// <param name="source"></param>
        public LocalizedTextService(IDictionary<CultureInfo, Lazy<XDocument>> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            _xmlSource = source;
        }

        /// <summary>
        /// Initializes with a source of a dictionary of culture -> areas -> sub dictionary of keys/values
        /// </summary>
        /// <param name="source"></param>
        public LocalizedTextService(IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> source)
        {            
            if (source == null) throw new ArgumentNullException("source");
            _dictionarySource = source;
        }

        public string Localize(string key, CultureInfo culture, object variables)
        {
            Mandate.ParameterNotNull(culture, "culture");

            //This is what the legacy ui service did
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            var keyParts = key.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var area = keyParts.Length > 1 ? keyParts[0] : null;
            var alias = keyParts.Length > 1 ? keyParts[1] : keyParts[0];

            if (_xmlSource != null)
            {
                return GetFromXmlSource(culture, area, alias);
            }
            else
            {
                return GetFromDictionarySource(culture, area, alias);
            }
        }

        private string GetFromDictionarySource(CultureInfo culture, string area, string key)
        {
            if (_dictionarySource.ContainsKey(culture) == false)
            {
                throw new NullReferenceException("The culture specified " + culture + " was not found in any configured sources for this service");
            }

            var cultureSource = _dictionarySource[culture];
            
            string found;
            if (area.IsNullOrWhiteSpace())
            {
                found = cultureSource
                    .SelectMany(x => x.Value)
                    .Where(keyvals => keyvals.Key.InvariantEquals(key))
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }
            else
            {
                found = cultureSource
                    .Where(areas => areas.Key.InvariantEquals(area))
                    .SelectMany(a => a.Value)
                    .Where(keyvals => keyvals.Key.InvariantEquals(key))
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }

            //NOTE: Based on how legacy works, the default text does not contain the area, just the key
            return found ?? "[" + key + "]";
        }

        private string GetFromXmlSource(CultureInfo culture, string area, string key)
        {
            if (_xmlSource.ContainsKey(culture) == false)
            {
                throw new NullReferenceException("The culture specified " + culture + " was not found in any configured sources for this service");
            }

            var cultureSource = _xmlSource[culture].Value;
            
            var xpath = area.IsNullOrWhiteSpace()
                    ? string.Format("//key [@alias = '{0}']", key)
                    : string.Format("//area [@alias = '{0}']/key [@alias = '{1}']", area, key);

            var found = cultureSource.XPathSelectElement(xpath);

            return found == null
                //NOTE: Based on how legacy works, the default text does not contain the area, just the key
                ? "[" + key + "]"
                : found.Value;
        }
     
   

    }
}
