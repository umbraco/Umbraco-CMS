using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Exposes the XDocument sources from files for the default localization text service and ensure caching is taken care
///     of
/// </summary>
public class LocalizedTextServiceFileSources
{
    private readonly IAppPolicyCache _cache;
    private readonly IDirectoryContents _directoryContents;
    private readonly DirectoryInfo? _fileSourceFolder;
    private readonly ILogger<LocalizedTextServiceFileSources> _logger;
    private readonly IEnumerable<LocalizedTextServiceSupplementaryFileSource>? _supplementFileSources;

    // TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
    private readonly Dictionary<string, CultureInfo> _twoLetterCultureConverter = new();

    private readonly Lazy<Dictionary<CultureInfo, Lazy<XDocument>>> _xmlSources;

    [Obsolete("Use ctor with all params. This will be removed in Umbraco 12")]
    public LocalizedTextServiceFileSources(
        ILogger<LocalizedTextServiceFileSources> logger,
        AppCaches appCaches,
        DirectoryInfo fileSourceFolder,
        IEnumerable<LocalizedTextServiceSupplementaryFileSource> supplementFileSources)
        : this(
            logger,
            appCaches,
            fileSourceFolder,
            supplementFileSources,
            new NotFoundDirectoryContents())
    {
    }

    /// <summary>
    ///     This is used to configure the file sources with the main file sources shipped with Umbraco and also including
    ///     supplemental/plugin based
    ///     localization files. The supplemental files will be loaded in and merged in after the primary files.
    ///     The supplemental files must be named with the 4 letter culture name with a hyphen such as : en-AU.xml
    /// </summary>
    public LocalizedTextServiceFileSources(
        ILogger<LocalizedTextServiceFileSources> logger,
        AppCaches appCaches,
        DirectoryInfo fileSourceFolder,
        IEnumerable<LocalizedTextServiceSupplementaryFileSource> supplementFileSources,
        IDirectoryContents directoryContents)
    {
        if (appCaches == null)
        {
            throw new ArgumentNullException("appCaches");
        }

        _logger = logger ?? throw new ArgumentNullException("logger");
        _directoryContents = directoryContents;
        _cache = appCaches.RuntimeCache;
        _fileSourceFolder = fileSourceFolder ?? throw new ArgumentNullException("fileSourceFolder");
        _supplementFileSources = supplementFileSources;

        // Create the lazy source for the _xmlSources
        _xmlSources = new Lazy<Dictionary<CultureInfo, Lazy<XDocument>>>(() =>
        {
            var result = new Dictionary<CultureInfo, Lazy<XDocument>>();

            IEnumerable<IFileInfo> files = GetLanguageFiles();

            if (!files.Any())
            {
                return result;
            }

            foreach (IFileInfo fileInfo in files)
            {
                IFileInfo localCopy = fileInfo;
                var filename = Path.GetFileNameWithoutExtension(localCopy.Name).Replace("_", "-");

                // TODO: Fix this nonsense... would have to wait until v8 to store the language files with their correct
                // names instead of storing them as 2 letters but actually having a 4 letter culture. So now, we
                // need to check if the file is 2 letters, then open it to try to find it's 4 letter culture, then use that
                // if it's successful. We're going to assume (though it seems assuming in the legacy logic is never a great idea)
                // that any 4 letter file is named with the actual culture that it is!
                CultureInfo? culture = null;
                if (filename.Length == 2)
                {
                    // we need to open the file to see if we can read it's 'real' culture, we'll use XmlReader since we don't
                    // want to load in the entire doc into mem just to read a single value
                    using (Stream fs = fileInfo.CreateReadStream())
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

                                        // add to the tracked dictionary
                                        _twoLetterCultureConverter[filename] = culture;
                                    }
                                    catch (CultureNotFoundException)
                                    {
                                        _logger.LogWarning(
                                            "The culture {CultureValue} found in the file {CultureFile} is not a valid culture",
                                            cultureVal,
                                            fileInfo.Name);

                                        // If the culture in the file is invalid, we'll just hope the file name is a valid culture below, otherwise
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

                // get the lazy value from cache
                result[culture] = new Lazy<XDocument>(
                    () => _cache.GetCacheItem(
                        string.Format("{0}-{1}", typeof(LocalizedTextServiceFileSources).Name, culture.Name),
                        () =>
                    {
                        XDocument xdoc;

                        // load in primary
                        using (Stream fs = localCopy.CreateReadStream())
                        {
                            xdoc = XDocument.Load(fs);
                        }

                        // load in supplementary
                        MergeSupplementaryFiles(culture, xdoc);

                        return xdoc;
                    },
                        isSliding: true,
                        timeout: TimeSpan.FromMinutes(10))!);
            }

            return result;
        });
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    public LocalizedTextServiceFileSources(ILogger<LocalizedTextServiceFileSources> logger, AppCaches appCaches, DirectoryInfo fileSourceFolder)
        : this(logger, appCaches, fileSourceFolder, Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>())
    {
    }

    /// <summary>
    ///     returns all xml sources for all culture files found in the folder
    /// </summary>
    /// <returns></returns>
    public IDictionary<CultureInfo, Lazy<XDocument>> GetXmlSources() => _xmlSources.Value;

    private IEnumerable<IFileInfo> GetLanguageFiles()
    {
        var result = new List<IFileInfo>();

        if (_fileSourceFolder is not null && _fileSourceFolder.Exists)
        {
            result.AddRange(
                new PhysicalDirectoryContents(_fileSourceFolder.FullName)
                    .Where(x => !x.IsDirectory && x.Name.EndsWith(".xml")));
        }

        if (_directoryContents.Exists)
        {
            result.AddRange(
                _directoryContents
                    .Where(x => !x.IsDirectory && x.Name.EndsWith(".xml")));
        }

        return result;
    }

    // TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
    public Attempt<CultureInfo?> TryConvert2LetterCultureTo4Letter(string twoLetterCulture)
    {
        if (twoLetterCulture.Length != 2)
        {
            return Attempt<CultureInfo?>.Fail();
        }

        // This needs to be resolved before continuing so that the _twoLetterCultureConverter cache is initialized
        Dictionary<CultureInfo, Lazy<XDocument>> resolved = _xmlSources.Value;

        return _twoLetterCultureConverter.ContainsKey(twoLetterCulture)
            ? Attempt.Succeed(_twoLetterCultureConverter[twoLetterCulture])
            : Attempt<CultureInfo?>.Fail();
    }

    // TODO: See other notes in this class, this is purely a hack because we store 2 letter culture file names that contain 4 letter cultures :(
    public Attempt<string?> TryConvert4LetterCultureTo2Letter(CultureInfo culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException("culture");
        }

        // This needs to be resolved before continuing so that the _twoLetterCultureConverter cache is initialized
        Dictionary<CultureInfo, Lazy<XDocument>> resolved = _xmlSources.Value;

        return _twoLetterCultureConverter.Values.Contains(culture)
            ? Attempt.Succeed(culture.Name.Substring(0, 2))
            : Attempt<string?>.Fail();
    }

    private void MergeSupplementaryFiles(CultureInfo culture, XDocument xMasterDoc)
    {
        if (xMasterDoc.Root == null)
        {
            return;
        }

        if (_supplementFileSources != null)
        {
            // now load in supplementary
            IEnumerable<LocalizedTextServiceSupplementaryFileSource> found = _supplementFileSources.Where(x =>
            {
                var extension = Path.GetExtension(x.File.FullName);
                var fileCultureName = Path.GetFileNameWithoutExtension(x.File.FullName).Replace("_", "-")
                    .Replace(".user", string.Empty);
                return extension.InvariantEquals(".xml") && (
                    fileCultureName.InvariantEquals(culture.Name)
                    || fileCultureName.InvariantEquals(culture.TwoLetterISOLanguageName));
            });

            foreach (LocalizedTextServiceSupplementaryFileSource supplementaryFile in found)
            {
                using (FileStream fs = supplementaryFile.File.OpenRead())
                {
                    XDocument xChildDoc;
                    try
                    {
                        xChildDoc = XDocument.Load(fs);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not load file into XML {File}", supplementaryFile.File.FullName);
                        continue;
                    }

                    if (xChildDoc.Root == null || xChildDoc.Root.Name != "language")
                    {
                        continue;
                    }

                    foreach (XElement xArea in xChildDoc.Root.Elements("area")
                                 .Where(x => ((string)x.Attribute("alias")!).IsNullOrWhiteSpace() == false))
                    {
                        var areaAlias = (string)xArea.Attribute("alias")!;

                        XElement? areaFound = xMasterDoc.Root.Elements("area").FirstOrDefault(x => (string)x.Attribute("alias")! == areaAlias);
                        if (areaFound == null)
                        {
                            // add the whole thing
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
        if (destination == null)
        {
            throw new ArgumentNullException("destination");
        }

        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        // merge in the child elements
        foreach (XElement key in source.Elements("key")
                     .Where(x => ((string)x.Attribute("alias")!).IsNullOrWhiteSpace() == false))
        {
            var keyAlias = (string)key.Attribute("alias")!;
            XElement? keyFound = destination.Elements("key")
                .FirstOrDefault(x => (string)x.Attribute("alias")! == keyAlias);
            if (keyFound == null)
            {
                // append, it doesn't exist
                destination.Add(key);
            }
            else if (overwrite)
            {
                // overwrite
                keyFound.Value = key.Value;
            }
        }
    }
}
