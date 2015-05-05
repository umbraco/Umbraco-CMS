using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
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

        //TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
        private readonly Dictionary<string, CultureInfo> _twoLetterCultureConverter = new Dictionary<string, CultureInfo>();

        private readonly Lazy<Dictionary<CultureInfo, Lazy<XDocument>>> _xmlSources;

        public LocalizedTextServiceFileSources(IRuntimeCacheProvider cache, DirectoryInfo fileSourceFolder)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (fileSourceFolder == null) throw new ArgumentNullException("fileSourceFolder");

            _cache = cache;

            //Create the lazy source for the _xmlSources
            _xmlSources = new Lazy<Dictionary<CultureInfo, Lazy<XDocument>>>(() =>
            {
                var result = new Dictionary<CultureInfo, Lazy<XDocument>>();

                if (_fileSourceFolder == null) return result;

                foreach (var fileInfo in _fileSourceFolder.GetFiles("*.xml"))
                {
                    var localCopy = fileInfo;
                    var filename = Path.GetFileNameWithoutExtension(localCopy.FullName).Replace("_", "-");

                    //TODO: Fix this nonsense... would have to wait until v8 to store the language files with their correct
                    // names instead of storing them as 2 letters but actually having a 4 letter culture. wtf. So now, we
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
                                            LogHelper.Warn<LocalizedTextServiceFileSources>(
                                                string.Format("The culture {0} found in the file {1} is not a valid culture", cultureVal, fileInfo.FullName));
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
                        string.Format("{0}-{1}", typeof (LocalizedTextServiceFileSources).Name, culture.Name), () =>
                        {
                            using (var fs = localCopy.OpenRead())
                            {
                                return XDocument.Load(fs);
                            }
                        }, isSliding: true, timeout: TimeSpan.FromMinutes(10), dependentFiles: new[] {localCopy.FullName}));
                }
                return result;
            });

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
            return _xmlSources.Value;
        }

        //TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
        public Attempt<CultureInfo> TryConvert2LetterCultureTo4Letter(string twoLetterCulture)
        {
            if (twoLetterCulture.Length != 2) Attempt<CultureInfo>.Fail();

            //This needs to be resolved before continuing so that the _twoLetterCultureConverter cache is initialized
            var resolved = _xmlSources.Value;

            return _twoLetterCultureConverter.ContainsKey(twoLetterCulture)
                ? Attempt.Succeed(_twoLetterCultureConverter[twoLetterCulture])
                : Attempt<CultureInfo>.Fail();
        }
    }
}