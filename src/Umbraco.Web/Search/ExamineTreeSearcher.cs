using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Search
{
    public class ExamineTreeSearcher : IUmbracoTreeSearcher
    {
        private readonly UmbracoHelper _umbracoHelper;

        internal ExamineTreeSearcher(UmbracoHelper umbracoHelper)
        {
            _umbracoHelper = umbracoHelper;
        }

        public IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes entityType, IUser user)
        {
            return Search(query, entityType, user, string.Empty);
        }

        public IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes entityType, IUser user, string nodeTypeAlias)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Enumerable.Empty<EntityBasic>();
            }

            return ExamineSearch(query, entityType, user, nodeTypeAlias);
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="nodeTypeAlias">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> ExamineSearch(string query, UmbracoEntityTypes entityType, IUser user, string nodeTypeAlias = null)
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
                    if (nodeTypeAlias != null && nodeTypeAlias != Constants.Conventions.MemberTypes.AllMembersListId && nodeTypeAlias.Trim() != "-1")
                    {
                        sb.Append("+__NodeTypeAlias:");
                        sb.Append(nodeTypeAlias);
                        sb.Append(" ");
                    }
                    break;
                case UmbracoEntityTypes.Media:
                    type = "media";

                    var mediaSearchFrom = int.MinValue;

                    if (user.StartMediaId > 0 ||
                        //if searchFrom is specified and it is greater than 0
                        (nodeTypeAlias != null && int.TryParse(nodeTypeAlias, out mediaSearchFrom) && mediaSearchFrom > 0))
                    {
                        sb.Append("+__Path: \\-1*\\,");
                        sb.Append(mediaSearchFrom > 0
                            ? mediaSearchFrom.ToString(CultureInfo.InvariantCulture)
                            : user.StartMediaId.ToString(CultureInfo.InvariantCulture));
                        sb.Append("\\,* ");
                    }
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";

                    var contentSearchFrom = int.MinValue;

                    if (user.StartContentId > 0 ||
                        //if searchFrom is specified and it is greater than 0
                        (nodeTypeAlias != null && int.TryParse(nodeTypeAlias, out contentSearchFrom) && contentSearchFrom > 0))
                    {
                        sb.Append("+__Path: \\-1*\\,");
                        sb.Append(contentSearchFrom > 0
                            ? contentSearchFrom.ToString(CultureInfo.InvariantCulture)
                            : user.StartContentId.ToString(CultureInfo.InvariantCulture));
                        sb.Append("\\,* ");
                    }
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(ExamineTreeSearcher) + " currently does not support searching against object type " + entityType);
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

                if (query.IsNullOrWhiteSpace())
                {
                    return new List<EntityBasic>();
                }

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
            }
            else
            {
                if (query.Trim(new[] { '\"', '\'' }).IsNullOrWhiteSpace())
                {
                    return new List<EntityBasic>();
                }

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
            }

            //must match index type
            sb.Append(") +__IndexType:");
            sb.Append(type);


            var raw = internalSearcher.CreateSearchCriteria().RawQuery(sb.ToString());

            //limit results to 200 to avoid huge over processing (CPU)
            var result = internalSearcher.Search(raw, 200);

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    return MemberFromSearchResults(result);
                case UmbracoEntityTypes.Media:
                    return MediaFromSearchResults(result);
                case UmbracoEntityTypes.Document:
                    return ContentFromSearchResults(result);
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " currently does not support searching against object type " + entityType);
            }
        }

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> MemberFromSearchResults(ISearchResults results)
        {
            var mapped = Mapper.Map<IEnumerable<EntityBasic>>(results).ToArray();
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
        private IEnumerable<EntityBasic> MediaFromSearchResults(ISearchResults results)
        {
            var mapped = Mapper.Map<IEnumerable<EntityBasic>>(results).ToArray();
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
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> ContentFromSearchResults(ISearchResults results)
        {
            var mapped = Mapper.Map<ISearchResults, IEnumerable<EntityBasic>>(results).ToArray();
            //add additional data
            foreach (var m in mapped)
            {
                var intId = m.Id.TryConvertTo<int>();
                if (intId.Success)
                {
                    m.AdditionalData["Url"] = _umbracoHelper.NiceUrl(intId.Result);
                }
            }
            return mapped;
        }
    }
}
