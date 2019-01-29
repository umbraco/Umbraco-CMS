using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        private readonly UmbracoFeatures _features;
        public IEnumerable<BackOfficePreviewLinkModel> PreviewLinks { get; }

        public BackOfficePreviewModel(UmbracoFeatures features, IGlobalSettings globalSettings, IEnumerable<BackOfficePreviewLinkModel> previewLinks) : base(features, globalSettings)
        {
            _features = features;
            PreviewLinks = previewLinks;
        }

        public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
    }
}
