using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        private readonly UmbracoFeatures _features;
        public IEnumerable<ILanguage> Languages { get; }

        public BackOfficePreviewModel(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IUmbracoVersion umbracoVersion,
            IEnumerable<ILanguage> languages,
            IContentSettings contentSettings,
            TreeCollection treeCollection,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment,
            IRuntimeSettings runtimeSettings,
            ISecuritySettings securitySettings,
            IIconService iconService)
            : base(
                features,
                globalSettings,
                umbracoVersion,
                contentSettings,
                treeCollection,
                httpContextAccessor,
                hostingEnvironment,
                runtimeSettings,
                securitySettings,
                iconService)
        {
            _features = features;
            Languages = languages;
        }

        public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
    }
}
