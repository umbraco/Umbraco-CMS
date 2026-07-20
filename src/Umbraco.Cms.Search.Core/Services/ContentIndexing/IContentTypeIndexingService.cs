using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IContentTypeIndexingService
{
    void ReindexByContentTypes(Guid[] contentTypeKeys, UmbracoObjectTypes objectType, string origin);
}
