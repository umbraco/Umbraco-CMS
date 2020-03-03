using System.Collections.Generic;
using Umbraco.Abstractions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.Features;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        private readonly UmbracoFeatures _features;
        public IEnumerable<ILanguage> Languages { get; }

        public BackOfficePreviewModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion, IEnumerable<ILanguage> languages, IUmbracoSettingsSection umbracoSettingsSection, IIOHelper ioHelper, TreeCollection treeCollection, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IRuntimeSettings runtimeSettings)
            : base(features, globalSettings, umbracoVersion, umbracoSettingsSection, ioHelper, treeCollection, httpContextAccessor, hostingEnvironment, runtimeSettings)
        {
            _features = features;
            Languages = languages;
        }

        public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
    }
}
