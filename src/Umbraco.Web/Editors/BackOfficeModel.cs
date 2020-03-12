using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Web.Features;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{

    public class BackOfficeModel
    {
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion,
            IUmbracoSettingsSection umbracoSettingsSection, IIOHelper ioHelper, TreeCollection treeCollection,
            IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment,
            IRuntimeSettings runtimeSettings, ISecuritySettings securitySettings)
        {
            Features = features;
            GlobalSettings = globalSettings;
            UmbracoVersion = umbracoVersion;
            UmbracoSettingsSection = umbracoSettingsSection;
            IOHelper = ioHelper;
            TreeCollection = treeCollection;
            HttpContextAccessor = httpContextAccessor;
            HostingEnvironment = hostingEnvironment;
            RuntimeSettings = runtimeSettings;
            SecuritySettings = securitySettings;
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public IUmbracoVersion UmbracoVersion { get; }
        public IUmbracoSettingsSection UmbracoSettingsSection { get; }
        public IIOHelper IOHelper { get; }
        public TreeCollection TreeCollection { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public IRuntimeSettings RuntimeSettings { get; set; }
        public ISecuritySettings SecuritySettings { get; set; }
    }
}
