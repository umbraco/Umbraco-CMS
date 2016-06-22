using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRedirectUrlRepository : IRepositoryQueryable<int, IRedirectUrl>
    {
        IRedirectUrl Get(string url, int contentId);
        void Delete(int id);
        void DeleteAll();
        void DeleteContentUrls(int contentId);
        IRedirectUrl GetMostRecentUrl(string url);
        IEnumerable<IRedirectUrl> GetContentUrls(int contentId);
        IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total);
        IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total);
    }
}
