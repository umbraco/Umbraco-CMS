namespace UmbracoExamine.DataServices
{
    public class UmbracoMemberDataService : UmbracoDataService
    {
        public UmbracoMemberDataService()
        {
            ContentService = new UmbracoMemberContentService();
        }
    }
}