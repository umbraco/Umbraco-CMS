using Umbraco.Core.Configuration;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{

    public class BackOfficeModel
    {
        private IconController IconController { get; }
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings)
        {
            Features = features;
            GlobalSettings = globalSettings;
            IconController = new IconController();
            IconCheckData = IconController.GetIcon("icon-check")?.SvgString;
            IconDeleteData = IconController.GetIcon("icon-delete")?.SvgString;
        }
        
        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public string IconCheckData { get; }
        public string IconDeleteData { get; }
    }
}
