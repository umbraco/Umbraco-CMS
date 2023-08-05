using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Lifti;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.SpecialisedSearchers;

namespace Umbraco.Search.Lifti.SpecialisedSearchers;

public class BackOfficeInMemorySearcher : IBackOfficeExamineSearcher
{
    private readonly ILiftiIndexManager _liftiIndexManager;
    private readonly ILocalizationService _languageService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly IUmbracoTreeSearcherFields _treeSearcherFields;
    private readonly AppCaches _appCaches;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public BackOfficeInMemorySearcher(
        ILiftiIndexManager liftiIndexManager,
        ILocalizationService languageService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IUmbracoTreeSearcherFields treeSearcherFields,
        AppCaches appCaches,
        IUmbracoMapper umbracoMapper,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _liftiIndexManager = liftiIndexManager;
        _languageService = languageService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityService = entityService;
        _treeSearcherFields = treeSearcherFields;
        _appCaches = appCaches;
        _umbracoMapper = umbracoMapper;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IEnumerable<IUmbracoSearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
    {
        var sb = new StringBuilder();

        string type;
        var indexName = Constants.UmbracoIndexes.InternalIndexName;
        var fields = _treeSearcherFields.GetBackOfficeFields().ToList();

        ISet<string> fieldsToLoad = new HashSet<string>(_treeSearcherFields.GetBackOfficeFieldsToLoad());


        //special GUID check since if a user searches on one specifically we need to escape it
        if (Guid.TryParse(query, out Guid g))
        {
            query = "\"" + g + "\"";
        }

        IUser? currentUser = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

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
                    sb.Append("&__NodeTypeAlias=");
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

        var index = _liftiIndexManager.GetIndex(indexName);
        if (index == null)
        {
            throw new InvalidOperationException("No index found by name " + indexName);
        }

        if (!BuildQuery(sb, query, searchFrom, fields, type))
        {
            totalFound = 0;
            return Enumerable.Empty<IUmbracoSearchResult>();
        }

        var allResults = index.LiftiIndex.Search(sb.ToString());
        IEnumerable<SearchResult<string>> result;
        result = allResults.Skip(Convert.ToInt32(pageSize * pageIndex)).Take(pageSize);


        totalFound = allResults.Count();

        return result.ToUmbracoResults();
    }

    private bool BuildQuery(StringBuilder sb, string query, string? searchFrom, List<string> fields, string type)
    {
        //build a lucene query:
        // the nodeName will be boosted 10x without wildcards
        // then nodeName will be matched normally with wildcards
        // the rest will be normal without wildcards

        var allLangs = _languageService.GetAllLanguages().Select(x => x.IsoCode.ToLowerInvariant()).ToList();


        //check if text is surrounded by single or double quotes, if so, then exact match
        var surroundedByQuotes = Regex.IsMatch(query, "^\".*?\"$")
                                 || Regex.IsMatch(query, "^\'.*?\'$");

        if (surroundedByQuotes)
        {
            //nothing to search
            if (searchFrom.IsNullOrWhiteSpace() && query.IsNullOrWhiteSpace())
            {
                return false;
            }

            GeneratePhraseSearch(sb, query, searchFrom, fields, type, allLangs);
        }
        else
        {
            var trimmed = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);

            //nothing to search
            if (searchFrom.IsNullOrWhiteSpace() && trimmed.IsNullOrWhiteSpace())
            {
                return false;
            }

            GenerateTrimmedQuery(sb, trimmed, query, searchFrom, fields, type, allLangs);
        }

        //must match index type
        sb.Append("&__IndexType=");
        sb.Append(type);

        return true;
    }

    private void GenerateTrimmedQuery(StringBuilder sb, string trimmed, string query, string? searchFrom,
        List<string> fields, string type, List<string> allLangs)
    {
        //update the query with the query term
        if (trimmed.IsNullOrWhiteSpace() == false)
        {
            var querywords = query.Split(Constants.CharArrays.Space, StringSplitOptions.RemoveEmptyEntries);

            sb.Append("&");

            AppendNodeNameExactWithBoost(sb, new QueryWithLanguages(query, allLangs));

            AppendNodeNameWithWildcards(sb, querywords, allLangs);

            foreach (var f in fields)
            {
                var queryWordsReplaced = new string[querywords.Length];

                // when searching file names containing hyphens we need to replace the hyphens with spaces
                if (f.Equals(UmbracoSearchFieldNames.UmbracoFileFieldName))
                {
                    for (var index = 0; index < querywords.Length; index++)
                    {
                        queryWordsReplaced[index] =
                            querywords[index].Replace("\\-", " ").Replace("_", " ").Trim(" ");
                    }
                }
                else
                {
                    queryWordsReplaced = querywords;
                }

                //additional fields normally
                sb.Append(f);
                sb.Append("=");
                foreach (var w in queryWordsReplaced)
                {
                    sb.Append(w.ToLower());
                    sb.Append("* ");
                }

                sb.Append(" ");
            }
        }
    }

    private void GeneratePhraseSearch(StringBuilder sb, string query, string? searchFrom, List<string> fields,
        string type, List<string> allLangs)
    {
        //strip quotes, escape string, the replace again
        query = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);


        //update the query with the query term
        if (query.IsNullOrWhiteSpace() == false)
        {
            //add back the surrounding quotes
            query = string.Format("{0}{1}{0}", "\"", query);

            sb.Append("&");

            AppendNodeNamePhraseWithBoost(sb, new QueryWithLanguages(query, allLangs));

            foreach (var f in fields)
            {
                //additional fields normally
                sb.Append(f);
                sb.Append("= ");
                sb.Append(query);
                sb.Append(" ");
            }
        }
    }

    private void AppendNodeNamePhraseWithBoost(StringBuilder sb, QueryWithLanguages queryWithLanguages)
    {
        //node name exactly boost x 10
        sb.Append("nodeName=");
        sb.Append(queryWithLanguages.Query.ToLower());
        sb.Append("");

        //also search on all variant node names
        foreach (var lang in queryWithLanguages.Languages)
        {
            //node name exactly boost x 10
            sb.Append($"|nodeName_{lang}= ");
            sb.Append(queryWithLanguages.Query.ToLower());
            sb.Append("");
        }
    }

    private void AppendNodeNameExactWithBoost(StringBuilder sb, QueryWithLanguages queryWithLanguages)
    {
        //node name exactly boost x 10
        sb.Append("nodeName=");
        sb.Append("\"");
        sb.Append(queryWithLanguages.Query.ToLower());
        sb.Append("\"");
        sb.Append("");
        //also search on all variant node names
        foreach (var lang in queryWithLanguages.Languages)
        {
            //node name exactly boost x 10
            sb.Append($"|nodeName_{lang}=");
            sb.Append("\"");
            sb.Append(queryWithLanguages.Query.ToLower());
            sb.Append("\"");
            sb.Append("");
        }
    }

    private void AppendNodeNameWithWildcards(StringBuilder sb, string[] querywords, IEnumerable<string> allLangs)
    {
        //node name normally with wildcards
        sb.Append("nodeName=");
        sb.Append("");
        foreach (var w in querywords)
        {
            sb.Append(w.ToLower());
            sb.Append("* ");
        }

        sb.Append("");
        //also search on all variant node names
        foreach (var lang in allLangs)
        {
            //node name normally with wildcards
            sb.Append($"|nodeName_{lang}:");
            sb.Append("");
            foreach (var w in querywords)
            {
                sb.Append(w.ToLower());
                sb.Append("* ");
            }

            sb.Append("");
        }
    }

    private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, int[]? startNodeIds, string? searchFrom,
        bool ignoreUserStartNodes, IEntityService entityService)
    {
        if (sb == null)
        {
            throw new ArgumentNullException(nameof(sb));
        }

        if (entityService == null)
        {
            throw new ArgumentNullException(nameof(entityService));
        }

        UdiParser.TryParse(searchFrom, true, out Udi? udi);
        searchFrom = udi == null ? searchFrom : entityService.GetId(udi).Result.ToString();

        TreeEntityPath? entityPath =
            int.TryParse(searchFrom, NumberStyles.Integer, CultureInfo.InvariantCulture, out var searchFromId) &&
            searchFromId > 0
                ? entityService.GetAllPaths(objectType, searchFromId).FirstOrDefault()
                : null;
        if (entityPath != null)
        {
            // find... only what's underneath
            sb.Append("+__Path:");
            AppendPath(sb, entityPath.Path, false);
            sb.Append(" ");
        }
        else if (startNodeIds?.Length == 0)
        {
            // make sure we don't find anything
            sb.Append("+__Path:none ");
        }
        else if (startNodeIds?.Contains(-1) == false && ignoreUserStartNodes == false) // -1 = no restriction
        {
            IEnumerable<TreeEntityPath> entityPaths = entityService.GetAllPaths(objectType, startNodeIds);

            // for each start node, find the start node, and what's underneath
            // +__Path:(-1*,1234 -1*,1234,* -1*,5678 -1*,5678,* ...)
            sb.Append("+__Path:(");
            var first = true;
            foreach (TreeEntityPath ep in entityPaths)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(" ");
                }

                AppendPath(sb, ep.Path, true);
            }

            sb.Append(") ");
        }
    }

    private void AppendPath(StringBuilder sb, string path, bool includeThisNode)
    {
        path = path.Replace("-", "\\-").Replace(",", "\\,");
        if (includeThisNode)
        {
            sb.Append(path);
            sb.Append(" ");
        }

        sb.Append(path);
        sb.Append("\\,*");
    }

    private class QueryWithLanguages
    {
        public QueryWithLanguages(string query, List<string> languages)
        {
            Query = query;
            Languages = languages;
        }

        public string Query { get; set; }
        public List<string> Languages { get; set; }
    }
}
