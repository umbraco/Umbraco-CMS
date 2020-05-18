using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Web.Features;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{
    // TODO: Almost nothing here needs to exist since we can inject these into the view
    public class BackOfficeModel
    {
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion,
            IContentSettings contentSettings, TreeCollection treeCollection,
            IHostingEnvironment hostingEnvironment,
            IRuntimeSettings runtimeSettings, ISecuritySettings securitySettings)
        {
            Features = features;
            GlobalSettings = globalSettings;
            UmbracoVersion = umbracoVersion;
            ContentSettings = contentSettings;
            TreeCollection = treeCollection;
            HostingEnvironment = hostingEnvironment;
            RuntimeSettings = runtimeSettings;
            SecuritySettings = securitySettings;
            BackOfficePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public IUmbracoVersion UmbracoVersion { get; }
        public IContentSettings ContentSettings { get; }
        public TreeCollection TreeCollection { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public IRuntimeSettings RuntimeSettings { get; set; }
        public ISecuritySettings SecuritySettings { get; set; }

        public string BackOfficePath { get; } 
    }
}
