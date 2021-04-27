using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Security
{
    public interface IPublicAccessChecker
    {
        Task<PublicAccessStatus> HasMemberAccessToContentAsync(int publishedContentId);
    }
}
