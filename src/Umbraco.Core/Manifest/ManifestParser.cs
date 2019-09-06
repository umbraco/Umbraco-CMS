using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Umbraco.Core.Cache;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    internal class ManifestParserImpl : ManifestParser
    {
        public ManifestParserImpl(AppCaches appCaches, ManifestValueValidatorCollection validators, ManifestFilterCollection filters, ILogger logger)
            : base(appCaches, validators, filters, logger)
        {
        }
    }

    /// <summary>
    /// Parses the Main.js file and replaces all tokens accordingly.
    /// </summary>
    public abstract class ManifestParser
    {
        private static readonly string Utf8Preamble = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        protected readonly IAppPolicyCache Cache;
        protected readonly ILogger Logger;
        protected readonly ManifestValueValidatorCollection Validators;
        protected readonly ManifestFilterCollection Filters;

        private string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestParser"/> class.
        /// </summary>
        protected ManifestParser(AppCaches appCaches, ManifestValueValidatorCollection validators, ManifestFilterCollection filters, ILogger logger)
            : this(appCaches, validators, filters, "~/App_Plugins", logger)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestParser"/> class.
        /// </summary>
        private ManifestParser(AppCaches appCaches, ManifestValueValidatorCollection validators, ManifestFilterCollection filters, string path, ILogger logger)
        {
            if (appCaches == null) throw new ArgumentNullException(nameof(appCaches));
            Cache = appCaches.RuntimeCache;
            Validators = validators ?? throw new ArgumentNullException(nameof(validators));
            Filters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullOrEmptyException(nameof(path));
            Path = path;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual string Path
        {
            get => _path;
            set => _path = value.StartsWith("~/") ? IOHelper.MapPath(value) : value;
        }

        /// <summary>
        /// Gets all manifests, merged into a single manifest object.
        /// </summary>
        /// <returns></returns>
        public virtual PackageManifest Manifest
            => Cache.GetCacheItem<PackageManifest>("Umbraco.Core.Manifest.ManifestParser::Manifests", () =>
            {
                var manifests = GetManifests();
                return MergeManifests(manifests);
            }, new TimeSpan(0, 4, 0));

        /// <summary>
        /// Gets all manifests.
        /// </summary>
        protected virtual IEnumerable<PackageManifest> GetManifests()
        {
            var manifests = new List<PackageManifest>();

            foreach (var path in GetManifestFiles())
            {
                try
                {
                    var text = File.ReadAllText(path);
                    text = TrimPreamble(text);
                    if (string.IsNullOrWhiteSpace(text))
                        continue;
                    var manifest = ParseManifest(text);
                    manifest.Source = path;
                    manifests.Add(manifest);
                }
                catch (Exception e)
                {
                    Logger.Error<ManifestParser>(e, "Failed to parse manifest at '{Path}', ignoring.", path);
                }
            }

            Filters.Filter(manifests);

            return manifests;
        }

        /// <summary>
        /// Merges all manifests into one.
        /// </summary>
        protected static PackageManifest MergeManifests(IEnumerable<PackageManifest> manifests)
        {
            var scripts = new HashSet<string>();
            var stylesheets = new HashSet<string>();
            var propertyEditors = new List<IDataEditor>();
            var parameterEditors = new List<IDataEditor>();
            var gridEditors = new List<GridEditor>();
            var contentApps = new List<ManifestContentAppDefinition>();
            var dashboards = new List<ManifestDashboard>();
            var sections = new List<ManifestSection>();

            foreach (var manifest in manifests)
            {
                if (manifest.Scripts != null) foreach (var script in manifest.Scripts) scripts.Add(script);
                if (manifest.Stylesheets != null) foreach (var stylesheet in manifest.Stylesheets) stylesheets.Add(stylesheet);
                if (manifest.PropertyEditors != null) propertyEditors.AddRange(manifest.PropertyEditors);
                if (manifest.ParameterEditors != null) parameterEditors.AddRange(manifest.ParameterEditors);
                if (manifest.GridEditors != null) gridEditors.AddRange(manifest.GridEditors);
                if (manifest.ContentApps != null) contentApps.AddRange(manifest.ContentApps);
                if (manifest.Dashboards != null) dashboards.AddRange(manifest.Dashboards);
                if (manifest.Sections != null) sections.AddRange(manifest.Sections.DistinctBy(x => x.Alias.ToLowerInvariant()));
            }

            return new PackageManifest
            {
                Scripts = scripts.ToArray(),
                Stylesheets = stylesheets.ToArray(),
                PropertyEditors = propertyEditors.ToArray(),
                ParameterEditors = parameterEditors.ToArray(),
                GridEditors = gridEditors.ToArray(),
                ContentApps = contentApps.ToArray(),
                Dashboards = dashboards.ToArray(),
                Sections = sections.ToArray()
            };
        }

        // gets all manifest files (recursively)
        private IEnumerable<string> GetManifestFiles()
        {
            if (Directory.Exists(_path) == false)
                return new string[0];
            return Directory.GetFiles(_path, "package.manifest", SearchOption.AllDirectories);
        }

        private static string TrimPreamble(string text)
        {
            // strangely StartsWith(preamble) would always return true
            if (text.Substring(0, 1) == Utf8Preamble)
                text = text.Remove(0, Utf8Preamble.Length);

            return text;
        }

        /// <summary>
        /// Parses a manifest.
        /// </summary>
        public virtual PackageManifest ParseManifest(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullOrEmptyException(nameof(text));

            var manifest = JsonConvert.DeserializeObject<PackageManifest>(text,
                new DataEditorConverter(Logger),
                new ValueValidatorConverter(Validators),
                new DashboardAccessRuleConverter());

            // scripts and stylesheets are raw string, must process here
            for (var i = 0; i < manifest.Scripts.Length; i++)
                manifest.Scripts[i] = IOHelper.ResolveVirtualUrl(manifest.Scripts[i]);
            for (var i = 0; i < manifest.Stylesheets.Length; i++)
                manifest.Stylesheets[i] = IOHelper.ResolveVirtualUrl(manifest.Stylesheets[i]);

            // add property editors that are also parameter editors, to the parameter editors list
            // (the manifest format is kinda legacy)
            var ppEditors = manifest.PropertyEditors.Where(x => (x.Type & EditorType.MacroParameter) > 0).ToList();
            if (ppEditors.Count > 0)
                manifest.ParameterEditors = manifest.ParameterEditors.Union(ppEditors).ToArray();

            return manifest;
        }

        // purely for tests
        internal IEnumerable<GridEditor> ParseGridEditors(string text)
        {
            return JsonConvert.DeserializeObject<IEnumerable<GridEditor>>(text);
        }
    }
}
