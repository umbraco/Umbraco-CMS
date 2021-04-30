using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Exposes the XDocument sources from files for the default localization text service and ensure caching is taken care of
    /// </summary>
    public class LocalizedTextServiceFileSources
    {
        private readonly ILogger _logger;
        private readonly IAppPolicyCache _cache;
        private readonly IEnumerable<LocalizedTextServiceSupplementaryFileSource> _supplementFileSources;
        private readonly DirectoryInfo _fileSourceFolder;

        // TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
        private readonly Dictionary<string, CultureInfo> _twoLetterCultureConverter = new Dictionary<string, CultureInfo>();

        private readonly Lazy<Dictionary<CultureInfo, Lazy<XDocument>>> _xmlSources;

        /// <summary>
        /// This is used to configure the file sources with the main file sources shipped with Umbraco and also including supplemental/plugin based
        /// localization files. The supplemental files will be loaded in and merged in after the primary files.
        /// The supplemental files must be named with the 4 letter culture name with a hyphen such as : en-AU.xml
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fileSourceFolder"></param>
        /// <param name="supplementFileSources"></param>
        public LocalizedTextServiceFileSources(
            ILogger logger,
            AppCaches appCaches,
            DirectoryInfo fileSourceFolder,
            IEnumerable<LocalizedTextServiceSupplementaryFileSource> supplementFileSources)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (appCaches == null) throw new ArgumentNullException("cache");
            if (fileSourceFolder == null) throw new ArgumentNullException("fileSourceFolder");

            _logger = logger;
            _cache = appCaches.RuntimeCache;

            //Create the lazy source for the _xmlSources
            _xmlSources = new Lazy<Dictionary<CultureInfo, Lazy<XDocument>>>(() =>
            {
                var result = new Dictionary<CultureInfo, Lazy<XDocument>>();

                if (_fileSourceFolder == null) return result;

                foreach (var fileInfo in _fileSourceFolder.GetFiles("*.xml"))
                {
                    var localCopy = fileInfo;
                    var filename = Path.GetFileNameWithoutExtension(localCopy.FullName).Replace("_", "-");

                    // TODO: Fix this nonsense... would have to wait until v8 to store the language files with their correct
                    // names instead of storing them as 2 letters but actually having a 4 letter culture. So now, we
                    // need to check if the file is 2 letters, then open it to try to find it's 4 letter culture, then use that
                    // if it's successful. We're going to assume (though it seems assuming in the legacy logic is never a great idea)
                    // that any 4 letter file is named with the actual culture that it is!
                    CultureInfo culture = null;
                    if (filename.Length == 2)
                    {
                        //we need to open the file to see if we can read it's 'real' culture, we'll use XmlReader since we don't
                        //want to load in the entire doc into mem just to read a single value
                        using (var fs = fileInfo.OpenRead())
                        using (var reader = XmlReader.Create(fs))
                        {
                            if (reader.IsStartElement())
                            {
                                if (reader.Name == "language")
                                {
                                    if (reader.MoveToAttribute("culture"))
                                    {
                                        var cultureVal = reader.Value;
                                        try
                                        {
                                            culture = CultureInfo.GetCultureInfo(cultureVal);
                                            //add to the tracked dictionary
                                            _twoLetterCultureConverter[filename] = culture;
                                        }
                                        catch (CultureNotFoundException)
                                        {
                                            Current.Logger.Warn<LocalizedTextServiceFileSources, string, string>("The culture {CultureValue} found in the file {CultureFile} is not a valid culture", cultureVal, fileInfo.FullName);
                                            //If the culture in the file is invalid, we'll just hope the file name is a valid culture below, otherwise
                                            // an exception will be thrown.
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (culture == null)
                    {
                        culture = CultureInfo.GetCultureInfo(filename);
                    }

                    //get the lazy value from cache
                    result[culture] = new Lazy<XDocument>(() => _cache.GetCacheItem<XDocument>(
                        string.Format("{0}-{1}", typeof(LocalizedTextServiceFileSources).Name, culture.Name), () =>
                        {
                            XDocument xdoc;

                            //load in primary
                            using (var fs = localCopy.OpenRead())
                            {
                                xdoc = XDocument.Load(fs);
                            }

                            //load in supplementary
                            MergeSupplementaryFiles(culture, xdoc);

                            return xdoc;
                        }, isSliding: true, timeout: TimeSpan.FromMinutes(10), dependentFiles: new[] { localCopy.FullName }));
                }
                return result;
            });

            if (fileSourceFolder.Exists == false)
            {
                Current.Logger.Warn<LocalizedTextServiceFileSources, string>("The folder does not exist: {FileSourceFolder}, therefore no sources will be discovered", fileSourceFolder.FullName);
            }
            else
            {
                _fileSourceFolder = fileSourceFolder;
                _supplementFileSources = supplementFileSources;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LocalizedTextServiceFileSources(ILogger logger, AppCaches appCaches, DirectoryInfo fileSourceFolder)
            : this(logger, appCaches, fileSourceFolder, Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>())
        { }

        /// <summary>
        /// returns all xml sources for all culture files found in the folder
        /// </summary>
        /// <returns></returns>
        public IDictionary<CultureInfo, Lazy<XDocument>> GetXmlSources()
        {
            return _xmlSources.Value;
        }

        // TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
        public Attempt<CultureInfo> TryConvert2LetterCultureTo4Letter(string twoLetterCulture)
        {
            if (twoLetterCulture.Length != 2) return Attempt<CultureInfo>.Fail();

            //This needs to be resolved before continuing so that the _twoLetterCultureConverter cache is initialized
            var resolved = _xmlSources.Value;

            return _twoLetterCultureConverter.ContainsKey(twoLetterCulture)
                ? Attempt.Succeed(_twoLetterCultureConverter[twoLetterCulture])
                : Attempt<CultureInfo>.Fail();
        }

        // TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
        public Attempt<string> TryConvert4LetterCultureTo2Letter(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException("culture");

            //This needs to be resolved before continuing so that the _twoLetterCultureConverter cache is initialized
            var resolved = _xmlSources.Value;

            return _twoLetterCultureConverter.Values.Contains(culture)
                ? Attempt.Succeed(culture.Name.Substring(0, 2))
                : Attempt<string>.Fail();
        }

        private void MergeSupplementaryFiles(CultureInfo culture, XDocument xMasterDoc)
        {
            if (xMasterDoc.Root == null) return;
            if (_supplementFileSources != null)
            {
                //now load in supplementary
                var found = _supplementFileSources.Where(x =>
                {
                    var extension = Path.GetExtension(x.File.FullName);
                    var fileCultureName = Path.GetFileNameWithoutExtension(x.File.FullName).Replace("_", "-").Replace(".user", "");
                    return extension.InvariantEquals(".xml") && (
                        fileCultureName.InvariantEquals(culture.Name)
                        || fileCultureName.InvariantEquals(culture.TwoLetterISOLanguageName)
                    );
                });

                foreach (var supplementaryFile in found)
                {
                    using (var fs = supplementaryFile.File.OpenRead())
                    {
                        XDocument xChildDoc;
                        try
                        {
                            xChildDoc = XDocument.Load(fs);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<LocalizedTextServiceFileSources,string>(ex, "Could not load file into XML {File}", supplementaryFile.File.FullName);
                            continue;
                        }

                        if (xChildDoc.Root == null || xChildDoc.Root.Name != "language") continue;
                        foreach (var xArea in xChildDoc.Root.Elements("area")
                            .Where(x => ((string)x.Attribute("alias")).IsNullOrWhiteSpace() == false))
                        {
                            var areaAlias = (string)xArea.Attribute("alias");

                            var areaFound = xMasterDoc.Root.Elements("area").FirstOrDefault(x => ((string)x.Attribute("alias")) == areaAlias);
                            if (areaFound == null)
                            {
                                //add the whole thing
                                xMasterDoc.Root.Add(xArea);
                            }
                            else
                            {
                                MergeChildKeys(xArea, areaFound, supplementaryFile.OverwriteCoreKeys);
                            }
                        }
                    }
                }
            }
        }

        private void MergeChildKeys(XElement source, XElement destination, bool overwrite)
        {
            if (destination == null) throw new ArgumentNullException("destination");
            if (source == null) throw new ArgumentNullException("source");

            //merge in the child elements
            foreach (var key in source.Elements("key")
                .Where(x => ((string)x.Attribute("alias")).IsNullOrWhiteSpace() == false))
            {
                var keyAlias = (string)key.Attribute("alias");
                var keyFound = destination.Elements("key").FirstOrDefault(x => ((string)x.Attribute("alias")) == keyAlias);
                if (keyFound == null)
                {
                    //append, it doesn't exist
                    destination.Add(key);
                }
                else if (overwrite)
                {
                    //overwrite
                    keyFound.Value = key.Value;
                }
            }
        }
    }
}
