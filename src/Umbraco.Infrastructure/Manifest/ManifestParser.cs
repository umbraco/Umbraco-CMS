using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Parses the Main.js file and replaces all tokens accordingly.
/// </summary>
public class ManifestParser : IManifestParser
{
    private static readonly string _utf8Preamble = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

    private readonly IAppPolicyCache _cache;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly IManifestFileProviderFactory _manifestFileProviderFactory;
    private readonly ManifestFilterCollection _filters;
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly IIOHelper _ioHelper;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ILogger<ManifestParser> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ManifestValueValidatorCollection _validators;

    private string _path = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManifestParser" /> class.
    /// </summary>
    public ManifestParser(
        AppCaches appCaches,
        ManifestValueValidatorCollection validators,
        ManifestFilterCollection filters,
        ILogger<ManifestParser> logger,
        IIOHelper ioHelper,
        IHostingEnvironment hostingEnvironment,
        IJsonSerializer jsonSerializer,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IDataValueEditorFactory dataValueEditorFactory,
        IManifestFileProviderFactory manifestFileProviderFactory)
    {
        if (appCaches == null)
        {
            throw new ArgumentNullException(nameof(appCaches));
        }

        _cache = appCaches.RuntimeCache;
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ioHelper = ioHelper;
        _hostingEnvironment = hostingEnvironment;
        AppPluginsPath = "~/App_Plugins";
        _jsonSerializer = jsonSerializer;
        _localizedTextService = localizedTextService;
        _shortStringHelper = shortStringHelper;
        _dataValueEditorFactory = dataValueEditorFactory;
        _manifestFileProviderFactory = manifestFileProviderFactory;
    }

    [Obsolete("Use other ctor - Will be removed in Umbraco 13")]
    public ManifestParser(
        AppCaches appCaches,
        ManifestValueValidatorCollection validators,
        ManifestFilterCollection filters,
        ILogger<ManifestParser> logger,
        IIOHelper ioHelper,
        IHostingEnvironment hostingEnvironment,
        IJsonSerializer jsonSerializer,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IDataValueEditorFactory dataValueEditorFactory)
        : this(
              appCaches,
              validators,
              filters,
              logger,
              ioHelper,
              hostingEnvironment,
              jsonSerializer,
              localizedTextService,
              shortStringHelper,
              dataValueEditorFactory,
              StaticServiceProvider.Instance.GetRequiredService<IManifestFileProviderFactory>())
    {
    }

    public string AppPluginsPath
    {
        get => _path;
        set => _path = value.StartsWith("~/") ? _hostingEnvironment.MapPathContentRoot(value) : value;
    }

    /// <summary>
    ///     Gets all manifests, merged into a single manifest object.
    /// </summary>
    /// <returns></returns>
    public CompositePackageManifest CombinedManifest
        => _cache.GetCacheItem("Umbraco.Core.Manifest.ManifestParser::Manifests", () =>
        {
            IEnumerable<PackageManifest> manifests = GetManifests();
            return MergeManifests(manifests);
        }, new TimeSpan(0, 4, 0))!;

    /// <summary>
    ///     Gets all manifests.
    /// </summary>
    public IEnumerable<PackageManifest> GetManifests()
    {
        var manifests = new List<PackageManifest>();
        IFileProvider? manifestFileProvider = _manifestFileProviderFactory.Create();

        if (manifestFileProvider is null)
        {
            throw new ArgumentNullException(nameof(manifestFileProvider));
        }

        foreach (IFileInfo file in GetManifestFiles(manifestFileProvider, Constants.SystemDirectories.AppPlugins))
        {
            try
            {
                using Stream stream = file.CreateReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var text = reader.ReadToEnd();
                text = TrimPreamble(text);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                PackageManifest manifest = ParseManifest(text);
                manifest.Source = file.PhysicalPath!; // We assure that the PhysicalPath is not null in GetManifestFiles()
                manifests.Add(manifest);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse manifest at '{Path}', ignoring.", file.PhysicalPath);
            }
        }

        _filters.Filter(manifests);

        return manifests;
    }

    /// <summary>
    ///     Parses a manifest.
    /// </summary>
    public PackageManifest ParseManifest(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(text));
        }

        PackageManifest? manifest = JsonConvert.DeserializeObject<PackageManifest>(
            text,
            new DataEditorConverter(_dataValueEditorFactory, _ioHelper, _localizedTextService, _shortStringHelper, _jsonSerializer),
            new ValueValidatorConverter(_validators),
            new DashboardAccessRuleConverter());

        // scripts and stylesheets are raw string, must process here
        for (var i = 0; i < manifest!.Scripts.Length; i++)
        {
            manifest.Scripts[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Scripts[i])!;
        }

        for (var i = 0; i < manifest.Stylesheets.Length; i++)
        {
            manifest.Stylesheets[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Stylesheets[i])!;
        }

        foreach (ManifestContentAppDefinition contentApp in manifest.ContentApps)
        {
            contentApp.View = _ioHelper.ResolveRelativeOrVirtualUrl(contentApp.View);
        }

        foreach (ManifestDashboard dashboard in manifest.Dashboards)
        {
            dashboard.View = _ioHelper.ResolveRelativeOrVirtualUrl(dashboard.View)!;
        }

        foreach (GridEditor gridEditor in manifest.GridEditors)
        {
            gridEditor.View = _ioHelper.ResolveRelativeOrVirtualUrl(gridEditor.View);
            gridEditor.Render = _ioHelper.ResolveRelativeOrVirtualUrl(gridEditor.Render);
        }

        // add property editors that are also parameter editors, to the parameter editors list
        // (the manifest format is kinda legacy)
        var ppEditors = manifest.PropertyEditors.Where(x => (x.Type & EditorType.MacroParameter) > 0).ToList();
        if (ppEditors.Count > 0)
        {
            manifest.ParameterEditors = manifest.ParameterEditors.Union(ppEditors).ToArray();
        }

        return manifest;
    }

    /// <summary>
    ///     Merges all manifests into one.
    /// </summary>
    private static CompositePackageManifest MergeManifests(IEnumerable<PackageManifest> manifests)
    {
        var scripts = new Dictionary<BundleOptions, List<ManifestAssets>>();
        var stylesheets = new Dictionary<BundleOptions, List<ManifestAssets>>();
        var propertyEditors = new List<IDataEditor>();
        var parameterEditors = new List<IDataEditor>();
        var gridEditors = new List<GridEditor>();
        var contentApps = new List<ManifestContentAppDefinition>();
        var dashboards = new List<ManifestDashboard>();
        var sections = new List<ManifestSection>();

        foreach (PackageManifest manifest in manifests)
        {
            if (!scripts.TryGetValue(manifest.BundleOptions, out List<ManifestAssets>? scriptsPerBundleOption))
            {
                scriptsPerBundleOption = new List<ManifestAssets>();
                scripts[manifest.BundleOptions] = scriptsPerBundleOption;
            }

            scriptsPerBundleOption.Add(new ManifestAssets(manifest.PackageName, manifest.Scripts));

            if (!stylesheets.TryGetValue(manifest.BundleOptions, out List<ManifestAssets>? stylesPerBundleOption))
            {
                stylesPerBundleOption = new List<ManifestAssets>();
                stylesheets[manifest.BundleOptions] = stylesPerBundleOption;
            }

            stylesPerBundleOption.Add(new ManifestAssets(manifest.PackageName, manifest.Stylesheets));

            propertyEditors.AddRange(manifest.PropertyEditors);

            parameterEditors.AddRange(manifest.ParameterEditors);

            gridEditors.AddRange(manifest.GridEditors);

            contentApps.AddRange(manifest.ContentApps);

            dashboards.AddRange(manifest.Dashboards);

            sections.AddRange(manifest.Sections.DistinctBy(x => x.Alias, StringComparer.OrdinalIgnoreCase));
        }

        return new CompositePackageManifest(
            propertyEditors,
            parameterEditors,
            gridEditors,
            contentApps,
            dashboards,
            sections,
            scripts.ToDictionary(x => x.Key, x => (IReadOnlyList<ManifestAssets>)x.Value),
            stylesheets.ToDictionary(x => x.Key, x => (IReadOnlyList<ManifestAssets>)x.Value));
    }

    private static string TrimPreamble(string text)
    {
        // strangely StartsWith(preamble) would always return true
        if (text.Substring(0, 1) == _utf8Preamble)
        {
            text = text.Remove(0, _utf8Preamble.Length);
        }

        return text;
    }

    // Gets all manifest files
    private static IEnumerable<IFileInfo> GetManifestFiles(IFileProvider fileProvider, string path)
    {
        var manifestFiles = new List<IFileInfo>();
        IEnumerable<IFileInfo> pluginFolders = fileProvider.GetDirectoryContents(path);

        foreach (IFileInfo pluginFolder in pluginFolders)
        {
            if (!pluginFolder.IsDirectory)
            {
                continue;
            }

            manifestFiles.AddRange(GetNestedManifestFiles(fileProvider, $"{path}/{pluginFolder.Name}"));
        }

        return manifestFiles;
    }

    // Helper method to get all nested package.manifest files (recursively)
    private static IEnumerable<IFileInfo> GetNestedManifestFiles(IFileProvider fileProvider, string path)
    {
        foreach (IFileInfo file in fileProvider.GetDirectoryContents(path))
        {
            if (file.IsDirectory)
            {
                var virtualPath = WebPath.Combine(path, file.Name);

                // Recursively find nested package.manifest files
                foreach (IFileInfo nested in GetNestedManifestFiles(fileProvider, virtualPath))
                {
                    yield return nested;
                }
            }
            else if (file.Name.InvariantEquals("package.manifest") && !string.IsNullOrEmpty(file.PhysicalPath))
            {
                yield return file;
            }
        }
    }
}
