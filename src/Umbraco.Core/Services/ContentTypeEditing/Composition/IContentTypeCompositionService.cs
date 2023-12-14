using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing.Composition;

public interface IContentTypeCompositionService
{
    Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositions(
        Guid id,
        IEnumerable<Guid> compositeIds,
        IEnumerable<string> currentPropertyAliases,
        bool isElement);
}
