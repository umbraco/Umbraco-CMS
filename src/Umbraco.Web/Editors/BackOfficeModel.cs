using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{

    public class BackOfficeModel
    {
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion, IUmbracoSettingsSection umbracoSettingsSection)
        {
            Features = features;
            GlobalSettings = globalSettings;
            UmbracoVersion = umbracoVersion;
            UmbracoSettingsSection = umbracoSettingsSection;
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public IUmbracoVersion UmbracoVersion { get; }
        public IUmbracoSettingsSection UmbracoSettingsSection { get; }
    }
}
