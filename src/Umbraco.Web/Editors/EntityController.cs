using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using System.Net.Http;
using Umbraco.Core.Models;
using Constants = Umbraco.Core.Constants;
using Examine;
using Umbraco.Web.Dynamics;
using System.Text.RegularExpressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using System.Web.Http.Controllers;
using Umbraco.Core.Xml;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of umbraco objects that are based on CMSNode
    /// </summary>
    /// <remarks>
    /// Some objects such as macros are not based on CMSNode
    /// </remarks>
    [EntityControllerConfiguration]
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class EntityControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(

                    //This is a special case, we'll accept a String here so that we can get page members when the special "all-members" 
                    //id is passed in eventually we'll probably want to support GUID + Udi too
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetPagedChildren", "id", typeof(int), typeof(string)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetPath", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetByIds", "ids", typeof(int[]), typeof(Guid[]), typeof(Udi[]))));
            }
        }

        /// <summary>
        /// Returns an Umbraco alias given a string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public dynamic GetSafeAlias(string value, bool camelCase = true)
        {
            var returnValue = (string.IsNullOrWhiteSpace(value)) ? string.Empty : value.ToSafeAlias(camelCase);
            dynamic returnObj = new System.Dynamic.ExpandoObject();
            returnObj.alias = returnValue;
            returnObj.original = value;
            returnObj.camelCase = camelCase;

            return returnObj;
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes type, string searchFrom = null)
        {
            //TODO: Should we restrict search results based on what app the user has access to?
            // - Theoretically you shouldn't be able to see member data if you don't have access to members right?

            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityBasic>();

            return ExamineSearch(query, type, searchFrom);
        }

        /// <summary>
        /// Searches for all content that the user is allowed to see (based on their allowed sections)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <remarks>
        /// Even though a normal entity search will allow any user to search on a entity type that they may not have access to edit, we need
        /// to filter these results to the sections they are allowed to edit since this search function is explicitly for the global search 
        /// so if we showed entities that they weren't allowed to edit they would get errors when clicking on the result.
        /// 
        /// The reason a user is allowed to search individual entity types that they are not allowed to edit is because those search
        /// methods might be used in things like pickers in the content editor.
        /// </remarks>
        [HttpGet]
        public IEnumerable<EntityTypeSearchResult> SearchAll(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityTypeSearchResult>();

            var allowedSections = Security.CurrentUser.AllowedSections.ToArray();

            var result = new List<EntityTypeSearchResult>();

            if (allowedSections.InvariantContains(Constants.Applications.Content))
            {
                result.Add(new EntityTypeSearchResult
                    {
                        Results = ExamineSearch(query, UmbracoEntityTypes.Document),
                        EntityType = UmbracoEntityTypes.Document.ToString()
                    });
            }
            if (allowedSections.InvariantContains(Constants.Applications.Media))
            {
                result.Add(new EntityTypeSearchResult
                {
                    Results = ExamineSearch(query, UmbracoEntityTypes.Media),
                    EntityType = UmbracoEntityTypes.Media.ToString()
                });
            }
            if (allowedSections.InvariantContains(Constants.Applications.Members))
            {
                result.Add(new EntityTypeSearchResult
                {
                    Results = ExamineSearch(query, UmbracoEntityTypes.Member),
                    EntityType = UmbracoEntityTypes.Member.ToString()
                });

            }
            return result;
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<int> GetPath(int id, UmbracoEntityTypes type)
        {
            var foundContent = GetResultForId(id, type);

            return foundContent.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<int> GetPath(Guid id, UmbracoEntityTypes type)
        {
            var foundContent = GetResultForKey(id, type);

            return foundContent.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<int> GetPath(Udi id, UmbracoEntityTypes type)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetPath(guidUdi.Guid, type);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);            
        }

        /// <summary>
        /// Gets the url of an entity
        /// </summary>
        /// <param name="id">Int id of the entity to fetch URL for</param>
        /// <param name="type">The tpye of entity such as Document, Media, Member</param>
        /// <returns>The URL or path to the item</returns>
        public HttpResponseMessage GetUrl(int id, UmbracoEntityTypes type)
        {
            var returnUrl = string.Empty;

            if (type == UmbracoEntityTypes.Document)
            {
                var foundUrl = Umbraco.Url(id);
                if (string.IsNullOrEmpty(foundUrl) == false && foundUrl != "#")
                {
                    returnUrl = foundUrl;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(returnUrl)
                    };
                }   
            }

            var ancestors = GetAncestors(id, type);

            //if content, skip the first node for replicating NiceUrl defaults
            if(type == UmbracoEntityTypes.Document) {
                ancestors = ancestors.Skip(1);
            }

            returnUrl = "/" + string.Join("/", ancestors.Select(x => x.Name));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(returnUrl)
            };
        }
        
        [Obsolete("Use GetyById instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EntityBasic GetByKey(Guid id, UmbracoEntityTypes type)
        {
            return GetResultForKey(id, type);
        }

        /// <summary>
        /// Gets an entity by a xpath query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeContextId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public EntityBasic GetByQuery(string query, int nodeContextId, UmbracoEntityTypes type)
        {
            //TODO: Rename this!!! It's misleading, it should be GetByXPath

          
            if (type != UmbracoEntityTypes.Document)
                throw new ArgumentException("Get by query is only compatible with enitities of type Document");


            var q = ParseXPathQuery(query, nodeContextId);
            var node = Umbraco.TypedContentSingleAtXPath(q);

            if (node == null)
                return null;

            return GetById(node.Id, type);
        }

        //PP: wip in progress on the query parser
        private string ParseXPathQuery(string query, int id)
        {
            return UmbracoXPathPathSyntaxParser.ParseXPathQuery(
                xpathExpression: query,
                nodeContextId: id,
                getPath: nodeid =>
                {
                    var ent = Services.EntityService.Get(nodeid);
                    return ent.Path.Split(',').Reverse();
                },
                publishedContentExists: i => Umbraco.TypedContent(i) != null);
        }

        #region GetById

        /// <summary>
        /// Gets an entity by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public EntityBasic GetById(int id, UmbracoEntityTypes type)
        {
            return GetResultForId(id, type);
        }

        /// <summary>
        /// Gets an entity by it's key
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public EntityBasic GetById(Guid id, UmbracoEntityTypes type)
        {
            return GetResultForKey(id, type);
        }

        /// <summary>
        /// Gets an entity by it's UDI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public EntityBasic GetById(Udi id, UmbracoEntityTypes type)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetResultForKey(guidUdi.Guid, type);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        } 
        #endregion

        #region GetByIds
        /// <summary>
        /// Get entities by integer ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// We allow for POST because there could be quite a lot of Ids
        /// </remarks>
        [HttpGet]
        [HttpPost]
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]int[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForIds(ids, type);
        }

        /// <summary>
        /// Get entities by GUID ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// We allow for POST because there could be quite a lot of Ids
        /// </remarks>
        [HttpGet]
        [HttpPost]
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]Guid[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForKeys(ids, type);
        }

        /// <summary>
        /// Get entities by UDIs
        /// </summary>
        /// <param name="ids">
        /// A list of UDIs to lookup items by, all UDIs must be of the same UDI type!
        /// </param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// We allow for POST because there could be quite a lot of Ids.
        /// </remarks>
        [HttpGet]
        [HttpPost]
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]Udi[] ids, [FromUri]UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (ids.Length == 0)
            {
                return Enumerable.Empty<EntityBasic>();
            }

            //all udi types will need to be the same in this list so we'll determine by the first
            //currently we only support GuidIdi for this method

            var guidUdi = ids[0] as GuidUdi;
            if (guidUdi != null)
            {
                return GetResultForKeys(ids.Select(x => ((GuidUdi)x).Guid).ToArray(), type);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }       
        #endregion

        [Obsolete("Use GetyByIds instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<EntityBasic> GetByKeys([FromUri]Guid[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForKeys(ids, type);
        }

        public IEnumerable<EntityBasic> GetChildren(int id, UmbracoEntityTypes type)
        {
            return GetResultForChildren(id, type);
        }

        /// <summary>
        /// Get paged child entities by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PagedResult<EntityBasic> GetPagedChildren(
            string id,
            UmbracoEntityTypes type,
            int pageNumber,
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {
            int intId;

            if (int.TryParse(id, out intId))
            {
                return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter);
            }

            Guid guidId;
            if (Guid.TryParse(id, out guidId))
            {
                //Not supported currently
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Udi udiId;
            if (Udi.TryParse(id, out udiId))
            {
                //Not supported currently
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //so we don't have an INT, GUID or UDI, it's just a string, so now need to check if it's a special id or a member type
            if (id == Constants.Conventions.MemberTypes.AllMembersListId)
            {
                //the EntityService can search paged members from the root

                intId = -1;
                return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter);
            }

            //the EntityService cannot search members of a certain type, this is currently not supported and would require 
            //quite a bit of plumbing to do in the Services/Repository, we'll revert to a paged search

            int total;
            var searchResult = ExamineSearch(filter ?? "", type, pageSize, pageNumber - 1, out total, id);

            return new PagedResult<EntityBasic>(total, pageNumber, pageSize)
            {
                Items = searchResult
            };            
        }

        /// <summary>
        /// Get paged child entities by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PagedResult<EntityBasic> GetPagedChildren(
            int id,
            UmbracoEntityTypes type,
            int pageNumber, 
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {
            if (pageNumber <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (pageSize <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            
            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                long totalRecords;
                var entities = Services.EntityService.GetPagedChildren(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter);

                if (totalRecords == 0)
                {
                    return new PagedResult<EntityBasic>(0, 0, 0);
                }

                var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
                {
                    Items = entities.Select(Mapper.Map<EntityBasic>)
                };

                return pagedResult;
            }

            //now we need to convert the unknown ones
            switch (type)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + type);
            }
        }

        public PagedResult<EntityBasic> GetPagedDescendants(
            int id,
            UmbracoEntityTypes type,
            int pageNumber,
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {
            if (pageNumber <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (pageSize <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                long totalRecords;
                //if it's from root, don't return recycled
                var entities = id == Constants.System.Root
                    ? Services.EntityService.GetPagedDescendantsFromRoot(objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter, includeTrashed:false)
                    : Services.EntityService.GetPagedDescendants(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter);

                if (totalRecords == 0)
                {
                    return new PagedResult<EntityBasic>(0, 0, 0);
                }

                var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
                {
                    Items = entities.Select(Mapper.Map<EntityBasic>)
                };

                return pagedResult;
            }

            //now we need to convert the unknown ones
            switch (type)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + type);
            }
        }

        public IEnumerable<EntityBasic> GetAncestors(int id, UmbracoEntityTypes type)
        {
            return GetResultForAncestors(id, type);
        }

        public IEnumerable<EntityBasic> GetAll(UmbracoEntityTypes type, string postFilter, [FromUri]IDictionary<string, object> postFilterParams)
        {
            return GetResultForAll(type, postFilter, postFilterParams);
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="searchFrom"></param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> ExamineSearch(string query, UmbracoEntityTypes entityType, string searchFrom = null)
        {
            int total;
            return ExamineSearch(query, entityType, 200, 0, out total, searchFrom);
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
        private IEnumerable<EntityBasic> ExamineSearch(string query, UmbracoEntityTypes entityType, int pageSize, int pageIndex, out int totalFound, string searchFrom = null)
        {
            //TODO: We need to update this to support paging

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
                    fields = new[] { "id", "__NodeId", "email", "loginName"};
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

                    if (Security.CurrentUser.StartMediaId > 0 ||
                        //if searchFrom is specified and it is greater than 0
                        (searchFrom != null && int.TryParse(searchFrom, out mediaSearchFrom) && mediaSearchFrom > 0))
                    {
                        sb.Append("+__Path: \\-1*\\,");
                        sb.Append(mediaSearchFrom > 0
                            ? mediaSearchFrom.ToString(CultureInfo.InvariantCulture)
                            : Security.CurrentUser.StartMediaId.ToString(CultureInfo.InvariantCulture));
                        sb.Append("\\,* ");
                    }
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";

                    var contentSearchFrom = int.MinValue;

                    if (Security.CurrentUser.StartContentId > 0 || 
                        //if searchFrom is specified and it is greater than 0
                        (searchFrom != null && int.TryParse(searchFrom, out contentSearchFrom) && contentSearchFrom > 0))
                    {
                        sb.Append("+__Path: \\-1*\\,");
                        sb.Append(contentSearchFrom > 0
                            ? contentSearchFrom.ToString(CultureInfo.InvariantCulture)
                            : Security.CurrentUser.StartContentId.ToString(CultureInfo.InvariantCulture));
                        sb.Append("\\,* ");
                    }
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " currently does not support searching against object type " + entityType);                    
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
                    return new List<EntityBasic>();
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
                var trimmed = query.Trim(new[] {'\"', '\''});

                //nothing to search
                if (searchFrom.IsNullOrWhiteSpace() && trimmed.IsNullOrWhiteSpace())
                {
                    totalFound = 0;
                    return new List<EntityBasic>();
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
                .Search(raw, pageSize * (pageIndex + 1)); 

            totalFound = result.TotalItemCount;

            var pagedResult = result.Skip(pageIndex);
            
            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    return MemberFromSearchResults(pagedResult.ToArray());
                case UmbracoEntityTypes.Media:
                    return MediaFromSearchResults(pagedResult);                    
                case UmbracoEntityTypes.Document:
                    return ContentFromSearchResults(pagedResult);
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " currently does not support searching against object type " + entityType);
            }
        }

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> MemberFromSearchResults(SearchResult[] results)
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
        private IEnumerable<EntityBasic> MediaFromSearchResults(IEnumerable<SearchResult> results)
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
        private IEnumerable<EntityBasic> ContentFromSearchResults(IEnumerable<SearchResult> results)
        {
            var mapped = Mapper.Map<IEnumerable<EntityBasic>>(results).ToArray();
            //add additional data
            foreach (var m in mapped)
            {
                var intId = m.Id.TryConvertTo<int>();
                if (intId.Success)
                {
                    m.AdditionalData["Url"] = Umbraco.NiceUrl(intId.Result);
                }
            }
            return mapped;
        } 

        private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support hierarchic here, some might not.

                return Services.EntityService.GetChildren(id, objectType.Value)
                    .WhereNotNull()
                    .Select(Mapper.Map<EntityBasic>);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForAncestors(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support hierarchic here, some might not.

                var ids = Services.EntityService.Get(id).Path.Split(',').Select(int.Parse).Distinct().ToArray();

                return Services.EntityService.GetAll(objectType.Value, ids)
                    .WhereNotNull()
                    .OrderBy(x => x.Level)
                    .Select(Mapper.Map<EntityBasic>);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        /// <summary>
        /// Gets the result for the entity list based on the type
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="postFilter">A string where filter that will filter the results dynamically with linq - optional</param>
        /// <param name="postFilterParams">the parameters to fill in the string where filter - optional</param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> GetResultForAll(UmbracoEntityTypes entityType, string postFilter = null, IDictionary<string, object> postFilterParams = null)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Should we order this by something ?
                var entities = Services.EntityService.GetAll(objectType.Value).WhereNotNull().Select(Mapper.Map<EntityBasic>);
                return ExecutePostFilter(entities, postFilter, postFilterParams);                
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Template:
                    var templates = Services.FileService.GetTemplates();
                    var filteredTemplates = ExecutePostFilter(templates, postFilter, postFilterParams);
                    return filteredTemplates.Select(Mapper.Map<EntityBasic>);

                case UmbracoEntityTypes.Macro:                    
                    //Get all macros from the macro service
                    var macros = Services.MacroService.GetAll().WhereNotNull().OrderBy(x => x.Name);
                    var filteredMacros = ExecutePostFilter(macros, postFilter, postFilterParams);
                    return filteredMacros.Select(Mapper.Map<EntityBasic>);

                case UmbracoEntityTypes.PropertyType:

                    //get all document types, then combine all property types into one list
                    var propertyTypes = Services.ContentTypeService.GetAllContentTypes().Cast<IContentTypeComposition>()
                                                .Concat(Services.ContentTypeService.GetAllMediaTypes())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyTypes)
                                                .DistinctBy(composition => composition.Alias);
                    var filteredPropertyTypes = ExecutePostFilter(propertyTypes, postFilter, postFilterParams);
                    return Mapper.Map<IEnumerable<PropertyType>, IEnumerable<EntityBasic>>(filteredPropertyTypes);

                case UmbracoEntityTypes.PropertyGroup:

                    //get all document types, then combine all property types into one list
                    var propertyGroups = Services.ContentTypeService.GetAllContentTypes().Cast<IContentTypeComposition>()
                                                .Concat(Services.ContentTypeService.GetAllMediaTypes())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyGroups)
                                                .DistinctBy(composition => composition.Name);
                    var filteredpropertyGroups = ExecutePostFilter(propertyGroups, postFilter, postFilterParams);
                    return Mapper.Map<IEnumerable<PropertyGroup>, IEnumerable<EntityBasic>>(filteredpropertyGroups);

                case UmbracoEntityTypes.User:

                    int total;
                    var users = Services.UserService.GetAll(0, int.MaxValue, out total);
                    var filteredUsers = ExecutePostFilter(users, postFilter, postFilterParams);
                    return Mapper.Map<IEnumerable<IUser>, IEnumerable<EntityBasic>>(filteredUsers);

                case UmbracoEntityTypes.Domain:

                case UmbracoEntityTypes.Language:

                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForKeys(Guid[] keys, UmbracoEntityTypes entityType)
        {
            if (keys.Length == 0)
                return Enumerable.Empty<EntityBasic>();

            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var entities = Services.EntityService.GetAll(objectType.Value, keys)
                    .WhereNotNull()
                    .Select(Mapper.Map<EntityBasic>);

                // entities are in "some" order, put them back in order
                var xref = entities.ToDictionary(x => x.Key);
                var result = keys.Select(x => xref.ContainsKey(x) ? xref[x] : null).Where(x => x != null);

                return result;
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForIds(int[] ids, UmbracoEntityTypes entityType)
        {
            if (ids.Length == 0)
                return Enumerable.Empty<EntityBasic>();

            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var entities = Services.EntityService.GetAll(objectType.Value, ids)
                    .WhereNotNull()
                    .Select(Mapper.Map<EntityBasic>);

                // entities are in "some" order, put them back in order
                var xref = entities.ToDictionary(x => x.Id);
                var result = ids.Select(x => xref.ContainsKey(x) ? xref[x] : null).Where(x => x != null);

                return result;
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private EntityBasic GetResultForKey(Guid key, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var found = Services.EntityService.GetByKey(key, objectType.Value);
                if (found == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return Mapper.Map<EntityBasic>(found);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:

                case UmbracoEntityTypes.PropertyGroup:

                case UmbracoEntityTypes.Domain:

                case UmbracoEntityTypes.Language:

                case UmbracoEntityTypes.User:

                case UmbracoEntityTypes.Macro:

                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private EntityBasic GetResultForId(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var found = Services.EntityService.Get(id, objectType.Value);
                if (found == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return Mapper.Map<EntityBasic>(found);
            }                
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                    
                case UmbracoEntityTypes.PropertyGroup:

                case UmbracoEntityTypes.Domain:
                    
                case UmbracoEntityTypes.Language:
                    
                case UmbracoEntityTypes.User:
                    
                case UmbracoEntityTypes.Macro:
                    
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private static UmbracoObjectTypes? ConvertToObjectType(UmbracoEntityTypes entityType)
        {
            switch (entityType)
            {
                case UmbracoEntityTypes.Document:
                    return UmbracoObjectTypes.Document;
                case UmbracoEntityTypes.Media:
                    return UmbracoObjectTypes.Media;
                case UmbracoEntityTypes.MemberType:
                    return UmbracoObjectTypes.MediaType;
                case UmbracoEntityTypes.MemberGroup:
                    return UmbracoObjectTypes.MemberGroup;
                case UmbracoEntityTypes.ContentItem:
                    return UmbracoObjectTypes.ContentItem;
                case UmbracoEntityTypes.MediaType:
                    return UmbracoObjectTypes.MediaType;
                case UmbracoEntityTypes.DocumentType:
                    return UmbracoObjectTypes.DocumentType;
                case UmbracoEntityTypes.Stylesheet:
                    return UmbracoObjectTypes.Stylesheet;
                case UmbracoEntityTypes.Member:
                    return UmbracoObjectTypes.Member;
                case UmbracoEntityTypes.DataType:
                    return UmbracoObjectTypes.DataType;
                default:
                    //There is no UmbracoEntity conversion (things like Macros, Users, etc...)
                    return null;
            }
        }

        /// <summary>
        /// Executes the post filter against a collection of objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="postFilter"></param>
        /// <param name="postFilterParams"></param>
        /// <returns></returns>
        private IEnumerable<T> ExecutePostFilter<T>(IEnumerable<T> entities, string postFilter, IDictionary<string, object> postFilterParams)
        {
            //if a post filter is assigned then try to execute it
            if (postFilter.IsNullOrWhiteSpace() == false)
            {
                return postFilterParams == null
                               ? entities.AsQueryable().Where(postFilter).ToArray()
                               : entities.AsQueryable().Where(postFilter, postFilterParams).ToArray();

            }
            return entities;
        } 

    }
}
