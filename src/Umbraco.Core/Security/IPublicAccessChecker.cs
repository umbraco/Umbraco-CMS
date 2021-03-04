namespace Umbraco.Cms.Core.Security
{
    public interface IPublicAccessChecker
    {
        PublicAccessStatus HasMemberAccessToContent(int publishedContentId);
    }
}
