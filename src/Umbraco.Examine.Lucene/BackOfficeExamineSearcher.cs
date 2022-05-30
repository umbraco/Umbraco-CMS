// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.Search;
using Lucene.Net.QueryParsers.Classic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class BackOfficeExamineSearcher : IBackOfficeExamineSearcher
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly IExamineManager _examineManager;
    private readonly ILocalizationService _languageService;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IUmbracoTreeSearcherFields _treeSearcherFields;
    private readonly IUmbracoMapper _umbracoMapper;

    public BackOfficeExamineSearcher(
        IExamineManager examineManager,
        ILocalizationService languageService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IUmbracoTreeSearcherFields treeSearcherFields,
        AppCaches appCaches,
        IUmbracoMapper umbracoMapper,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _examineManager = examineManager;
        _languageService = languageService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityService = entityService;
        _treeSearcherFields = treeSearcherFields;
        _appCaches = appCaches;
        _umbracoMapper = umbracoMapper;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public IEnumerable<ISearchResult> Search(
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

        // TODO: WE should try to allow passing in a lucene raw query, however we will still need to do some manual string
        // manipulation for things like start paths, member types, etc...
        //if (Examine.ExamineExtensions.TryParseLuceneQuery(query))
        //{

        //}

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
                AppendPath(sb, UmbracoObjectTypes.Media, allMediaStartNodes, searchFrom, ignoreUserStartNodes, _entityService);
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
                AppendPath(sb, UmbracoObjectTypes.Document, allContentStartNodes, searchFrom, ignoreUserStartNodes, _entityService);
                break;
            default:
                throw new NotSupportedException("The " + typeof(BackOfficeExamineSearcher) +
                                                " currently does not support searching against object type " +
                                                entityType);
        }

        if (!_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found by name " + indexName);
        }

        if (!BuildQuery(sb, query, searchFrom, fields, type))
        {
            totalFound = 0;
            return Enumerable.Empty<ISearchResult>();
        }

        ISearchResults? result = index.Searcher
            .CreateQuery()
            .NativeQuery(sb.ToString())
            .SelectFields(fieldsToLoad)
            //only return the number of items specified to read up to the amount of records to fill from 0 -> the number of items on the page requested
            .Execute(QueryOptions.SkipTake(Convert.ToInt32(pageSize * pageIndex), pageSize));

        totalFound = result.TotalItemCount;

        return result;
    }

    private bool BuildQuery(StringBuilder sb, string query, string? searchFrom, List<string> fields, string type)
    {
        //build a lucene query:
        // the nodeName will be boosted 10x without wildcards
        // then nodeName will be matched normally with wildcards
        // the rest will be normal without wildcards

        var allLangs = _languageService.GetAllLanguages().Select(x => x.IsoCode.ToLowerInvariant()).ToList();

        // the chars [*-_] in the query will mess everything up so let's remove those
        query = Regex.Replace(query, "[\\*\\-_]", string.Empty);

        //check if text is surrounded by single or double quotes, if so, then exact match
        var surroundedByQuotes = Regex.IsMatch(query, "^\".*?\"$")
                                 || Regex.IsMatch(query, "^\'.*?\'$");

        if (surroundedByQuotes)
        {
            //strip quotes, escape string, the replace again
            query = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);

            query = QueryParserBase.Escape(query);

            //nothing to search
            if (searchFrom.IsNullOrWhiteSpace() && query.IsNullOrWhiteSpace())
            {
                return false;
            }

            //update the query with the query term
            if (query.IsNullOrWhiteSpace() == false)
            {
                //add back the surrounding quotes
                query = string.Format("{0}{1}{0}", "\"", query);

                sb.Append("+(");

                AppendNodeNamePhraseWithBoost(sb, query, allLangs);

                foreach (var f in fields)
                {
                    //additional fields normally
                    sb.Append(f);
                    sb.Append(": (");
                    sb.Append(query);
                    sb.Append(") ");
                }

                sb.Append(") ");
            }
        }
        else
        {
            var trimmed = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);

            //nothing to search
            if (searchFrom.IsNullOrWhiteSpace() && trimmed.IsNullOrWhiteSpace())
            {
                return false;
            }

            //update the query with the query term
            if (trimmed.IsNullOrWhiteSpace() == false)
            {
                query = QueryParserBase.Escape(query);

                var querywords = query.Split(Constants.CharArrays.Space, StringSplitOptions.RemoveEmptyEntries);

                sb.Append("+(");

                AppendNodeNameExactWithBoost(sb, query, allLangs);

                AppendNodeNameWithWildcards(sb, querywords, allLangs);

                foreach (var f in fields)
                {
                    var queryWordsReplaced = new string[querywords.Length];

                    // when searching file names containing hyphens we need to replace the hyphens with spaces
                    if (f.Equals(UmbracoExamineFieldNames.UmbracoFileFieldName))
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
                    sb.Append(":");
                    sb.Append("(");
                    foreach (var w in queryWordsReplaced)
                    {
                        sb.Append(w.ToLower());
                        sb.Append("* ");
                    }

                    sb.Append(")");
                    sb.Append(" ");
                }

                sb.Append(") ");
            }
        }

        //must match index type
        sb.Append("+__IndexType:");
        sb.Append(type);

        return true;
    }

    private void AppendNodeNamePhraseWithBoost(StringBuilder sb, string query, IEnumerable<string> allLangs)
    {
        //node name exactly boost x 10
        sb.Append("nodeName: (");
        sb.Append(query.ToLower());
        sb.Append(")^10.0 ");

        //also search on all variant node names
        foreach (var lang in allLangs)
        {
            //node name exactly boost x 10
            sb.Append($"nodeName_{lang}: (");
            sb.Append(query.ToLower());
            sb.Append(")^10.0 ");
        }
    }

    private void AppendNodeNameExactWithBoost(StringBuilder sb, string query, IEnumerable<string> allLangs)
    {
        //node name exactly boost x 10
        sb.Append("nodeName:");
        sb.Append("\"");
        sb.Append(query.ToLower());
        sb.Append("\"");
        sb.Append("^10.0 ");
        //also search on all variant node names
        foreach (var lang in allLangs)
        {
            //node name exactly boost x 10
            sb.Append($"nodeName_{lang}:");
            sb.Append("\"");
            sb.Append(query.ToLower());
            sb.Append("\"");
            sb.Append("^10.0 ");
        }
    }

    private void AppendNodeNameWithWildcards(StringBuilder sb, string[] querywords, IEnumerable<string> allLangs)
    {
        //node name normally with wildcards
        sb.Append("nodeName:");
        sb.Append("(");
        foreach (var w in querywords)
        {
            sb.Append(w.ToLower());
            sb.Append("* ");
        }

        sb.Append(") ");
        //also search on all variant node names
        foreach (var lang in allLangs)
        {
            //node name normally with wildcards
            sb.Append($"nodeName_{lang}:");
            sb.Append("(");
            foreach (var w in querywords)
            {
                sb.Append(w.ToLower());
                sb.Append("* ");
            }

            sb.Append(") ");
        }
    }

    private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, int[]? startNodeIds, string? searchFrom, bool ignoreUserStartNodes, IEntityService entityService)
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
}
