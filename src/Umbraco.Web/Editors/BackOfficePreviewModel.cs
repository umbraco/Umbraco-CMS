using Umbraco.Core.Configuration;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        public BackOfficePreviewModel(UmbracoFeatures features, IGlobalSettings globalSettings) : base(features, globalSettings)
        {
        }

        public bool DisableDevicePreview => Features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => Features.Enabled.PreviewExtendedView;
    }
}
