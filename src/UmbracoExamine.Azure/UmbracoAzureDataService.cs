using UmbracoExamine.DataServices;

namespace UmbracoExamine.Azure
{
    public class UmbracoAzureDataService : UmbracoDataService
    {
        public UmbracoAzureDataService()
        {
            //overwrite the log service
            LogService = new UmbracoAzureLogService();
        }
    }
}