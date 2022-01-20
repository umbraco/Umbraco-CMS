using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest
{
    /// <summary>
    /// Parses the Main.js file and replaces all tokens accordingly.
    /// </summary>
    public class ManifestParser : IManifestParser
    {

        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IDataValueEditorFactory _dataValueEditorFactory;
        private static readonly string s_utf8Preamble = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        private readonly IAppPolicyCache _cache;
        private readonly ILogger<ManifestParser> _logger;
        private readonly ManifestValueValidatorCollection _validators;
        private readonly ManifestFilterCollection _filters;

        private string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestParser"/> class.
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
            IDataValueEditorFactory dataValueEditorFactory)
            : this(appCaches, validators, filters, "~/App_Plugins", logger, ioHelper, hostingEnvironment)
        {
            _jsonSerializer = jsonSerializer;
            _localizedTextService = localizedTextService;
            _shortStringHelper = shortStringHelper;
            _dataValueEditorFactory = dataValueEditorFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestParser"/> class.
        /// </summary>
        private ManifestParser(AppCaches appCaches, ManifestValueValidatorCollection validators, ManifestFilterCollection filters, string appPluginsPath, ILogger<ManifestParser> logger,  IIOHelper ioHelper, IHostingEnvironment hostingEnvironment)
        {
            if (appCaches == null) throw new ArgumentNullException(nameof(appCaches));
            _cache = appCaches.RuntimeCache;
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (appPluginsPath == null) throw new ArgumentNullException(nameof(appPluginsPath));
            if (string.IsNullOrWhiteSpace(appPluginsPath)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(appPluginsPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;

            AppPluginsPath = appPluginsPath;
        }

        public string AppPluginsPath
        {
            get => _path;
            set => _path = value.StartsWith("~/") ? _hostingEnvironment.MapPathContentRoot(value) : value;
        }

        /// <summary>
        /// Gets all manifests, merged into a single manifest object.
        /// </summary>
        /// <returns></returns>
        public CompositePackageManifest CombinedManifest
            => _cache.GetCacheItem<CompositePackageManifest>("Umbraco.Core.Manifest.ManifestParser::Manifests", () =>
            {
                IEnumerable<PackageManifest> manifests = GetManifests();
                return MergeManifests(manifests);

            }, new TimeSpan(0, 4, 0));

        /// <summary>
        /// Gets all manifests.
        /// </summary>
        public IEnumerable<PackageManifest> GetManifests()
        {
            var manifests = new List<PackageManifest>();

            foreach (var path in GetManifestFiles())
            {
                try
                {
                    var text = File.ReadAllText(path);
                    text = TrimPreamble(text);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    PackageManifest manifest = ParseManifest(text);
                    manifest.Source = path;
                    manifests.Add(manifest);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to parse manifest at '{Path}', ignoring.", path);
                }
            }

            _filters.Filter(manifests);

            return manifests;
        }

        /// <summary>
        /// Merges all manifests into one.
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
                if (manifest.Scripts != null)
                {
                    if (!scripts.TryGetValue(manifest.BundleOptions, out List<ManifestAssets> scriptsPerBundleOption))
                    {
                        scriptsPerBundleOption = new List<ManifestAssets>();
                        scripts[manifest.BundleOptions] = scriptsPerBundleOption;
                    }
                    scriptsPerBundleOption.Add(new ManifestAssets(manifest.PackageName, manifest.Scripts));
                }

                if (manifest.Stylesheets != null)
                {
                    if (!stylesheets.TryGetValue(manifest.BundleOptions, out List<ManifestAssets> stylesPerBundleOption))
                    {
                        stylesPerBundleOption = new List<ManifestAssets>();
                        stylesheets[manifest.BundleOptions] = stylesPerBundleOption;
                    }
                    stylesPerBundleOption.Add(new ManifestAssets(manifest.PackageName, manifest.Stylesheets));
                }

                if (manifest.PropertyEditors != null) propertyEditors.AddRange(manifest.PropertyEditors);
                if (manifest.ParameterEditors != null) parameterEditors.AddRange(manifest.ParameterEditors);
                if (manifest.GridEditors != null) gridEditors.AddRange(manifest.GridEditors);
                if (manifest.ContentApps != null) contentApps.AddRange(manifest.ContentApps);
                if (manifest.Dashboards != null) dashboards.AddRange(manifest.Dashboards);
                if (manifest.Sections != null) sections.AddRange(manifest.Sections.DistinctBy(x => x.Alias.ToLowerInvariant()));
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

        // gets all manifest files (recursively)
        private IEnumerable<string> GetManifestFiles()
        {
            if (Directory.Exists(_path) == false)
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(_path, "package.manifest", SearchOption.AllDirectories);
        }

        private static string TrimPreamble(string text)
        {
            // strangely StartsWith(preamble) would always return true
            if (text.Substring(0, 1) == s_utf8Preamble)
                text = text.Remove(0, s_utf8Preamble.Length);

            return text;
        }

        /// <summary>
        /// Parses a manifest.
        /// </summary>
        public PackageManifest ParseManifest(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(text));

            var manifest = JsonConvert.DeserializeObject<PackageManifest>(text,
                new DataEditorConverter(_dataValueEditorFactory, _ioHelper, _localizedTextService, _shortStringHelper, _jsonSerializer),
                new ValueValidatorConverter(_validators),
                new DashboardAccessRuleConverter());

            // scripts and stylesheets are raw string, must process here
            for (var i = 0; i < manifest.Scripts.Length; i++)
                manifest.Scripts[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Scripts[i]);
            for (var i = 0; i < manifest.Stylesheets.Length; i++)
                manifest.Stylesheets[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Stylesheets[i]);
            foreach (var contentApp in manifest.ContentApps)
            {
                contentApp.View = _ioHelper.ResolveRelativeOrVirtualUrl(contentApp.View);
            }
            foreach (var dashboard in manifest.Dashboards)
            {
                dashboard.View = _ioHelper.ResolveRelativeOrVirtualUrl(dashboard.View);
            }
            foreach (var gridEditor in manifest.GridEditors)
            {
                gridEditor.View = _ioHelper.ResolveRelativeOrVirtualUrl(gridEditor.View);
                gridEditor.Render = _ioHelper.ResolveRelativeOrVirtualUrl(gridEditor.Render);
            }

            // add property editors that are also parameter editors, to the parameter editors list
            // (the manifest format is kinda legacy)
            var ppEditors = manifest.PropertyEditors.Where(x => (x.Type & EditorType.MacroParameter) > 0).ToList();
            if (ppEditors.Count > 0)
                manifest.ParameterEditors = manifest.ParameterEditors.Union(ppEditors).ToArray();

            return manifest;
        }
    }
}
