using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        private readonly UmbracoFeatures _features;
        public IEnumerable<ILanguage> Languages { get; }

        public BackOfficePreviewModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion, IEnumerable<ILanguage> languages, IUmbracoSettingsSection umbracoSettingsSection) : base(features, globalSettings, umbracoVersion, umbracoSettingsSection)
        {
            _features = features;
            Languages = languages;
        }

        public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
    }
}
