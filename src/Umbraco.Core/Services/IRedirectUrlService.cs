using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IRedirectUrlService : IService
    {
        void Save(RedirectUrl redirectUrl);

        void Delete(int id);

        void DeleteContentRules(int contentId);

        void DeleteAll();

        RedirectUrl GetMostRecentRule(string url);

        IEnumerable<RedirectUrl> GetRules(int contentId);

        IEnumerable<RedirectUrl> GetAllRules(long pageIndex, int pageSize, out long total);

        IEnumerable<RedirectUrl> GetAllRules(int rootContentId, long pageIndex, int pageSize, out long total);
    }
}
