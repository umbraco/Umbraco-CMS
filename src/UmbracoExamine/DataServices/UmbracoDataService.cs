using System.Web;
using System.Web.Hosting;

namespace UmbracoExamine.DataServices
{
    public class UmbracoDataService : IDataService
    {
        public UmbracoDataService()
        {
            ContentService = new UmbracoContentService();
            MediaService = new UmbracoMediaService();
            LogService = new UmbracoLogService();
        }        

        public IContentService ContentService { get; protected set; }
        public IMediaService MediaService { get; protected set; }
        public ILogService LogService { get; protected set; }

        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

    }
}
