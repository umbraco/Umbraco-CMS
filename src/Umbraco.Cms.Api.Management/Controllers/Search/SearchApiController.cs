using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Search;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Services;
using SearchDocument = Umbraco.Cms.Core.Search.Querying.Document;

namespace Umbraco.Cms.Api.Management.Controllers.Search;

/// <summary>
/// Executes an ad-hoc search against a registered index, for use by the backoffice Search section.
/// </summary>
[ApiVersion("1.0")]
public class SearchApiController : SearchControllerBase
{
    private readonly ISearcherResolver _searcherResolver;
    private readonly IEntityService _entityService;
    private readonly IVariationContextAccessor _variationContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchApiController"/> class.
    /// </summary>
    /// <param name="searcherResolver">The resolver used to obtain the searcher for the requested index alias.</param>
    /// <param name="entityService">The service used to hydrate matching results with entity name and icon.</param>
    /// <param name="variationContextAccessor">The accessor used to set the culture context for rendering entity names.</param>
    public SearchApiController(
        ISearcherResolver searcherResolver,
        IEntityService entityService,
        IVariationContextAccessor variationContextAccessor)
    {
        _searcherResolver = searcherResolver;
        _entityService = entityService;
        _variationContextAccessor = variationContextAccessor;
    }

    /// <summary>
    /// Executes a search against the index specified in the request.
    /// </summary>
    /// <param name="request">The search request, including index alias, query, filters, facets and sorters.</param>
    /// <param name="skip">The number of results to skip for pagination.</param>
    /// <param name="take">The maximum number of results to return.</param>
    /// <returns>The search results, or an error if the index alias is missing or could not be resolved.</returns>
    [HttpPost("search")]
    [ProducesResponseType<SearchResultResponseModel>(StatusCodes.Status200OK)]
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

        return Ok(new SearchResultResponseModel
        {
            Total = result.Total,
            Documents = CreateSearchDocumentResponseModels(result.Documents),
            Facets = result.Facets.Select(f => new FacetResultResponseModel
            {
                FieldName = f.FieldName,
                Values = f.Values,
            }),
        });
    }

    private IEnumerable<SearchDocumentResponseModel> CreateSearchDocumentResponseModels(IEnumerable<SearchDocument> documents)
    {
        foreach (IGrouping<UmbracoObjectTypes, SearchDocument> group in documents.GroupBy(d => d.ObjectType))
        {
            SearchDocument[] groupDocuments = group.ToArray();
            Guid[] keys = groupDocuments.Select(d => d.Id).Distinct().ToArray();

            // Default to an empty lookup; for unknown or unsupported object types
            // we will skip the entity lookup and return documents with null name/icon.
            Dictionary<Guid, IEntitySlim> entitiesByKey =
                group.Key is UmbracoObjectTypes.Document or UmbracoObjectTypes.Media or UmbracoObjectTypes.Member
                    ? _entityService.GetAll(group.Key, keys).ToDictionary(e => e.Key)
                    : new Dictionary<Guid, IEntitySlim>();

            foreach (SearchDocument document in groupDocuments)
            {
                IEntitySlim? entity = entitiesByKey.GetValueOrDefault(document.Id);
                yield return new SearchDocumentResponseModel
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
