using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.WebAssets
{
    public class BackOfficeWebAssets
    {
        public const string UmbracoPreviewJsBundleName = "umbraco-preview-js";
        public const string UmbracoPreviewCssBundleName = "umbraco-preview-css";
        public const string UmbracoCssBundleName = "umbraco-backoffice-css";
        public const string UmbracoInitCssBundleName = "umbraco-backoffice-init-css";
        public const string UmbracoCoreJsBundleName = "umbraco-backoffice-js";
        public const string UmbracoExtensionsJsBundleName = "umbraco-backoffice-extensions-js";
        public const string UmbracoTinyMceJsBundleName = "umbraco-tinymce-js";
        public const string UmbracoUpgradeCssBundleName = "umbraco-authorize-upgrade-css";

        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IManifestParser _parser;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly PropertyEditorCollection _propertyEditorCollection;

        public BackOfficeWebAssets(
            IRuntimeMinifier runtimeMinifier,
            IManifestParser parser,
            PropertyEditorCollection propertyEditorCollection,
            IHostingEnvironment hostingEnvironment,
            IOptions<GlobalSettings> globalSettings)
        {
            _runtimeMinifier = runtimeMinifier;
            _parser = parser;
            _propertyEditorCollection = propertyEditorCollection;
            _hostingEnvironment = hostingEnvironment;
            _globalSettings = globalSettings.Value;
        }

        public void CreateBundles()
        {
            // Create bundles

            // TODO: I think we don't want to optimize these css if/when we get gulp to do that all for us
            _runtimeMinifier.CreateCssBundle(UmbracoInitCssBundleName, true,
                FormatPaths("lib/bootstrap-social/bootstrap-social.css",
                "assets/css/umbraco.css",
                "lib/font-awesome/css/font-awesome.min.css"));

            _runtimeMinifier.CreateCssBundle(UmbracoUpgradeCssBundleName, true,
                FormatPaths("assets/css/umbraco.css",
                "lib/bootstrap-social/bootstrap-social.css",
                "lib/font-awesome/css/font-awesome.min.css"));

            _runtimeMinifier.CreateCssBundle(UmbracoPreviewCssBundleName, true,
                FormatPaths("assets/css/canvasdesigner.css"));

            _runtimeMinifier.CreateJsBundle(UmbracoPreviewJsBundleName, false,
                FormatPaths(GetScriptsForPreview()));

            _runtimeMinifier.CreateJsBundle(UmbracoTinyMceJsBundleName, false,
                FormatPaths(GetScriptsForTinyMce()));

            _runtimeMinifier.CreateJsBundle(UmbracoCoreJsBundleName, false,
                FormatPaths(GetScriptsForBackOfficeCore()));

            var propertyEditorAssets = ScanPropertyEditors()
                .GroupBy(x => x.AssetType)
                .ToDictionary(x => x.Key, x => x.Select(c => c.FilePath));

            _runtimeMinifier.CreateJsBundle(
                UmbracoExtensionsJsBundleName, true,
                FormatPaths(
                    GetScriptsForBackOfficeExtensions(
                        propertyEditorAssets.TryGetValue(AssetType.Javascript, out var scripts) ? scripts : Enumerable.Empty<string>())));

            _runtimeMinifier.CreateCssBundle(
                UmbracoCssBundleName, true,
                FormatPaths(
                    GetStylesheetsForBackOffice(
                        propertyEditorAssets.TryGetValue(AssetType.Css, out var styles) ? styles : Enumerable.Empty<string>())));
        }

        /// <summary>
        /// Returns scripts used to load the back office
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptsForBackOfficeExtensions(IEnumerable<string> propertyEditorScripts)
        {
            var scripts = new HashSet<string>();
            foreach (string script in _parser.Manifest.Scripts)
            {
                scripts.Add(script);
            }

            foreach (string script in propertyEditorScripts)
            {
                scripts.Add(script);
            }

            return scripts.ToArray();
        }

        /// <summary>
        /// Returns the list of scripts for back office initialization
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptsForBackOfficeCore()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.JsInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Returns stylesheets used to load the back office
        /// </summary>
        /// <returns></returns>
        private string[] GetStylesheetsForBackOffice(IEnumerable<string> propertyEditorStyles)
        {
            var stylesheets = new HashSet<string>();

            foreach (string script in _parser.Manifest.Stylesheets)
            {
                stylesheets.Add(script);
            }

            foreach (string stylesheet in propertyEditorStyles)
            {
                stylesheets.Add(stylesheet);
            }

            return stylesheets.ToArray();
        }

        /// <summary>
        /// Returns the scripts used for tinymce
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptsForTinyMce()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.TinyMceInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Returns the scripts used for preview
        /// </summary>
        /// <returns></returns>
        private string[] GetScriptsForPreview()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.PreviewInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Re-format asset paths to be absolute paths
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        private string[] FormatPaths(params string[] assets)
        {
            var umbracoPath = _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);

            return assets
                .Where(x => x.IsNullOrWhiteSpace() == false)
                .Select(x => !x.StartsWith("/") && Uri.IsWellFormedUriString(x, UriKind.Relative)
                    // most declarations with be made relative to the /umbraco folder, so things
                    // like lib/blah/blah.js so we need to turn them into absolutes here
                    ? umbracoPath.EnsureStartsWith('/').TrimEnd("/") + x.EnsureStartsWith('/')
                    : x).ToArray();
        }

        /// <summary>
        /// Returns the web asset paths to load for property editors that have the <see cref="PropertyEditorAssetAttribute"/> attribute applied
        /// </summary>
        /// <returns></returns>
        private IEnumerable<PropertyEditorAssetAttribute> ScanPropertyEditors()
        {
            return _propertyEditorCollection
                .SelectMany(x => x.GetType().GetCustomAttributes<PropertyEditorAssetAttribute>(false));
        }
    }
}
