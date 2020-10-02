using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Web.Features;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{

    public class BackOfficeModel
    {
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion,
            IContentSettings contentSettings, TreeCollection treeCollection,
            IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment,
            IRuntimeSettings runtimeSettings, ISecuritySettings securitySettings, IIconService iconService)
        {
            Features = features;
            GlobalSettings = globalSettings;
            UmbracoVersion = umbracoVersion;
            ContentSettings = contentSettings;
            TreeCollection = treeCollection;
            HttpContextAccessor = httpContextAccessor;
            HostingEnvironment = hostingEnvironment;
            RuntimeSettings = runtimeSettings;
            SecuritySettings = securitySettings;
            BackOfficePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);
            IconCheckData = iconService.GetIcon("icon-check")?.SvgString;
            IconDeleteData = iconService.GetIcon("icon-delete")?.SvgString;
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public IUmbracoVersion UmbracoVersion { get; }
        public IContentSettings ContentSettings { get; }
        public TreeCollection TreeCollection { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public IRuntimeSettings RuntimeSettings { get; set; }
        public ISecuritySettings SecuritySettings { get; set; }

        public string BackOfficePath { get; }
        public string IconCheckData { get; }
        public string IconDeleteData { get; }
    }
}
