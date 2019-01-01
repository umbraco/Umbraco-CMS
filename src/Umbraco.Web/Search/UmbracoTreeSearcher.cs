using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using SearchResult = Examine.SearchResult;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Used for internal Umbraco implementations of <see cref="ISearchableTree"/>
    /// </summary>
    public class UmbracoTreeSearcher
    {
        private readonly IExamineManager _examineManager;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly ILocalizationService _languageService;
        private readonly IEntityService _entityService;

        public UmbracoTreeSearcher(IExamineManager examineManager,
            UmbracoHelper umbracoHelper,
            ILocalizationService languageService,
            IEntityService entityService)
        {
            _examineManager = examineManager ?? throw new ArgumentNullException(nameof(examineManager));
            _umbracoHelper = umbracoHelper ?? throw new ArgumentNullException(nameof(umbracoHelper));
            _languageService = languageService;
            _entityService = entityService;
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="totalFound"></param>
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IEnumerable<SearchResultEntity> ExamineSearch(
            string query,
            UmbracoEntityTypes entityType,
            int pageSize,
            long pageIndex, out long totalFound, string searchFrom = null)
        {
            var sb = new StringBuilder();

            string type;
            var indexName = Constants.UmbracoIndexes.InternalIndexName;
            var fields = new[] { "id", "__NodeId" };

            var umbracoContext = _umbracoHelper.UmbracoContext;

            //TODO: WE should try to allow passing in a lucene raw query, however we will still need to do some manual string
            // manipulation for things like start paths, member types, etc... 
            //if (Examine.ExamineExtensions.TryParseLuceneQuery(query))
            //{

            //}

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    indexName = Constants.UmbracoIndexes.MembersIndexName;
                    type = "member";
                    fields = new[] { "id", "__NodeId", "email", "loginName" };
                    if (searchFrom != null && searchFrom != Constants.Conventions.MemberTypes.AllMembersListId && searchFrom.Trim() != "-1")
                    {
                        sb.Append("+__NodeTypeAlias:");
                        sb.Append(searchFrom);
                        sb.Append(" ");
                    }
                    break;
                case UmbracoEntityTypes.Media:
                    type = "media";
                    var allMediaStartNodes = umbracoContext.Security.CurrentUser.CalculateMediaStartNodeIds(_entityService);
                    AppendPath(sb, UmbracoObjectTypes.Media,  allMediaStartNodes, searchFrom, _entityService);
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";
                    var allContentStartNodes = umbracoContext.Security.CurrentUser.CalculateContentStartNodeIds(_entityService);
                    AppendPath(sb, UmbracoObjectTypes.Document, allContentStartNodes, searchFrom, _entityService);
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) + " currently does not support searching against object type " + entityType);
            }

            if (!_examineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException("No index found by name " + indexName);

            var internalSearcher = index.GetSearcher();

            if (!BuildQuery(sb, query, searchFrom, fields, type))
            {
                totalFound = 0;
                return Enumerable.Empty<SearchResultEntity>();
            }

            var result = internalSearcher.CreateQuery().NativeQuery(sb.ToString())
                //only return the number of items specified to read up to the amount of records to fill from 0 -> the number of items on the page requested
                .Execute(Convert.ToInt32(pageSize * (pageIndex + 1)));

            totalFound = result.TotalItemCount;

            var pagedResult = result.Skip(Convert.ToInt32(pageIndex));

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    return MemberFromSearchResults(pagedResult.ToArray());
                case UmbracoEntityTypes.Media:
                    return MediaFromSearchResults(pagedResult);
                case UmbracoEntityTypes.Document:
                    return ContentFromSearchResults(pagedResult);
                default:
                    throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) + " currently does not support searching against object type " + entityType);
            }
        }

        private bool BuildQuery(StringBuilder sb, string query, string searchFrom, string[] fields, string type)
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
                //strip quotes, escape string, the replace again
                query = query.Trim('\"', '\'');

                query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

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
                var trimmed = query.Trim(new[] { '\"', '\'' });

                //nothing to search
                if (searchFrom.IsNullOrWhiteSpace() && trimmed.IsNullOrWhiteSpace())
                {
                    return false;
                }

                //update the query with the query term
                if (trimmed.IsNullOrWhiteSpace() == false)
                {
                    query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

                    var querywords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    sb.Append("+(");

                    AppendNodeNameExactWithBoost(sb, query, allLangs);

                    AppendNodeNameWithWildcards(sb, querywords, allLangs);
                   
                    foreach (var f in fields)
                    {
                        //additional fields normally
                        sb.Append(f);
                        sb.Append(":");
                        sb.Append("(");
                        foreach (var w in querywords)
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

        private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, int[] startNodeIds, string searchFrom, IEntityService entityService)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (entityService == null) throw new ArgumentNullException(nameof(entityService));

            Udi.TryParse(searchFrom, true, out var udi);
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
            else if (startNodeIds.Contains(-1) == false) // -1 = no restriction
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
                var m = Mapper.Map<SearchResultEntity>(result);

                //if no icon could be mapped, it will be set to document, so change it to picture
                if (m.Icon == "icon-document")
                {
                    m.Icon = "icon-user";
                }
                
                if (result.Values.ContainsKey("email") && result.Values["email"] != null)
                {
                    m.AdditionalData["Email"] = result.Values["email"];
                }
                if (result.Values.ContainsKey("__key") && result.Values["__key"] != null)
                {
                    if (Guid.TryParse(result.Values["__key"], out var key))
                    {
                        m.Key = key;
                    }
                }

                yield return m;
            }
        }

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> MediaFromSearchResults(IEnumerable<ISearchResult> results)
        {
            //add additional data
            foreach (var result in results)
            {
                var m = Mapper.Map<SearchResultEntity>(result);
                //if no icon could be mapped, it will be set to document, so change it to picture
                if (m.Icon == "icon-document")
                {
                    m.Icon = "icon-picture";
                }
                yield return m;
            }
        }

        /// <summary>
        /// Returns a collection of entities for content based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> ContentFromSearchResults(IEnumerable<ISearchResult> results)
        {
            var defaultLang = _languageService.GetDefaultLanguageIsoCode();

            foreach (var result in results)
            {
                var entity = Mapper.Map<SearchResultEntity>(result);

                var intId = entity.Id.TryConvertTo<int>();
                if (intId.Success)
                {
                    //if it varies by culture, return the default language URL
                    if (result.Values.TryGetValue(UmbracoContentIndex.VariesByCultureFieldName, out var varies) && varies == "1")
                    {
                        entity.AdditionalData["Url"] = _umbracoHelper.Url(intId.Result, defaultLang);
                    }
                    else
                    {
                        entity.AdditionalData["Url"] = _umbracoHelper.Url(intId.Result);
                    }
                }

                yield return entity;
            }
        }

    }
}
