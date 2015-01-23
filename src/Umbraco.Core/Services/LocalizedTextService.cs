using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Services
{
    //TODO: Convert all of this over to Niels K's localization framework one day

    public class LocalizedTextService : ILocalizedTextService
    {
        private readonly ILogger _logger;
        private readonly IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> _dictionarySource;
        private readonly IDictionary<CultureInfo, Lazy<XDocument>> _xmlSource;

        /// <summary>
        /// Initializes with a file sources instance
        /// </summary>
        /// <param name="fileSources"></param>
        /// <param name="logger"></param>
        public LocalizedTextService(LocalizedTextServiceFileSources fileSources, ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
            _xmlSource = fileSources.GetXmlSources();
        }

        /// <summary>
        /// Initializes with an XML source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        public LocalizedTextService(IDictionary<CultureInfo, Lazy<XDocument>> source, ILogger logger)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (logger == null) throw new ArgumentNullException("logger");
            _xmlSource = source;
            _logger = logger;
        }

        /// <summary>
        /// Initializes with a source of a dictionary of culture -> areas -> sub dictionary of keys/values
        /// </summary>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        public LocalizedTextService(IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> source, ILogger logger)
        {            
            if (source == null) throw new ArgumentNullException("source");
            if (logger == null) throw new ArgumentNullException("logger");
            _dictionarySource = source;
            _logger = logger;
        }

        public string Localize(string key, CultureInfo culture, IDictionary<string, string> tokens = null)
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
                    _logger.Warn<LocalizedTextService>("The culture specified {0} was not found in any configured sources for this service", () => culture);
                    return result;
                }

                //convert all areas + keys to a single key with a '/'
                var areas = _xmlSource[culture].Value.XPathSelectElements("//area");
                foreach (var area in areas)
                {
                    var keys = area.XPathSelectElements("./key");
                    foreach (var key in keys)
                    {
                        var dictionaryKey = string.Format("{0}/{1}", (string) area.Attribute("alias"), (string) key.Attribute("alias"));
                        //there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
                        if (result.ContainsKey(dictionaryKey) == false)
                        {
                            result.Add(dictionaryKey, key.Value);
                        }
                    }
                }
            }
            else
            {
                if (_dictionarySource.ContainsKey(culture) == false)
                {
                    _logger.Warn<LocalizedTextService>("The culture specified {0} was not found in any configured sources for this service", () => culture);
                    return result;
                }

                //convert all areas + keys to a single key with a '/'
                foreach (var area in _dictionarySource[culture])
                {
                    foreach (var key in area.Value)
                    {
                        var dictionaryKey = string.Format("{0}/{1}", area.Key, key.Key);
                        //i don't think it's possible to have duplicates because we're dealing with a dictionary in the first place, but we'll double check here just in case.
                        if (result.ContainsKey(dictionaryKey) == false)
                        {
                            result.Add(dictionaryKey, key.Value);
                        }
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
                _logger.Warn<LocalizedTextService>("The culture specified {0} was not found in any configured sources for this service", () => culture);
                return "[" + key + "]";  
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
                _logger.Warn<LocalizedTextService>("The culture specified {0} was not found in any configured sources for this service", () => culture);
                return "[" + key + "]";                
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
