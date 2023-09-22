using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
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
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
/// Parses the Main.js file and replaces all tokens accordingly.
/// </summary>
public class LegacyManifestParser : ILegacyManifestParser
{
    private static readonly string _utf8Preamble = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

    private readonly IAppPolicyCache _cache;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;
    private readonly ILegacyPackageManifestFileProviderFactory _legacyPackageManifestFileProviderFactory;
    private readonly ISemVersionFactory _semVersionFactory;
    private readonly LegacyManifestFilterCollection _filters;
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly IIOHelper _ioHelper;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ILogger<LegacyManifestParser> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ManifestValueValidatorCollection _validators;

    private string _path = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyManifestParser" /> class.
    /// </summary>
    public LegacyManifestParser(
        AppCaches appCaches,
        ManifestValueValidatorCollection validators,
        LegacyManifestFilterCollection filters,
        ILogger<LegacyManifestParser> logger,
        IIOHelper ioHelper,
        IHostingEnvironment hostingEnvironment,
        IJsonSerializer jsonSerializer,
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IDataValueEditorFactory dataValueEditorFactory,
        ILegacyPackageManifestFileProviderFactory legacyPackageManifestFileProviderFactory,
        ISemVersionFactory semVersionFactory)
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
        _legacyPackageManifestFileProviderFactory = legacyPackageManifestFileProviderFactory;
        _semVersionFactory = semVersionFactory;
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
    public CompositeLegacyPackageManifest CombinedManifest
        => _cache.GetCacheItem("Umbraco.Core.Manifest.ManifestParser::Manifests", () =>
        {
            IEnumerable<LegacyPackageManifest> manifests = GetManifests();
            return MergeManifests(manifests);
        }, new TimeSpan(0, 4, 0))!;

    /// <summary>
    ///     Gets all manifests.
    /// </summary>
    public IEnumerable<LegacyPackageManifest> GetManifests()
    {
        var manifests = new List<LegacyPackageManifest>();
        IFileProvider? manifestFileProvider = _legacyPackageManifestFileProviderFactory.Create();

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

                LegacyPackageManifest manifest = ParseManifest(text);
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
    public LegacyPackageManifest ParseManifest(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(text));
        }

        LegacyPackageManifest? manifest = JsonConvert.DeserializeObject<LegacyPackageManifest>(
            text,
            new DataEditorConverter(_dataValueEditorFactory, _ioHelper, _localizedTextService, _shortStringHelper, _jsonSerializer),
            new ValueValidatorConverter(_validators),
            new DashboardAccessRuleConverter())!;

        if (string.IsNullOrEmpty(manifest.Version))
        {
            string? assemblyName = manifest.VersionAssemblyName;
            if (string.IsNullOrEmpty(assemblyName))
            {
                // Fallback to package ID
                assemblyName = manifest.PackageId;
            }

            if (!string.IsNullOrEmpty(assemblyName) &&
                TryGetAssemblyInformationalVersion(assemblyName, _semVersionFactory, out string? version))
            {
                manifest.Version = version;
            }
        }

        // scripts and stylesheets are raw string, must process here
        for (var i = 0; i < manifest.Scripts.Length; i++)
        {
            manifest.Scripts[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Scripts[i]);
        }

        for (var i = 0; i < manifest.Stylesheets.Length; i++)
        {
            manifest.Stylesheets[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Stylesheets[i]);
        }

        foreach (LegacyManifestContentAppDefinition contentApp in manifest.ContentApps)
        {
            contentApp.View = _ioHelper.ResolveRelativeOrVirtualUrl(contentApp.View);
        }

        foreach (LegacyManifestDashboard dashboard in manifest.Dashboards)
        {
            dashboard.View = _ioHelper.ResolveRelativeOrVirtualUrl(dashboard.View);
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

    private bool TryGetAssemblyInformationalVersion(string name, ISemVersionFactory semVersionFactory, [NotNullWhen(true)] out string? version)
    {
        foreach (Assembly assembly in AssemblyLoadContext.Default.Assemblies)
        {
            AssemblyName assemblyName = assembly.GetName();
            if (string.Equals(assemblyName.Name, name, StringComparison.OrdinalIgnoreCase) &&
                assembly.TryGetInformationalVersion(semVersionFactory, out version))
            {
                return true;
            }
        }

        version = null;
        return false;
    }

    /// <summary>
    ///     Merges all manifests into one.
    /// </summary>
    private static CompositeLegacyPackageManifest MergeManifests(IEnumerable<LegacyPackageManifest> manifests)
    {
        var scripts = new Dictionary<BundleOptions, List<LegacyManifestAssets>>();
        var stylesheets = new Dictionary<BundleOptions, List<LegacyManifestAssets>>();
        var propertyEditors = new List<IDataEditor>();
        var parameterEditors = new List<IDataEditor>();
        var gridEditors = new List<GridEditor>();
        var contentApps = new List<LegacyManifestContentAppDefinition>();
        var dashboards = new List<LegacyManifestDashboard>();
        var sections = new List<LegacyManifestSection>();

        foreach (LegacyPackageManifest manifest in manifests)
        {
            if (!scripts.TryGetValue(manifest.BundleOptions, out List<LegacyManifestAssets>? scriptsPerBundleOption))
            {
                scriptsPerBundleOption = new List<LegacyManifestAssets>();
                scripts[manifest.BundleOptions] = scriptsPerBundleOption;
            }

            scriptsPerBundleOption.Add(new LegacyManifestAssets(manifest.PackageName, manifest.Scripts));

            if (!stylesheets.TryGetValue(manifest.BundleOptions, out List<LegacyManifestAssets>? stylesPerBundleOption))
            {
                stylesPerBundleOption = new List<LegacyManifestAssets>();
                stylesheets[manifest.BundleOptions] = stylesPerBundleOption;
            }

            stylesPerBundleOption.Add(new LegacyManifestAssets(manifest.PackageName, manifest.Stylesheets));

            propertyEditors.AddRange(manifest.PropertyEditors);

            parameterEditors.AddRange(manifest.ParameterEditors);

            gridEditors.AddRange(manifest.GridEditors);

            contentApps.AddRange(manifest.ContentApps);

            dashboards.AddRange(manifest.Dashboards);

            sections.AddRange(manifest.Sections.DistinctBy(x => x.Alias, StringComparer.OrdinalIgnoreCase));
        }

        return new CompositeLegacyPackageManifest(
            propertyEditors,
            parameterEditors,
            gridEditors,
            contentApps,
            dashboards,
            sections,
            scripts.ToDictionary(x => x.Key, x => (IReadOnlyList<LegacyManifestAssets>)x.Value),
            stylesheets.ToDictionary(x => x.Key, x => (IReadOnlyList<LegacyManifestAssets>)x.Value));
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
