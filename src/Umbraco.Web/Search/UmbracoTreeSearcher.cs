using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Search
{
    internal class UmbracoTreeSearcher
    {
        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="umbracoHelper"></param>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="totalFound"></param>
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IEnumerable<SearchResultItem> ExamineSearch(
            UmbracoHelper umbracoHelper,
            string query,
            UmbracoEntityTypes entityType,
            int pageSize,
            long pageIndex, out long totalFound, string searchFrom = null)
        {
            var sb = new StringBuilder();

            string type;
            var searcher = Constants.Examine.InternalSearcher;
            var fields = new[] { "id", "__NodeId" };

            var umbracoContext = umbracoHelper.UmbracoContext;
            var appContext = umbracoContext.Application;

            //TODO: WE should really just allow passing in a lucene raw query
            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    searcher = Constants.Examine.InternalMemberSearcher;
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
                    var allMediaStartNodes = umbracoContext.Security.CurrentUser.CalculateMediaStartNodeIds(appContext.Services.EntityService);
                    AppendPath(sb, UmbracoObjectTypes.Media,  allMediaStartNodes, searchFrom, appContext.Services.EntityService);
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";
                    var allContentStartNodes = umbracoContext.Security.CurrentUser.CalculateContentStartNodeIds(appContext.Services.EntityService);
                    AppendPath(sb, UmbracoObjectTypes.Document, allContentStartNodes, searchFrom, appContext.Services.EntityService);
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) + " currently does not support searching against object type " + entityType);
            }

            var internalSearcher = ExamineManager.Instance.SearchProviderCollection[searcher];

            //build a lucene query:
            // the __nodeName will be boosted 10x without wildcards
            // then __nodeName will be matched normally with wildcards
            // the rest will be normal without wildcards


            //check if text is surrounded by single or double quotes, if so, then exact match
            var surroundedByQuotes = Regex.IsMatch(query, "^\".*?\"$")
                                     || Regex.IsMatch(query, "^\'.*?\'$");

            if (surroundedByQuotes)
            {
                //strip quotes, escape string, the replace again
                query = query.Trim(new[] { '\"', '\'' });

                query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

                //nothing to search
                if (searchFrom.IsNullOrWhiteSpace() && query.IsNullOrWhiteSpace())
                {
                    totalFound = 0;
                    return new List<SearchResultItem>();
                }

                //update the query with the query term
                if (query.IsNullOrWhiteSpace() == false)
                {
                    //add back the surrounding quotes
                    query = string.Format("{0}{1}{0}", "\"", query);

                    //node name exactly boost x 10
                    sb.Append("+(__nodeName: (");
                    sb.Append(query.ToLower());
                    sb.Append(")^10.0 ");

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
                    totalFound = 0;
                    return new List<SearchResultItem>();
                }

                //update the query with the query term
                if (trimmed.IsNullOrWhiteSpace() == false)
                {
                    query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

                    var querywords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //node name exactly boost x 10
                    sb.Append("+(__nodeName:");
                    sb.Append("\"");
                    sb.Append(query.ToLower());
                    sb.Append("\"");
                    sb.Append("^10.0 ");

                    //node name normally with wildcards
                    sb.Append(" __nodeName:");
                    sb.Append("(");
                    foreach (var w in querywords)
                    {
                        sb.Append(w.ToLower());
                        sb.Append("* ");
                    }
                    sb.Append(") ");


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

            var raw = internalSearcher.CreateSearchCriteria().RawQuery(sb.ToString());

            var result = internalSearcher
                //only return the number of items specified to read up to the amount of records to fill from 0 -> the number of items on the page requested
                .Search(raw, Convert.ToInt32(pageSize * (pageIndex + 1)));

            totalFound = result.TotalItemCount;

            var pagedResult = result.Skip(Convert.ToInt32(pageIndex));

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    return MemberFromSearchResults(pagedResult.ToArray());
                case UmbracoEntityTypes.Media:
                    return MediaFromSearchResults(pagedResult);
                case UmbracoEntityTypes.Document:
                    return ContentFromSearchResults(umbracoHelper, pagedResult);
                default:
                    throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) + " currently does not support searching against object type " + entityType);
            }
        }

        private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, int[] startNodeIds, string searchFrom, IEntityService entityService)
        {
            if (sb == null) throw new ArgumentNullException("sb");
            if (entityService == null) throw new ArgumentNullException("entityService");

            Udi udi;
            Udi.TryParse(searchFrom, true, out udi);
            searchFrom = udi == null ? searchFrom : entityService.GetIdForUdi(udi).Result.ToString();

            int searchFromId;
            var entityPath = int.TryParse(searchFrom, out searchFromId) && searchFromId > 0
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
        private IEnumerable<SearchResultItem> MemberFromSearchResults(SearchResult[] results)
        {
            var mapped = Mapper.Map<IEnumerable<SearchResultItem>>(results).ToArray();
            //add additional data
            foreach (var m in mapped)
            {
                //if no icon could be mapped, it will be set to document, so change it to picture
                if (m.Icon == "icon-document")
                {
                    m.Icon = "icon-user";
                }

                var searchResult = results.First(x => x.Id.ToInvariantString() == m.Id.ToString());
                if (searchResult.Fields.ContainsKey("email") && searchResult.Fields["email"] != null)
                {
                    m.AdditionalData["Email"] = results.First(x => x.Id.ToInvariantString() == m.Id.ToString()).Fields["email"];
                }
                if (searchResult.Fields.ContainsKey("__key") && searchResult.Fields["__key"] != null)
                {
                    Guid key;
                    if (Guid.TryParse(searchResult.Fields["__key"], out key))
                    {
                        m.Key = key;
                    }
                }
            }
            return mapped;
        }

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultItem> MediaFromSearchResults(IEnumerable<SearchResult> results)
        {
            var mapped = Mapper.Map<IEnumerable<SearchResultItem>>(results).ToArray();
            //add additional data
            foreach (var m in mapped)
            {
                //if no icon could be mapped, it will be set to document, so change it to picture
                if (m.Icon == "icon-document")
                {
                    m.Icon = "icon-picture";
                }
            }
            return mapped;
        }

        /// <summary>
        /// Returns a collection of entities for content based on search results
        /// </summary>
        /// <param name="umbracoHelper"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultItem> ContentFromSearchResults(UmbracoHelper umbracoHelper, IEnumerable<SearchResult> results)
        {
            var mapped = Mapper.Map<IEnumerable<SearchResultItem>>(results).ToArray();
            //add additional data
            foreach (var m in mapped)
            {
                var intId = m.Id.TryConvertTo<int>();
                if (intId.Success)
                {
                    m.AdditionalData["Url"] = umbracoHelper.NiceUrl(intId.Result);
                }
            }
            return mapped;
        }

    }
}
