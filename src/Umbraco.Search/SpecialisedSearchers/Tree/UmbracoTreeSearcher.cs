using System.Globalization;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using Umbraco.Search.Models;

namespace Umbraco.Search.SpecialisedSearchers.Tree;

/// <summary>
///     Used for internal Umbraco implementations of <see cref="ISearchableTree" />
/// </summary>
public class UmbracoTreeSearcher
{
    private readonly IBackOfficeExamineSearcher _backOfficeExamineSearcher;
    private readonly IEntityService _entityService;
    private readonly ILocalizationService _languageService;
    private readonly IUmbracoMapper _mapper;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly ISqlContext _sqlContext;

    public UmbracoTreeSearcher(
        ILocalizationService languageService,
        IEntityService entityService,
        IUmbracoMapper mapper,
        ISqlContext sqlContext,
        IBackOfficeExamineSearcher backOfficeExamineSearcher,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _languageService = languageService;
        _entityService = entityService;
        _mapper = mapper;
        _sqlContext = sqlContext;
        _backOfficeExamineSearcher = backOfficeExamineSearcher;
        _publishedUrlProvider = publishedUrlProvider;
    }

    /// <summary>
    ///     Searches Examine for results based on the entity type
    /// </summary>
    /// <param name="query"></param>
    /// <param name="entityType"></param>
    /// <param name="culture"></param>
    /// <param name="totalFound"></param>
    /// <param name="searchFrom">
    ///     A starting point for the search, generally a node id, but for members this is a member type alias
    /// </param>
    /// <param name="pageSize"></param>
    /// <param name="pageIndex"></param>
    /// <param name="ignoreUserStartNodes">If set to true, user and group start node permissions will be ignored.</param>
    /// <returns></returns>
    public IEnumerable<SearchResultEntity> IndexSearch(
        IBackofficeSearchRequest request,
        out long totalFound)
    {
        IEnumerable<IUmbracoSearchResult> pagedResult = _backOfficeExamineSearcher.Search(request, out totalFound);

        return request.EntityType switch
        {
            UmbracoEntityTypes.Member => MemberFromSearchResults(pagedResult.ToArray()),
            UmbracoEntityTypes.Media => MediaFromSearchResults(pagedResult),
            UmbracoEntityTypes.Document => ContentFromSearchResults(pagedResult, request.Culture),
            _ => throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) +
                                                 " currently does not support searching against object type " +
                                                 request.EntityType)
        };
    }

    /// <summary>
    ///     Searches with the <see cref="IEntityService" /> for results based on the entity type
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="query"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageIndex"></param>
    /// <param name="totalFound"></param>
    /// <param name="searchFrom"></param>
    /// <returns></returns>
    public IEnumerable<SearchResultEntity?> EntitySearch(IBackofficeSearchRequest request,
        out long totalFound)
    {
        // if it's a GUID, match it
        Guid.TryParse(request.Query, out Guid g);

        IEnumerable<IEntitySlim> results = _entityService.GetPagedDescendants(
            request.ObjectType,
            request.Page,
            request.PageSize,
            out totalFound,
            _sqlContext.Query<IUmbracoEntity>()
                .Where(x => x.Name!.Contains(request.Query) || x.Key == g));
        return _mapper.MapEnumerable<IEntitySlim, SearchResultEntity>(results);
    }

    /// <summary>
    ///     Returns a collection of entities for media based on search results
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private IEnumerable<SearchResultEntity> MemberFromSearchResults(IEnumerable<IUmbracoSearchResult> results)
    {
        // add additional data
        foreach (IUmbracoSearchResult result in results)
        {
            SearchResultEntity? m = _mapper.Map<SearchResultEntity>(result);

            if (m is null)
            {
                continue;
            }

            // if no icon could be mapped, it will be set to document, so change it to picture
            if (m.Icon == Cms.Core.Constants.Icons.DefaultIcon)
            {
                m.Icon = Cms.Core.Constants.Icons.Member;
            }

            if (result.Values.ContainsKey("email") && result.Values["email"] != null)
            {
                m.AdditionalData["Email"] = result.Values["email"];
            }

            if (result.Values.ContainsKey(UmbracoSearchFieldNames.NodeKeyFieldName) &&
                result.Values[UmbracoSearchFieldNames.NodeKeyFieldName] != null)
            {
                var keyValue = result.Values[UmbracoSearchFieldNames.NodeKeyFieldName].FirstOrDefault()?.ToString();
                if (Guid.TryParse(keyValue, out Guid key))
                {
                    m.Key = key;
                }
            }

            yield return m;
        }
    }

    /// <summary>
    ///     Returns a collection of entities for media based on search results
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private IEnumerable<SearchResultEntity> MediaFromSearchResults(IEnumerable<IUmbracoSearchResult> results)
        => _mapper.Map<IEnumerable<SearchResultEntity>>(results) ?? Enumerable.Empty<SearchResultEntity>();

    /// <summary>
    ///     Returns a collection of entities for content based on search results
    /// </summary>
    /// <param name="results"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    private IEnumerable<SearchResultEntity> ContentFromSearchResults(
        IEnumerable<IUmbracoSearchResult> results,
        string? culture = null)
    {
        var defaultLang = _languageService.GetDefaultLanguageIsoCode();

        foreach (IUmbracoSearchResult result in results)
        {
            SearchResultEntity? entity = _mapper.Map<SearchResultEntity>(result, context =>
            {
                if (culture != null)
                {
                    context.SetCulture(culture);
                }
            });

            if (entity is null)
            {
                continue;
            }

            if (int.TryParse(entity.Id?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
            {
                // if it varies by culture, return the default language URL
                if (result.Values.TryGetValue(UmbracoSearchFieldNames.VariesByCultureFieldName, out var varies) &&
                    varies.FirstOrDefault()?.ToString() == "y")
                {
                    entity.AdditionalData["Url"] = _publishedUrlProvider.GetUrl(intId, culture: culture ?? defaultLang);
                }
                else
                {
                    entity.AdditionalData["Url"] = _publishedUrlProvider.GetUrl(intId);
                }
            }

            yield return entity;
        }
    }
}
