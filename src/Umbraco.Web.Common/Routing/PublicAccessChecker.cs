using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Routing
{
    public class PublicAccessChecker : IPublicAccessChecker
    {
        //TODO implement
        public PublicAccessStatus HasMemberAccessToContent(int publishedContentId) => PublicAccessStatus.AccessAccepted;
    }
}
