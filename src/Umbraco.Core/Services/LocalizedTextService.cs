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

        public string Localize(string key, CultureInfo culture, IDictionary<string, string> tokens)
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
                return GetFromXmlSource(culture, area, alias, tokens);
            }
            else
            {
                return GetFromDictionarySource(culture, area, alias, tokens);
            }
        }

        /// <summary>
        /// Returns all key/values in storage for the given culture
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetAllStoredValues(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException("culture");

            var result = new Dictionary<string, string>();

            if (_xmlSource != null)
            {
                if (_xmlSource.ContainsKey(culture) == false)
                {
                    throw new NullReferenceException("The culture specified " + culture + " was not found in any configured sources for this service");
                }

                //convert all areas + keys to a single key with a '/'
                var areas = _xmlSource[culture].Value.XPathSelectElements("//area");
                foreach (var area in areas)
                {
                    var keys = area.XPathSelectElements("./key");
                    foreach (var key in keys)
                    {
                        result.Add(string.Format("{0}/{1}", (string) area.Attribute("alias"), (string) key.Attribute("alias")), key.Value);
                    }
                }
            }
            else
            {
                if (_dictionarySource.ContainsKey(culture) == false)
                {
                    throw new NullReferenceException("The culture specified " + culture + " was not found in any configured sources for this service");
                }

                //convert all areas + keys to a single key with a '/'
                foreach (var area in _dictionarySource[culture])
                {
                    foreach (var key in area.Value)
                    {
                        result.Add(string.Format("{0}/{1}", area.Key, key.Key), key.Value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of all currently supported cultures
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CultureInfo> GetSupportedCultures()
        {
            return _xmlSource != null ? _xmlSource.Keys : _dictionarySource.Keys;
        }

        private string GetFromDictionarySource(CultureInfo culture, string area, string key, IDictionary<string, string> tokens)
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

            if (found != null)
            {
                return ParseTokens(found, tokens);
            }

            //NOTE: Based on how legacy works, the default text does not contain the area, just the key
            return "[" + key + "]";
        }

        private string GetFromXmlSource(CultureInfo culture, string area, string key, IDictionary<string, string> tokens)
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

            if (found != null)
            {
                return ParseTokens(found.Value, tokens);
            }

            //NOTE: Based on how legacy works, the default text does not contain the area, just the key
            return "[" + key + "]";
        }

        /// <summary>
        /// Parses the tokens in the value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is based on how the legacy ui localized text worked, each token was just a sequential value delimited with a % symbol. 
        /// For example: hello %0%, you are %1% !
        /// 
        /// Since we're going to continue using the same language files for now, the token system needs to remain the same. With our new service
        /// we support a dictionary which means in the future we can really have any sort of token system. 
        /// Currently though, the token key's will need to be an integer and sequential - though we aren't going to throw exceptions if that is not the case.
        /// </remarks>
        internal string ParseTokens(string value, IDictionary<string, string> tokens)
        {
            if (tokens == null || tokens.Any() == false)
            {
                return value;
            }

            foreach (var token in tokens)
            {
                value = value.Replace(string.Format("{0}{1}{0}", "%", token.Key), token.Value);
            }

            return value;
        }

    }
}
