using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IRedirectUrlService : IService
    {
        void Save(IRedirectUrl redirectUrl);

        void DeleteContentUrls(int contentId);

        void Delete(IRedirectUrl redirectUrl);

        void Delete(int id);

        void DeleteAll();

        IRedirectUrl GetMostRecentRule(string url);

        IEnumerable<IRedirectUrl> GetContentUrls(int contentId);

        IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total);

        IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total);
    }
}
