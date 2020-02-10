using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Web.Features;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{

    public class BackOfficeModel
    {
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion, IUmbracoSettingsSection umbracoSettingsSection, IIOHelper ioHelper, TreeCollection treeCollection)
        {
            Features = features;
            GlobalSettings = globalSettings;
            UmbracoVersion = umbracoVersion;
            UmbracoSettingsSection = umbracoSettingsSection;
            IOHelper = ioHelper;
            TreeCollection = treeCollection;
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public IUmbracoVersion UmbracoVersion { get; }
        public IUmbracoSettingsSection UmbracoSettingsSection { get; }
        public IIOHelper IOHelper { get; }
        public TreeCollection TreeCollection { get; }
    }
}
