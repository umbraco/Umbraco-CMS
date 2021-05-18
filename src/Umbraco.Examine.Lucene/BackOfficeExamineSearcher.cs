// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class BackOfficeExamineSearcher : IBackOfficeExamineSearcher
    {
        private readonly IExamineManager _examineManager;
        private readonly ILocalizationService _languageService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IEntityService _entityService;
        private readonly IUmbracoTreeSearcherFields _treeSearcherFields;
        private readonly AppCaches _appCaches;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IPublishedUrlProvider _publishedUrlProvider;

        public BackOfficeExamineSearcher(IExamineManager examineManager,
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

        public IEnumerable<ISearchResult> Search(string query, UmbracoEntityTypes entityType, int pageSize, long pageIndex, out long totalFound, string searchFrom = null, bool ignoreUserStartNodes = false)
        {
            var sb = new StringBuilder();

            string type;
            var indexName = Constants.UmbracoIndexes.InternalIndexName;
            var fields = _treeSearcherFields.GetBackOfficeFields().ToList();

            //special GUID check since if a user searches on one specifically we need to escape it
            if (Guid.TryParse(query, out var g))
            {
                query = "\"" + g.ToString() + "\"";
            }

            var currentUser = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    indexName = Constants.UmbracoIndexes.MembersIndexName;
                    type = "member";
                    fields.AddRange(_treeSearcherFields.GetBackOfficeMembersFields());
                    if (searchFrom != null && searchFrom != Constants.Conventions.MemberTypes.AllMembersListId && searchFrom.Trim() != "-1")
                    {
                        sb.Append("+__NodeTypeAlias:");
                        sb.Append(searchFrom);
                        sb.Append(" ");
                    }
                    break;
                case UmbracoEntityTypes.Media:
                    type = "media";
                    fields.AddRange(_treeSearcherFields.GetBackOfficeMediaFields());
                    var allMediaStartNodes = currentUser != null
                        ? currentUser.CalculateMediaStartNodeIds(_entityService, _appCaches)
                        : Array.Empty<int>();
                    AppendPath(sb, UmbracoObjectTypes.Media, allMediaStartNodes, searchFrom, ignoreUserStartNodes, _entityService);
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";
                    fields.AddRange(_treeSearcherFields.GetBackOfficeDocumentFields());
                    var allContentStartNodes = currentUser != null
                        ? currentUser.CalculateContentStartNodeIds(_entityService, _appCaches)
                        : Array.Empty<int>();
                    AppendPath(sb, UmbracoObjectTypes.Document, allContentStartNodes, searchFrom, ignoreUserStartNodes, _entityService);
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(BackOfficeExamineSearcher) + " currently does not support searching against object type " + entityType);
            }

            if (!_examineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException("No index found by name " + indexName);

            if (!BuildQuery(sb, query, searchFrom, fields, type))
            {
                totalFound = 0;
                return Enumerable.Empty<ISearchResult>();
            }

            var result = index.Searcher
                .CreateQuery()
                .NativeQuery(sb.ToString())                
                .Execute(QueryOptions.SkipTake(Convert.ToInt32(pageSize * pageIndex), pageSize));

            totalFound = result.TotalItemCount;

            var pagedResult = result.Skip(Convert.ToInt32(pageIndex));

            return pagedResult;
        }

        private bool BuildQuery(StringBuilder sb, string query, string searchFrom, List<string> fields, string type)
        {
            //build a lucene query:
            // the nodeName will be boosted 10x without wildcards
            // then nodeName will be matched normally with wildcards
            // the rest will be normal without wildcards

            var allLangs = _languageService.GetAllLanguages().Select(x => x.IsoCode.ToLowerInvariant()).ToList();

            // the chars [*-_] in the query will mess everything up so let's remove those
            query = Regex.Replace(query, "[\\*\\-_]", "");

            //check if text is surrounded by single or double quotes, if so, then exact match
            var surroundedByQuotes = Regex.IsMatch(query, "^\".*?\"$")
                                     || Regex.IsMatch(query, "^\'.*?\'$");

            if (surroundedByQuotes)
            {
                //strip quotes, escape string, the replace again
                query = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);

                query = QueryParser.Escape(query);

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
                    query = QueryParser.Escape(query);

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
                                queryWordsReplaced[index] = querywords[index].Replace("\\-", " ").Replace("_", " ").Trim(" ");
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

        private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, int[] startNodeIds, string searchFrom, bool ignoreUserStartNodes, IEntityService entityService)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (entityService == null) throw new ArgumentNullException(nameof(entityService));

            UdiParser.TryParse(searchFrom, true, out var udi);
            searchFrom = udi == null ? searchFrom : entityService.GetId(udi).Result.ToString();

            var entityPath = int.TryParse(searchFrom, out var searchFromId) && searchFromId > 0
                ? entityService.GetAllPaths(objectType, searchFromId).FirstOrDefault()
                : null;
            if (entityPath != null)
            {
                // find... only what's underneath
                sb.Append("+__Path:");
                AppendPath(sb, entityPath.Path, false);
                sb.Append(" ");
            }
            else if (startNodeIds.Length == 0)
            {
                // make sure we don't find anything
                sb.Append("+__Path:none ");
            }
            else if (startNodeIds.Contains(-1) == false && ignoreUserStartNodes == false) // -1 = no restriction
            {
                var entityPaths = entityService.GetAllPaths(objectType, startNodeIds);

                // for each start node, find the start node, and what's underneath
                // +__Path:(-1*,1234 -1*,1234,* -1*,5678 -1*,5678,* ...)
                sb.Append("+__Path:(");
                var first = true;
                foreach (var ep in entityPaths)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(" ");
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

        // TODO: When/Where is this used?

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> MemberFromSearchResults(IEnumerable<ISearchResult> results)
        {
            //add additional data
            foreach (var result in results)
            {
                var m = _umbracoMapper.Map<SearchResultEntity>(result);

                //if no icon could be mapped, it will be set to document, so change it to picture
                if (m.Icon == Constants.Icons.DefaultIcon)
                {
                    m.Icon = Constants.Icons.Member;
                }

                if (result.Values.ContainsKey("email") && result.Values["email"] != null)
                {
                    m.AdditionalData["Email"] = result.Values["email"];
                }
                if (result.Values.ContainsKey(UmbracoExamineFieldNames.NodeKeyFieldName) && result.Values[UmbracoExamineFieldNames.NodeKeyFieldName] != null)
                {
                    if (Guid.TryParse(result.Values[UmbracoExamineFieldNames.NodeKeyFieldName], out var key))
                    {
                        m.Key = key;
                    }
                }

                yield return m;
            }
        }

        // TODO: When/Where is this used?

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> MediaFromSearchResults(IEnumerable<ISearchResult> results)
            => _umbracoMapper.Map<IEnumerable<SearchResultEntity>>(results);

        // TODO: When/Where is this used?

        /// <summary>
        /// Returns a collection of entities for content based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> ContentFromSearchResults(IEnumerable<ISearchResult> results, string culture = null)
        {
            var defaultLang = _languageService.GetDefaultLanguageIsoCode();
            foreach (var result in results)
            {
                var entity = _umbracoMapper.Map<SearchResultEntity>(result, context =>
                {
                    if (culture != null)
                    {
                        context.SetCulture(culture);
                    }
                }
                );

                var intId = entity.Id.TryConvertTo<int>();
                if (intId.Success)
                {
                    //if it varies by culture, return the default language URL
                    if (result.Values.TryGetValue(UmbracoExamineFieldNames.VariesByCultureFieldName, out var varies) && varies == "y")
                    {
                        entity.AdditionalData["Url"] = _publishedUrlProvider.GetUrl(intId.Result, culture: culture ?? defaultLang);
                    }
                    else
                    {
                        entity.AdditionalData["Url"] = _publishedUrlProvider.GetUrl(intId.Result);
                    }
                }

                yield return entity;
            }
        }

    }
}
