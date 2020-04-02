using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.WebAssets;
using Umbraco.Infrastructure.WebAssets;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.JavaScript
{
    public class BackOfficeWebAssets
    {
        public const string UmbracoPreviewJsBundleName = "umbraco-preview-js";
        public const string UmbracoPreviewCssBundleName = "umbraco-preview-css";
        public const string UmbracoCssBundleName = "umbraco-backoffice-css";
        public const string UmbracoInitCssBundleName = "umbraco-backoffice-init-css";
        public const string UmbracoJsBundleName = "umbraco-backoffice-js";
        public const string UmbracoTinyMceJsBundleName = "umbraco-tinymce-js";
        public const string UmbracoUpgradeCssBundleName = "umbraco-authorize-upgrade-css";

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IManifestParser _parser;
        private readonly PropertyEditorCollection _propertyEditorCollection;

        public BackOfficeWebAssets(
            IHostingEnvironment hostingEnvironment,
            IRuntimeMinifier runtimeMinifier,
            IManifestParser parser,
            PropertyEditorCollection propertyEditorCollection)
        {
            _hostingEnvironment = hostingEnvironment;
            _runtimeMinifier = runtimeMinifier;
            _parser = parser;
            _propertyEditorCollection = propertyEditorCollection;
        }

        public void CreateBundles()
        {
            // Create bundles

            _runtimeMinifier.CreateCssBundle(UmbracoInitCssBundleName,
                "lib/bootstrap-social/bootstrap-social.css",
                "assets/css/umbraco.css",
                "lib/font-awesome/css/font-awesome.min.css");

            _runtimeMinifier.CreateCssBundle(UmbracoUpgradeCssBundleName,
                "assets/css/umbraco.css",
                "lib/bootstrap-social/bootstrap-social.css",
                "lib/font-awesome/css/font-awesome.min.css");

            _runtimeMinifier.CreateCssBundle(UmbracoPreviewCssBundleName,
                "assets/css/canvasdesigner.css");

            _runtimeMinifier.CreateJsBundle(UmbracoPreviewJsBundleName,
                GetScriptsForPreview().ToArray());

            _runtimeMinifier.CreateJsBundle(UmbracoTinyMceJsBundleName,
                GetScriptsForTinyMce().ToArray());

            var propertyEditorAssets = ScanPropertyEditors().GroupBy(x => x.AssetType);
            foreach (var assetGroup in propertyEditorAssets)
            {
                switch (assetGroup.Key)
                {
                    case AssetType.Javascript:
                        _runtimeMinifier.CreateJsBundle(
                            UmbracoJsBundleName,
                            GetScriptsForBackoffice(assetGroup.Select(x => x.FilePath)).ToArray());
                        break;
                    case AssetType.Css:
                        _runtimeMinifier.CreateCssBundle(
                            UmbracoCssBundleName,
                            GetStylesheetsForBackoffice(assetGroup.Select(x => x.FilePath)).ToArray());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        /// <summary>
        /// Returns scripts used to load the back office
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetScriptsForBackoffice(IEnumerable<string> propertyEditorScripts)
        {
            var umbracoInit = JsInitialization.GetDefaultInitialization();
            var scripts = new HashSet<string>();
            foreach (var script in umbracoInit)
                scripts.Add(script);
            foreach (var script in _parser.Manifest.Scripts)
                scripts.Add(script);
            foreach (var script in propertyEditorScripts)
                scripts.Add(script);

            return new HashSet<string>(FormatPaths(scripts));
        }

        /// <summary>
        /// Returns stylesheets used to load the back office
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetStylesheetsForBackoffice(IEnumerable<string> propertyEditorStyles)
        {
            var stylesheets = new HashSet<string>();

            foreach (var script in _parser.Manifest.Stylesheets)
                stylesheets.Add(script);
            foreach (var stylesheet in propertyEditorStyles)
                stylesheets.Add(stylesheet);

            return new HashSet<string>(FormatPaths(stylesheets));
        }

        /// <summary>
        /// Returns the scripts used for tinymce
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetScriptsForTinyMce()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.TinyMceInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }

        /// <summary>
        /// Returns the scripts used for preview
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetScriptsForPreview()
        {
            var resources = JsonConvert.DeserializeObject<JArray>(Resources.PreviewInitialize);
            return resources.Where(x => x.Type == JTokenType.String).Select(x => x.ToString());
        }

        /// <summary>
        /// Re-format asset paths to be absolute paths
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        private IEnumerable<string> FormatPaths(IEnumerable<string> assets)
        {
            return assets
                .Where(x => x.IsNullOrWhiteSpace() == false)
                .Select(x => !x.StartsWith("/") && Uri.IsWellFormedUriString(x, UriKind.Relative)
                    // most declarations with be made relative to the /umbraco folder, so things
                    // like lib/blah/blah.js so we need to turn them into absolutes here
                    ? _hostingEnvironment.ApplicationVirtualPath.EnsureStartsWith('/').TrimEnd("/") + x.EnsureStartsWith('/')
                    : x).ToList();
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
