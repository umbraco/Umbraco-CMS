using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
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
    private readonly IMemberPresentationService _memberPresentationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchApiController"/> class.
    /// </summary>
    /// <param name="searcherResolver">The resolver used to obtain the searcher for the requested index alias.</param>
    /// <param name="entityService">The service used to hydrate matching results with entity name and icon.</param>
    /// <param name="variationContextAccessor">The accessor used to set the culture context for rendering entity names.</param>
    /// <param name="memberPresentationService">The service used to hydrate matching members, for content-based and external members alike.</param>
    public SearchApiController(
        ISearcherResolver searcherResolver,
        IEntityService entityService,
        IVariationContextAccessor variationContextAccessor,
        IMemberPresentationService memberPresentationService)
    {
        _searcherResolver = searcherResolver;
        _entityService = entityService;
        _variationContextAccessor = variationContextAccessor;
        _memberPresentationService = memberPresentationService;
    }

    /// <summary>
    /// Executes a search against the index specified in the request.
    /// </summary>
    /// <param name="request">The search request, including index alias, query, culture and segment.</param>
    /// <param name="skip">The number of results to skip for pagination.</param>
    /// <param name="take">The maximum number of results to return.</param>
    /// <returns>The search results, or an error if the index alias is missing or could not be resolved.</returns>
    [HttpPost("search")]
    [MapToApiVersion("1.0")]
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
            culture: request.Culture,
            segment: request.Segment,
            accessContext: AccessContext.BypassProtection(),
            skip: skip,
            take: take);

        // set the variation context so EntityService renders the correct culture variant
        _variationContextAccessor.VariationContext = new VariationContext(request.Culture);

        var documents = new List<SearchDocumentResponseModel>();
        foreach (IGrouping<UmbracoObjectTypes, SearchDocument> group in result.Documents.GroupBy(d => d.ObjectType))
        {
            documents.AddRange(group.Key is UmbracoObjectTypes.Member
                ? await CreateMemberSearchDocumentResponseModelsAsync(group.ToArray())
                : CreateEntitySearchDocumentResponseModels(group.Key, group.ToArray()));
        }

        return Ok(new SearchResultResponseModel
        {
            Total = result.Total,
            Documents = documents,
        });
    }

    private IEnumerable<SearchDocumentResponseModel> CreateEntitySearchDocumentResponseModels(UmbracoObjectTypes objectType, SearchDocument[] documents)
    {
        Guid[] keys = documents.Select(d => d.Id).Distinct().ToArray();

        // Default to an empty lookup; for unknown or unsupported object types
        // we will skip the entity lookup and return documents with null name/icon.
        Dictionary<Guid, IEntitySlim> entitiesByKey =
            objectType is UmbracoObjectTypes.Document or UmbracoObjectTypes.Media or UmbracoObjectTypes.Element
                ? _entityService.GetAll(objectType, keys).ToDictionary(e => e.Key)
                : new Dictionary<Guid, IEntitySlim>();

        foreach (SearchDocument document in documents)
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

    // members are resolved by the member presentation service, as member indexes can also hold external members,
    // which are not entities
    private async Task<IEnumerable<SearchDocumentResponseModel>> CreateMemberSearchDocumentResponseModelsAsync(SearchDocument[] documents)
    {
        var membersByKey =
            (await _memberPresentationService.CreateItemResponseModelsAsync(documents.Select(d => d.Id).ToHashSet()))
            .ToDictionary(m => m.Id);

        return documents.Select(document =>
        {
            MemberItemResponseModel? member = membersByKey.GetValueOrDefault(document.Id);
            return new SearchDocumentResponseModel
            {
                Id = document.Id,
                ObjectType = document.ObjectType,
                Name = member?.Variants.FirstOrDefault()?.Name,
                Icon = member is null ? null : GetIconForMember(member),
            };
        });
    }

    private static string GetIconForMember(MemberItemResponseModel member)
        => member.MemberType.Icon.IsNullOrWhiteSpace() ? Constants.Icons.Member : member.MemberType.Icon;

    private string? GetCultureNameForEntity(IEntitySlim? entity) =>
        entity switch
        {
            IPublishableContentEntitySlim publishableContentEntitySlim when publishableContentEntitySlim.CultureNames.TryGetValue(
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
