using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Search.Configuration;
using Umbraco.Search.SpecialisedSearchers;

namespace Umbraco.Search.InMemory.SpecialisedSearchers;

public class BackOfficeInMemorySearcher : IBackOfficeExamineSearcher
{
    private readonly IInMemoryIndexManager _inMemoryIndexManager;
    private readonly ILocalizationService _languageService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly IUmbracoTreeSearcherFields _treeSearcherFields;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public BackOfficeInMemorySearcher(
        IInMemoryIndexManager inMemoryIndexManager,
        ILocalizationService languageService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IUmbracoTreeSearcherFields treeSearcherFields,
        AppCaches appCaches,
        IUmbracoMapper umbracoMapper,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _inMemoryIndexManager = inMemoryIndexManager;
        _languageService = languageService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityService = entityService;
        _treeSearcherFields = treeSearcherFields;
        _umbracoMapper = umbracoMapper;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IEnumerable<IUmbracoSearchResult> Search(string query, UmbracoEntityTypes entityType, int pageSize,
        long pageIndex, out long totalFound,
        string? searchFrom = null, bool ignoreUserStartNodes = false)
    {
        //special GUID check since if a user searches on one specifically we need to escape it
        if (Guid.TryParse(query, out Guid g))
        {
            query = "\"" + g + "\"";
        }

        IUser? currentUser = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;
        string type;
        var indexName = Constants.UmbracoIndexes.InternalIndexName;
        var fields = _treeSearcherFields.GetBackOfficeFields().ToList();

        switch (entityType)
        {
            case UmbracoEntityTypes.Member:
                indexName = Constants.UmbracoIndexes.MembersIndexName;
                type = "member";
                fields.AddRange(_treeSearcherFields.GetBackOfficeMembersFields());
                foreach (var field in _treeSearcherFields.GetBackOfficeMembersFieldsToLoad())
                {
                    fieldsToLoad.Add(field);
                }

                if (searchFrom != null && searchFrom != Constants.Conventions.MemberTypes.AllMembersListId &&
                    searchFrom.Trim() != "-1")
                {
                    sb.Append("+__NodeTypeAlias:");
                    sb.Append(searchFrom);
                    sb.Append(" ");
                }

                break;
            case UmbracoEntityTypes.Media:
                type = "media";
                fields.AddRange(_treeSearcherFields.GetBackOfficeMediaFields());
                foreach (var field in _treeSearcherFields.GetBackOfficeMediaFieldsToLoad())
                {
                    fieldsToLoad.Add(field);
                }

                var allMediaStartNodes = currentUser != null
                    ? currentUser.CalculateMediaStartNodeIds(_entityService, _appCaches)
                    : Array.Empty<int>();
                AppendPath(sb, UmbracoObjectTypes.Media, allMediaStartNodes, searchFrom, ignoreUserStartNodes,
                    _entityService);
                break;
            case UmbracoEntityTypes.Document:
                type = "content";
                fields.AddRange(_treeSearcherFields.GetBackOfficeDocumentFields());
                foreach (var field in _treeSearcherFields.GetBackOfficeDocumentFieldsToLoad())
                {
                    fieldsToLoad.Add(field);
                }

                var allContentStartNodes = currentUser != null
                    ? currentUser.CalculateContentStartNodeIds(_entityService, _appCaches)
                    : Array.Empty<int>();
                AppendPath(sb, UmbracoObjectTypes.Document, allContentStartNodes, searchFrom, ignoreUserStartNodes,
                    _entityService);
                break;
            default:
                throw new NotSupportedException("The " + typeof(BackOfficeInMemorySearcher) +
                                                " currently does not support searching against object type " +
                                                entityType);
        }

        var searcher = _inMemoryIndexManager.GetSearcher(indexName);
        if (searcher == null)
        {
            throw new NotSupportedException("The " + typeof(BackOfficeInMemorySearcher) +
                                            "cannot find searcher for index " +
                                            indexName);
        }

        var result = searcher?.CreateSearchRequest();



    }
}
