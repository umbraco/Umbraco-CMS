using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        private readonly UmbracoFeatures _features;
        public IEnumerable<ILanguage> Languages { get; }

        public BackOfficePreviewModel(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IEnumerable<ILanguage> languages)
            : base(features, globalSettings)
        {
            _features = features;
            Languages = languages;
        }

        public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
    }
}
