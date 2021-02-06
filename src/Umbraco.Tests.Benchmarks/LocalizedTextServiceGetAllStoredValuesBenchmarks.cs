using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Loggers;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Services.Implement;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Tests.Benchmarks.Config;
using Umbraco.Core.Services;
using Umbraco.Core;
using System.Xml.XPath;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class LocalizedTextServiceGetAllStoredValuesBenchmarks
    {
        private CultureInfo culture;
        private OldLocalizedTextService _dictionaryService;
        private OldLocalizedTextService _xmlService;

        private LocalizedTextService _optimized;
        private LocalizedTextService _optimizedDict;
        [GlobalSetup]
        public void Setup()
        {
            culture = CultureInfo.GetCultureInfo("en-US");
            _dictionaryService = GetDictionaryLocalizedTextService(culture);
            _xmlService = GetXmlService(culture);
            _optimized = GetOptimizedService(culture);
            _optimizedDict = GetOptimizedServiceDict(culture);
            var result1 = _dictionaryService.Localize("language", culture);
            var result2 = _xmlService.Localize("language", culture);
            var result3 = _dictionaryService.GetAllStoredValues(culture);
            var result4 = _xmlService.GetAllStoredValues(culture);
            var result5 = _optimized.GetAllStoredValues(culture);
            var result6 = _xmlService.GetAllStoredValues(culture);
            var result7 = _optimized.GetAllStoredValuesByAreaAndAlias(culture);
        }

        [Benchmark]
        public void OriginalDictionaryGetAll()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _dictionaryService.GetAllStoredValues(culture);
            }

        }

        [Benchmark]
        public void OriginalXmlGetAll()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _xmlService.GetAllStoredValues(culture);
            }

        }

        [Benchmark]
        public void OriginalDictionaryLocalize()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _dictionaryService.Localize("language", culture);
            }

        }


        [Benchmark(Baseline = true)]
        public void OriginalXmlLocalize()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _xmlService.Localize("language", culture);
            }
        }
        [Benchmark]
        public void OptimizedXmlGetAll()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _optimized.GetAllStoredValues(culture);
            }

        }
        [Benchmark]
        public void OptimizedDictGetAll()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _optimizedDict.GetAllStoredValues(culture);
            }
        }

        [Benchmark]
        public void OptimizedDictGetAllV2()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _optimizedDict.GetAllStoredValuesByAreaAndAlias(culture);
            }
        }

        [Benchmark()]
        public void OptimizedXmlLocalize()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _optimized.Localize(null, "language", culture);
            }
        }
        [Benchmark()]
        public void OptimizedDictLocalize()
        {
            for (int i = 0; i < 10000; i++)
            {
                var result = _optimizedDict.Localize(null, "language", culture);
            }
        }

        private static LocalizedTextService GetOptimizedServiceDict(CultureInfo culture)
        {
            return new LocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea1", new Dictionary<string, string>
                                {
                                    {"testKey1", "testValue1"},
                                    {"testKey2", "testValue2"}
                                }
                            },
                            {
                                "testArea2", new Dictionary<string, string>
                                {
                                    {"blah1", "blahValue1"},
                                    {"blah2", "blahValue2"}
                                }
                            },
                        }
                    }
                }, Mock.Of<Umbraco.Core.Logging.ILogger>());
        }
        private static LocalizedTextService GetOptimizedService(CultureInfo culture)
        {
            var txtService = new LocalizedTextService(new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("language",
                                new XElement("area", new XAttribute("alias", "testArea1"),
                                    new XElement("key", new XAttribute("alias", "testKey1"), "testValue1"),
                                    new XElement("key", new XAttribute("alias", "testKey2"), "testValue2")),
                                new XElement("area", new XAttribute("alias", "testArea2"),
                                    new XElement("key", new XAttribute("alias", "blah1"), "blahValue1"),
                                    new XElement("key", new XAttribute("alias", "blah2"), "blahValue2")))))
                    }
                }, Mock.Of<Umbraco.Core.Logging.ILogger>());
            return txtService;
        }

        private static OldLocalizedTextService GetXmlService(CultureInfo culture)
        {
            var txtService = new OldLocalizedTextService(new Dictionary<CultureInfo, Lazy<XDocument>>
                {
                    {
                        culture, new Lazy<XDocument>(() => new XDocument(
                            new XElement("language",
                                new XElement("area", new XAttribute("alias", "testArea1"),
                                    new XElement("key", new XAttribute("alias", "testKey1"), "testValue1"),
                                    new XElement("key", new XAttribute("alias", "testKey2"), "testValue2")),
                                new XElement("area", new XAttribute("alias", "testArea2"),
                                    new XElement("key", new XAttribute("alias", "blah1"), "blahValue1"),
                                    new XElement("key", new XAttribute("alias", "blah2"), "blahValue2")))))
                    }
                }, Mock.Of<Umbraco.Core.Logging.ILogger>());
            return txtService;
        }

        private static OldLocalizedTextService GetDictionaryLocalizedTextService(CultureInfo culture)
        {
            return new OldLocalizedTextService(
                new Dictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>>
                {
                    {
                        culture, new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea1", new Dictionary<string, string>
                                {
                                    {"testKey1", "testValue1"},
                                    {"testKey2", "testValue2"}
                                }
                            },
                            {
                                "testArea2", new Dictionary<string, string>
                                {
                                    {"blah1", "blahValue1"},
                                    {"blah2", "blahValue2"}
                                }
                            },
                        }
                    }
                }, Mock.Of<Umbraco.Core.Logging.ILogger>());
        }
    }

    //Original
    public class OldLocalizedTextService : ILocalizedTextService
    {
        private readonly ILogger _logger;
        private readonly Lazy<LocalizedTextServiceFileSources> _fileSources;
        private readonly IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> _dictionarySource;
        private readonly IDictionary<CultureInfo, Lazy<XDocument>> _xmlSource;

        /// <summary>
        /// Initializes with a file sources instance
        /// </summary>
        /// <param name="fileSources"></param>
        /// <param name="logger"></param>
        public OldLocalizedTextService(Lazy<LocalizedTextServiceFileSources> fileSources, ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
            if (fileSources == null) throw new ArgumentNullException("fileSources");
            _fileSources = fileSources;
        }

        /// <summary>
        /// Initializes with an XML source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        public OldLocalizedTextService(IDictionary<CultureInfo, Lazy<XDocument>> source, ILogger logger)
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
        public OldLocalizedTextService(IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> source, ILogger logger)
        {
            _dictionarySource = source ?? throw new ArgumentNullException(nameof(source));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Localize(string key, CultureInfo culture, IDictionary<string, string> tokens = null)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));

            // TODO: Hack, see notes on ConvertToSupportedCultureWithRegionCode
            culture = ConvertToSupportedCultureWithRegionCode(culture);

            //This is what the legacy ui service did
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            var keyParts = key.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var area = keyParts.Length > 1 ? keyParts[0] : null;
            var alias = keyParts.Length > 1 ? keyParts[1] : keyParts[0];

            var xmlSource = _xmlSource ?? (_fileSources != null
                ? _fileSources.Value.GetXmlSources()
                : null);

            if (xmlSource != null)
            {
                return GetFromXmlSource(xmlSource, culture, area, alias, tokens);
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

            // TODO: Hack, see notes on ConvertToSupportedCultureWithRegionCode
            culture = ConvertToSupportedCultureWithRegionCode(culture);

            var result = new Dictionary<string, string>();

            var xmlSource = _xmlSource ?? (_fileSources != null
                ? _fileSources.Value.GetXmlSources()
                : null);

            if (xmlSource != null)
            {
                if (xmlSource.ContainsKey(culture) == false)
                {
                    _logger.Warn<OldLocalizedTextService>("The culture specified {Culture} was not found in any configured sources for this service", culture);
                    return result;
                }

                //convert all areas + keys to a single key with a '/'
                result = GetStoredTranslations(xmlSource, culture);

                //merge with the English file in case there's keys in there that don't exist in the local file
                var englishCulture = new CultureInfo("en-US");
                if (culture.Equals(englishCulture) == false)
                {
                    var englishResults = GetStoredTranslations(xmlSource, englishCulture);
                    foreach (var englishResult in englishResults.Where(englishResult => result.ContainsKey(englishResult.Key) == false))
                        result.Add(englishResult.Key, englishResult.Value);
                }
            }
            else
            {
                if (_dictionarySource.ContainsKey(culture) == false)
                {
                    _logger.Warn<OldLocalizedTextService>("The culture specified {Culture} was not found in any configured sources for this service", culture);
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

        private Dictionary<string, string> GetStoredTranslations(IDictionary<CultureInfo, Lazy<XDocument>> xmlSource, CultureInfo cult)
        {
            var result = new Dictionary<string, string>();
            var areas = xmlSource[cult].Value.XPathSelectElements("//area");
            foreach (var area in areas)
            {
                var keys = area.XPathSelectElements("./key");
                foreach (var key in keys)
                {
                    var dictionaryKey = string.Format("{0}/{1}", (string)area.Attribute("alias"),
                        (string)key.Attribute("alias"));
                    //there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
                    if (result.ContainsKey(dictionaryKey) == false)
                        result.Add(dictionaryKey, key.Value);
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
            var xmlSource = _xmlSource ?? (_fileSources != null
                ? _fileSources.Value.GetXmlSources()
                : null);

            return xmlSource != null ? xmlSource.Keys : _dictionarySource.Keys;
        }

        /// <summary>
        /// Tries to resolve a full 4 letter culture from a 2 letter culture name
        /// </summary>
        /// <param name="currentCulture">
        /// The culture to determine if it is only a 2 letter culture, if so we'll try to convert it, otherwise it will just be returned
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: This is just a hack due to the way we store the language files, they should be stored with 4 letters since that
        /// is what they reference but they are stored with 2, further more our user's languages are stored with 2. So this attempts
        /// to resolve the full culture if possible.
        ///
        /// This only works when this service is constructed with the LocalizedTextServiceFileSources
        /// </remarks>
        public CultureInfo ConvertToSupportedCultureWithRegionCode(CultureInfo currentCulture)
        {
            if (currentCulture == null) throw new ArgumentNullException("currentCulture");

            if (_fileSources == null) return currentCulture;
            if (currentCulture.Name.Length > 2) return currentCulture;

            var attempt = _fileSources.Value.TryConvert2LetterCultureTo4Letter(currentCulture.TwoLetterISOLanguageName);
            return attempt ? attempt.Result : currentCulture;
        }

        private string GetFromDictionarySource(CultureInfo culture, string area, string key, IDictionary<string, string> tokens)
        {
            if (_dictionarySource.ContainsKey(culture) == false)
            {
                _logger.Warn<OldLocalizedTextService>("The culture specified {Culture} was not found in any configured sources for this service", culture);
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

        private string GetFromXmlSource(IDictionary<CultureInfo, Lazy<XDocument>> xmlSource, CultureInfo culture, string area, string key, IDictionary<string, string> tokens)
        {
            if (xmlSource.ContainsKey(culture) == false)
            {
                _logger.Warn<OldLocalizedTextService>("The culture specified {Culture} was not found in any configured sources for this service", culture);
                return "[" + key + "]";
            }

            var found = FindTranslation(xmlSource, culture, area, key);

            if (found != null)
            {
                return ParseTokens(found.Value, tokens);
            }

            // Fall back to English by default if we can't find the key
            found = FindTranslation(xmlSource, new CultureInfo("en-US"), area, key);
            if (found != null)
                return ParseTokens(found.Value, tokens);

            // If it can't be found in either file, fall back  to the default, showing just the key in square brackets
            // NOTE: Based on how legacy works, the default text does not contain the area, just the key
            return "[" + key + "]";
        }

        private XElement FindTranslation(IDictionary<CultureInfo, Lazy<XDocument>> xmlSource, CultureInfo culture, string area, string key)
        {
            var cultureSource = xmlSource[culture].Value;

            var xpath = area.IsNullOrWhiteSpace()
                ? string.Format("//key [@alias = '{0}']", key)
                : string.Format("//area [@alias = '{0}']/key [@alias = '{1}']", area, key);

            var found = cultureSource.XPathSelectElement(xpath);
            return found;
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
        internal static string ParseTokens(string value, IDictionary<string, string> tokens)
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

//    // * Summary *

//    BenchmarkDotNet=v0.11.3, OS=Windows 10.0.18362
//Intel Core i5-8265U CPU 1.60GHz(Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
// [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4250.0
//  Job-JIATTD : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4250.0

//IterationCount=3  IterationTime=100.0000 ms LaunchCount = 1
//WarmupCount=3

//                Method |      Mean |      Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
//---------------------- |----------:|-----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
//      DictionaryGetAll | 11.199 ms |  1.8170 ms | 0.0996 ms |  0.14 |   1888.8889 |           - |           - |          5868.59 KB |
//             XmlGetAll | 62.963 ms | 24.0615 ms | 1.3189 ms |  0.81 |  13500.0000 |           - |           - |         42448.71 KB |
//    DictionaryLocalize |  9.757 ms |  1.6966 ms | 0.0930 ms |  0.13 |   1100.0000 |           - |           - |          3677.65 KB |
//           XmlLocalize | 77.725 ms | 14.6069 ms | 0.8007 ms |  1.00 |  14000.0000 |           - |           - |          43032.8 KB |
//  OptimizedXmlLocalize |  2.402 ms |  0.4256 ms | 0.0233 ms |  0.03 |    187.5000 |           - |           - |           626.01 KB |
// OptimizedDictLocalize |  2.345 ms |  0.2411 ms | 0.0132 ms |  0.03 |    187.5000 |           - |           - |           626.01 KB |

//// * Warnings *
//MinIterationTime
//  LocalizedTextServiceGetAllStoredValuesBenchmarks.DictionaryGetAll: IterationCount= 3, IterationTime= 100.0000 ms, LaunchCount= 1, WarmupCount= 3->MinIterationTime = 99.7816 ms which is very small. It's recommended to increase it.
//  LocalizedTextServiceGetAllStoredValuesBenchmarks.DictionaryLocalize: IterationCount= 3, IterationTime= 100.0000 ms, LaunchCount= 1, WarmupCount= 3->MinIterationTime = 96.7415 ms which is very small. It's recommended to increase it.
//  LocalizedTextServiceGetAllStoredValuesBenchmarks.XmlLocalize: IterationCount= 3, IterationTime= 100.0000 ms, LaunchCount= 1, WarmupCount= 3->MinIterationTime = 76.8151 ms which is very small. It's recommended to increase it.

//// * Legends *
//  Mean                : Arithmetic mean of all measurements
//  Error               : Half of 99.9% confidence interval
//  StdDev              : Standard deviation of all measurements
//  Ratio               : Mean of the ratio distribution ([Current]/[Baseline])
//  Gen 0/1k Op         : GC Generation 0 collects per 1k Operations
//  Gen 1/1k Op         : GC Generation 1 collects per 1k Operations
//  Gen 2/1k Op         : GC Generation 2 collects per 1k Operations
//  Allocated Memory/Op : Allocated memory per single operation(managed only, inclusive, 1KB = 1024B)
//  1 ms                : 1 Millisecond(0.001 sec)

//// * Diagnostic Output - MemoryDiagnoser *


//    // ***** BenchmarkRunner: End *****
//    Run time: 00:00:09 (9.15 sec), executed benchmarks: 6
}
