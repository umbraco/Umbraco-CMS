using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IContentProtectionProvider
{
    Task<ContentProtection?> GetContentProtectionAsync(IContentBase content);
}
