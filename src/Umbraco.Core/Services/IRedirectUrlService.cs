using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IRedirectUrlService : IService
    {
        void Save(IRedirectUrl redirectUrl);

        void DeleteContentRedirectUrls(int contentId);

        void Delete(IRedirectUrl redirectUrl);

        void Delete(int id);

        void DeleteAll();

        IRedirectUrl GetMostRecentRedirectUrl(string url);

        IEnumerable<IRedirectUrl> GetContentRedirectUrls(int contentId);

        IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total);

        IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total);
    }
}
