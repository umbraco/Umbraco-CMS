namespace Umbraco.Web.Security
{
    public interface IPublicAccessChecker
    {
        PublicAccessStatus HasMemberAccessToContent(int publishedContentId);
    }
}
