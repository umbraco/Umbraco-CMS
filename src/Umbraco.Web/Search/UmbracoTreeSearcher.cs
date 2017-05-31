using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Examine;
using Umbraco.Core;
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

                    var mediaSearchFrom = int.MinValue;

                    if (umbracoHelper.UmbracoContext.Security.CurrentUser.StartMediaId > 0 ||
                        //if searchFrom is specified and it is greater than 0
                        (searchFrom != null && int.TryParse(searchFrom, out mediaSearchFrom) && mediaSearchFrom > 0))
                    {
                        sb.Append("+__Path: \\-1*\\,");
                        sb.Append(mediaSearchFrom > 0
                            ? mediaSearchFrom.ToString(CultureInfo.InvariantCulture)
                            : umbracoHelper.UmbracoContext.Security.CurrentUser.StartMediaId.ToString(CultureInfo.InvariantCulture));
                        sb.Append("\\,* ");
                    }
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";

                    var contentSearchFrom = int.MinValue;

                    if (umbracoHelper.UmbracoContext.Security.CurrentUser.StartContentId > 0 ||
                        //if searchFrom is specified and it is greater than 0
                        (searchFrom != null && int.TryParse(searchFrom, out contentSearchFrom) && contentSearchFrom > 0))
                    {
                        sb.Append("+__Path: \\-1*\\,");
                        sb.Append(contentSearchFrom > 0
                            ? contentSearchFrom.ToString(CultureInfo.InvariantCulture)
                            : umbracoHelper.UmbracoContext.Security.CurrentUser.StartContentId.ToString(CultureInfo.InvariantCulture));
                        sb.Append("\\,* ");
                    }
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