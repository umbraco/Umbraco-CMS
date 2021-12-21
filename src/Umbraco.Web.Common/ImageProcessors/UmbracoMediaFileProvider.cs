using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Web.Common.ImageProcessors
{
    internal class UmbracoMediaFileProvider : PhysicalFileProvider, IUmbracoMediaFileProvider
    {
        public UmbracoMediaFileProvider(IHostingEnvironment hostingEnvironment, IOptions<GlobalSettings> globalSettings)
            : base(hostingEnvironment.MapPathWebRoot(globalSettings.Value.UmbracoMediaPath))
        {
        }
    }
}
