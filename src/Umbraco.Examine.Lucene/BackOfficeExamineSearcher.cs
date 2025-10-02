// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.Search;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
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
    private readonly ILanguageService _languageService;
    private readonly IUmbracoTreeSearcherFields _treeSearcherFields;

    public BackOfficeExamineSearcher(
        IExamineManager examineManager,
        ILanguageService languageService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IUmbracoTreeSearcherFields treeSearcherFields,
        AppCaches appCaches)
    {
        _examineManager = examineManager;
        _languageService = languageService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityService = entityService;
        _treeSearcherFields = treeSearcherFields;
        _appCaches = appCaches;
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V18.")]
    public BackOfficeExamineSearcher(
        IExamineManager examineManager,
        ILocalizationService languageService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IUmbracoTreeSearcherFields treeSearcherFields,
        AppCaches appCaches,
        IUmbracoMapper umbracoMapper,
        IPublishedUrlProvider publishedUrlProvider)
        : this(
            examineManager,
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
            backOfficeSecurityAccessor,
            entityService,
            treeSearcherFields,
            appCaches)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V18.")]
    public BackOfficeExamineSearcher(
        IExamineManager examineManager,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IUmbracoTreeSearcherFields treeSearcherFields,
        AppCaches appCaches,
        IUmbracoMapper umbracoMapper,
        IPublishedUrlProvider publishedUrlProvider)
        : this(
            examineManager,
            languageService,
            backOfficeSecurityAccessor,
            entityService,
            treeSearcherFields,
            appCaches)
    {
    }

    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    public IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
        => Search(query, entityType, pageSize, pageIndex, out totalFound, null, null, searchFrom, ignoreUserStartNodes);

    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    public IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string[]? contentTypeAliases,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
        => Search(query, entityType, pageSize, pageIndex, out totalFound, contentTypeAliases, null, searchFrom, ignoreUserStartNodes);

    public IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string[]? contentTypeAliases,
        bool? trashed,
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
        else
        {
            // No Guid so no need to search the __Key field to prevent irrelevant results
            fields.Remove(UmbracoExamineFieldNames.NodeKeyFieldName);
        }

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
                    searchFrom.Trim() != Constants.System.RootString)
                {
                    sb.Append("+__NodeTypeAlias:");
                    sb.Append(searchFrom);
                    sb.Append(' ');
                }

                break;
            case UmbracoEntityTypes.Media:
                type = "media";
                fields.AddRange(_treeSearcherFields.GetBackOfficeMediaFields());
                foreach (var field in _treeSearcherFields.GetBackOfficeMediaFieldsToLoad())
                {
                    fieldsToLoad.Add(field);
                }

                AppendPath(sb, UmbracoObjectTypes.Media, searchFrom, ignoreUserStartNodes, out var abortMediaQuery);
                if (abortMediaQuery)
                {
                    totalFound = 0;
                    return [];
                }

                if (trashed.HasValue)
                {
                    AppendRequiredTrashPath(trashed.Value, sb, Constants.System.RecycleBinMedia);
                }

                break;
            case UmbracoEntityTypes.Document:
                type = "content";
                fields.AddRange(_treeSearcherFields.GetBackOfficeDocumentFields());
                foreach (var field in _treeSearcherFields.GetBackOfficeDocumentFieldsToLoad())
                {
                    fieldsToLoad.Add(field);
                }

                AppendPath(sb, UmbracoObjectTypes.Document, searchFrom, ignoreUserStartNodes, out var abortContentQuery);
                if (abortContentQuery)
                {
                    totalFound = 0;
                    return [];
                }

                if (trashed.HasValue)
                {
                    AppendRequiredTrashPath(trashed.Value, sb, Constants.System.RecycleBinContent);
                }

                break;
            default:
                throw new NotSupportedException("The " + typeof(BackOfficeExamineSearcher) +
                                                " currently does not support searching against object type " +
                                                entityType);
        }

        if (contentTypeAliases?.Any() is true)
        {
            sb.Append($"+({string.Join(" ", contentTypeAliases.Select(alias => $"{ExamineFieldNames.ItemTypeFieldName}:{alias}"))}) ");
        }

        if (!_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found by name " + indexName);
        }

        if (!BuildQuery(sb, query, searchFrom, fields, type))
        {
            totalFound = 0;
            return [];
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

    private void AppendRequiredTrashPath(bool trashed, StringBuilder sb, int recycleBinId)
    {
        var requiredOrNotString = trashed ? "+" : "!";
        var trashPath = $"-1,{recycleBinId}";
        trashPath = trashPath.Replace("-", "\\-").Replace(",", "\\,");
        sb.Append($"{requiredOrNotString}__Path:{trashPath}\\,* ");
    }

    private bool BuildQuery(StringBuilder sb, string query, string? searchFrom, List<string> fields, string type)
    {
        //build a lucene query:
        // the nodeName will be boosted 10x without wildcards
        // then nodeName will be matched normally with wildcards
        // the rest will be normal without wildcards

        var allLangs = _languageService.GetAllIsoCodesAsync().GetAwaiter().GetResult().Select(x => x.ToLowerInvariant()).ToList();

        // the chars [*-_] in the query will mess everything up so let's remove those
        // However we cannot just remove - and _  since these signify a space, so we instead replace them with that.
        query = Regex.Replace(query, "[\\*]", string.Empty);
        query = Regex.Replace(query, "[\\-_]", " ");


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
                    sb.Append(':');
                    sb.Append('(');
                    foreach (var w in queryWordsReplaced)
                    {
                        sb.Append(w.ToLower());
                        sb.Append("* ");
                    }

                    sb.Append(')');
                    sb.Append(' ');
                }

                sb.Append(") ");
            }
        }

        //must match index type
        sb.Append("+__IndexType:");
        sb.Append(type);

        return true;
    }

    private static void AppendNodeNamePhraseWithBoost(StringBuilder sb, string query, IEnumerable<string> allLangs)
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

    private static void AppendNodeNameExactWithBoost(StringBuilder sb, string query, IEnumerable<string> allLangs)
    {
        //node name exactly boost x 10
        sb.Append("nodeName:");
        sb.Append('"');
        sb.Append(query.ToLower());
        sb.Append('"');
        sb.Append("^10.0 ");
        //also search on all variant node names
        foreach (var lang in allLangs)
        {
            //node name exactly boost x 10
            sb.Append("nodeName_").Append(lang).Append(':');
            sb.Append('"');
            sb.Append(query.ToLower());
            sb.Append('"');
            sb.Append("^10.0 ");
        }
    }

    private static void AppendNodeNameWithWildcards(StringBuilder sb, string[] querywords, IEnumerable<string> allLangs)
    {
        //node name normally with wildcards
        sb.Append("nodeName:");
        sb.Append('(');
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
            sb.Append('(');
            foreach (var w in querywords)
            {
                sb.Append(w.ToLower());
                sb.Append("* ");
            }

            sb.Append(") ");
        }
    }

    private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, string? searchFrom, bool ignoreUserStartNodes, out bool abortQuery)
    {
        ArgumentNullException.ThrowIfNull(sb);

        abortQuery = false;

        if (searchFrom is Constants.System.RootString)
        {
            searchFrom = null;
        }

        var userStartNodes = ignoreUserStartNodes ? [Constants.System.Root] : GetUserStartNodes(objectType);
        if (searchFrom is null && userStartNodes.Contains(Constants.System.Root))
        {
            // If we have no searchFrom and the user either has access to the root node or we are ignoring user
            // start nodes, we don't need to filter by path.
            return;
        }

        string[] pathsToFilter;
        if (searchFrom is null)
        {
            // If we don't want to filter by a specific entity, we can simply use the user start nodes.
            pathsToFilter = GetEntityPaths(objectType, userStartNodes);
        }
        else
        {
            TreeEntityPath? searchFromPath = GetEntityPath(searchFrom, objectType);
            if (searchFromPath is null)
            {
                // If the searchFrom cannot be found, return no results.
                // This is to prevent showing entities outside the intended filter.
                abortQuery = true;
                return;
            }

            var userStartNodePaths = GetEntityPaths(objectType, userStartNodes);

            // If the user has access to the entity, we can simply filter by the entity path.
            if (userStartNodePaths.Any(userStartNodePath => StartsWithPath(searchFromPath.Path, userStartNodePath)))
            {
                sb.Append("+__Path:");
                AppendPath(sb, searchFromPath.Path, false);
                sb.Append(' ');
                return;
            }

            // If the user does not have access to the entity, let's filter the paths by the ones that start with the
            // entity path (are descendants of the entity).
            pathsToFilter = userStartNodePaths.Where(ep => StartsWithPath(ep, searchFromPath.Path)).ToArray();
        }

        // If we have no paths left, no need to perform the query at all, just return no results.
        if (pathsToFilter.Length == 0)
        {
            abortQuery = true;
            return;
        }

        // For each start node, find the start node, and what's underneath
        // +__Path:(-1*,1234 -1*,1234,* -1*,5678 -1*,5678,* ...)
        sb.Append("+__Path:(");
        var first = true;
        foreach (string pathToFilter in pathsToFilter)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(' ');
            }

            AppendPath(sb, pathToFilter, true);
        }

        sb.Append(") ");
    }

    private static void AppendPath(StringBuilder sb, string path, bool includeThisNode)
    {
        path = path.Replace("-", "\\-");
        if (includeThisNode)
        {
            sb.Append(path);
            sb.Append(' ');
        }

        sb.Append(path);
        sb.Append(",*");
    }

    private static bool StartsWithPath(string path1, string path2)
    {
        if (path1.StartsWith(path2) == false)
        {
            return false;
        }

        return path1.Length == path2.Length || path1[path2.Length] == ',';
    }

    private int[] GetUserStartNodes(UmbracoObjectTypes objectType)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (currentUser is null)
        {
            return [];
        }

        var startNodes = objectType switch
        {
            UmbracoObjectTypes.Document => currentUser.CalculateContentStartNodeIds(_entityService, _appCaches),
            UmbracoObjectTypes.Media => currentUser.CalculateMediaStartNodeIds(_entityService, _appCaches),
            _ => throw new NotSupportedException($"The object type {objectType} is not supported for start nodes."),
        };

        return startNodes ?? [Constants.System.Root]; // If no start nodes are defined, we assume the user has access to the root node (-1).
    }

    private string[] GetEntityPaths(UmbracoObjectTypes objectType, int[] entityIds) =>
        entityIds switch
        {
            [] => [],
            _ when entityIds.Contains(Constants.System.Root) => [Constants.System.RootString],
            _ => _entityService.GetAllPaths(objectType, entityIds).Select(x => x.Path).ToArray(),
        };

    private TreeEntityPath? GetEntityPath(string? searchFrom, UmbracoObjectTypes objectType)
    {
        if (searchFrom is null)
        {
            return null;
        }

        Guid? entityKey = null;
        if (Guid.TryParse(searchFrom, out Guid entityGuid))
        {
            entityKey = entityGuid;
        } // fallback to Udi for legacy reasons as the calling methods take string?
        else if (UdiParser.TryParse(searchFrom, true, out Udi? udi) && udi is GuidUdi guidUdi)
        {
            entityKey = guidUdi.Guid;
        }
        else if (int.TryParse(searchFrom, NumberStyles.Integer, CultureInfo.InvariantCulture, out var entityId)
                 && entityId > 0
                 && _entityService.GetKey(entityId, objectType) is { Success: true } attempt)
        {
            entityKey = attempt.Result;
        }

        return entityKey is null ? null : _entityService.GetAllPaths(objectType, entityKey.Value).FirstOrDefault();
    }
}
