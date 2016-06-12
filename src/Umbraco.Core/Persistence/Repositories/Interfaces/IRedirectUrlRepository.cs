using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRedirectUrlRepository : IRepositoryQueryable<int, IRedirectUrl>
    {
        void Delete(int id);
        void DeleteAll();
        void DeleteContentUrls(int contentId);
        IRedirectUrl GetMostRecentRule(string url);
        IEnumerable<RedirectUrl> GetContentUrls(int contentId);
        IEnumerable<RedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total);
        IEnumerable<RedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total);
    }
}
