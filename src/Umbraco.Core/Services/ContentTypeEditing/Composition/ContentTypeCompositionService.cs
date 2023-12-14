using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing.Composition;

internal sealed class ContentTypeCompositionService : CompositionServiceBase<IContentType, IContentTypeService>, IContentTypeCompositionService
{
    public ContentTypeCompositionService(IContentTypeService concreteTypeService)
        : base(concreteTypeService)
    {
    }

    public Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositions(
        Guid id,
        IEnumerable<Guid> compositeIds,
        IEnumerable<string> currentPropertyAliases,
        bool isElement) => base.GetAvailableCompositions(id, compositeIds, currentPropertyAliases, isElement);
}
