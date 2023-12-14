using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing.Composition;

internal abstract class CompositionServiceBase<TType, TTypeService>
    where TType : class, IContentTypeComposition
    where TTypeService : IContentTypeBaseService<TType>
{
    private readonly TTypeService _concreteTypeService;

    protected CompositionServiceBase(TTypeService concreteTypeService)
    {
        _concreteTypeService = concreteTypeService;
    }

    protected async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositions(Guid id, IEnumerable<Guid> compositeIds, IEnumerable<string> currentPropertyAliases, bool isElement)
    {
        var contentType = await _concreteTypeService.GetAsync(id); // NB: different for media/member (media/member service)

        IContentTypeComposition[] allContentTypes = _concreteTypeService.GetAll().Cast<IContentTypeComposition>().ToArray(); // NB: different for media/member (media/member service)
        var currentCompositionAliases = compositeIds.Any()
            ? _concreteTypeService.GetAll(compositeIds).Select(x => x.Alias).ToArray()
            : Array.Empty<string>();

        ContentTypeAvailableCompositionsResults availableCompositions = _concreteTypeService.GetAvailableCompositeContentTypes( // TODO: should always be contentTypeService
            contentType,
            allContentTypes,
            currentCompositionAliases,
            currentPropertyAliases.ToArray(),
            isElement);

        return availableCompositions.Results;
    }
}
