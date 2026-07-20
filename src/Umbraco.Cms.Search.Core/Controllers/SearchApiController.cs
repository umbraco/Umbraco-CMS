using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.ViewModels;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Controllers;

[ApiVersion("1.0")]
public class SearchApiController : ApiControllerBase
{
    private readonly ISearcherResolver _searcherResolver;
    private readonly IEntityService _entityService;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public SearchApiController(
        ISearcherResolver searcherResolver,
        IEntityService entityService,
        IVariationContextAccessor variationContextAccessor)
    {
        _searcherResolver = searcherResolver;
        _entityService = entityService;
        _variationContextAccessor = variationContextAccessor;
    }

    [HttpPost("search")]
    [ProducesResponseType<SearchResultViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Search([FromBody] SearchRequestModel request, int skip = 0, int take = 100)
    {
        if (string.IsNullOrWhiteSpace(request.IndexAlias))
        {
            return BadRequest("The indexAlias parameter must be provided and cannot be empty.");
        }

        ISearcher? searcher = _searcherResolver.GetSearcher(request.IndexAlias);
        if (searcher is null)
        {
            return NotFound($"No searcher was found for the index alias '{request.IndexAlias}'.");
        }

        SearchResult result = await searcher.SearchAsync(
            request.IndexAlias,
            request.Query,
            request.Filters,
            request.Facets,
            request.Sorters,
            request.Culture,
            request.Segment,
            AccessContext.BypassProtection(),
            skip,
            take);

        // set the variation context so EntityService renders the correct culture variant
        _variationContextAccessor.VariationContext = new VariationContext(request.Culture);

        return Ok(new SearchResultViewModel
        {
            Total = result.Total,
            Documents = CreateDocumentViewModels(result.Documents),
            Facets = result.Facets.Select(f => new FacetResultViewModel
            {
                FieldName = f.FieldName,
                Values = f.Values,
            }),
        });
    }

    private IEnumerable<DocumentViewModel> CreateDocumentViewModels(IEnumerable<Document> documents)
    {
        foreach (IGrouping<UmbracoObjectTypes, Document> group in documents.GroupBy(d => d.ObjectType))
        {
            Document[] groupDocuments = group.ToArray();
            Guid[] keys = groupDocuments.Select(d => d.Id).Distinct().ToArray();

            // Default to an empty lookup; for unknown or unsupported object types
            // we will skip the entity lookup and return documents with null name/icon.
            Dictionary<Guid, IEntitySlim> entitiesByKey =
                group.Key is UmbracoObjectTypes.Document or UmbracoObjectTypes.Media or UmbracoObjectTypes.Member
                    ? _entityService.GetAll(group.Key, keys).ToDictionary(e => e.Key)
                    : new Dictionary<Guid, IEntitySlim>();

            foreach (Document document in groupDocuments)
            {
                IEntitySlim? entity = entitiesByKey.GetValueOrDefault(document.Id);
                yield return new DocumentViewModel
                {
                    Id = document.Id,
                    ObjectType = document.ObjectType,
                    Name = GetCultureNameForEntity(entity),
                    Icon = GetIconForEntity(entity),
                };
            }
        }
    }

    private string? GetCultureNameForEntity(IEntitySlim? entity) =>
        entity switch
        {
            IDocumentEntitySlim documentEntitySlim when documentEntitySlim.CultureNames.TryGetValue(
                _variationContextAccessor.VariationContext!.Culture,
                out var name) => name,
            _ => entity?.Name,
        };

    private static string? GetIconForEntity(IEntitySlim? entity)
    {
        if (entity is IContentEntitySlim slim)
        {
            return slim.ContentTypeIcon;
        }

        return null;
    }
}
