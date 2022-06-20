using System.Globalization;
using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search;

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
    public IEnumerable<SearchResultEntity> ExamineSearch(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? culture = null,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
    {
        IEnumerable<ISearchResult> pagedResult = _backOfficeExamineSearcher.Search(query, entityType, pageSize, pageIndex, out totalFound, searchFrom, ignoreUserStartNodes);

        switch (entityType)
        {
            case UmbracoEntityTypes.Member:
                return MemberFromSearchResults(pagedResult.ToArray());
            case UmbracoEntityTypes.Media:
                return MediaFromSearchResults(pagedResult);
            case UmbracoEntityTypes.Document:
                return ContentFromSearchResults(pagedResult, culture);
            default:
                throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) +
                                                " currently does not support searching against object type " +
                                                entityType);
        }
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
    public IEnumerable<SearchResultEntity?> EntitySearch(UmbracoObjectTypes objectType, string query, int pageSize, long pageIndex, out long totalFound, string? searchFrom = null)
    {
        // if it's a GUID, match it
        Guid.TryParse(query, out Guid g);

        IEnumerable<IEntitySlim> results = _entityService.GetPagedDescendants(
            objectType,
            pageIndex,
            pageSize,
            out totalFound,
            _sqlContext.Query<IUmbracoEntity>()
                .Where(x => x.Name!.Contains(query) || x.Key == g));
        return _mapper.MapEnumerable<IEntitySlim, SearchResultEntity>(results);
    }

    /// <summary>
    ///     Returns a collection of entities for media based on search results
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private IEnumerable<SearchResultEntity> MemberFromSearchResults(IEnumerable<ISearchResult> results)
    {
        // add additional data
        foreach (ISearchResult result in results)
        {
            SearchResultEntity? m = _mapper.Map<SearchResultEntity>(result);

            if (m is null)
            {
                continue;
            }

            // if no icon could be mapped, it will be set to document, so change it to picture
            if (m.Icon == Constants.Icons.DefaultIcon)
            {
                m.Icon = Constants.Icons.Member;
            }

            if (result.Values.ContainsKey("email") && result.Values["email"] != null)
            {
                m.AdditionalData["Email"] = result.Values["email"];
            }

            if (result.Values.ContainsKey(UmbracoExamineFieldNames.NodeKeyFieldName) &&
                result.Values[UmbracoExamineFieldNames.NodeKeyFieldName] != null)
            {
                if (Guid.TryParse(result.Values[UmbracoExamineFieldNames.NodeKeyFieldName], out Guid key))
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
    private IEnumerable<SearchResultEntity> MediaFromSearchResults(IEnumerable<ISearchResult> results)
        => _mapper.Map<IEnumerable<SearchResultEntity>>(results) ?? Enumerable.Empty<SearchResultEntity>();

    /// <summary>
    ///     Returns a collection of entities for content based on search results
    /// </summary>
    /// <param name="results"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    private IEnumerable<SearchResultEntity> ContentFromSearchResults(
        IEnumerable<ISearchResult> results,
        string? culture = null)
    {
        var defaultLang = _languageService.GetDefaultLanguageIsoCode();

        foreach (ISearchResult result in results)
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
                if (result.Values.TryGetValue(UmbracoExamineFieldNames.VariesByCultureFieldName, out var varies) &&
                    varies == "y")
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
