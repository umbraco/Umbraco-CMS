using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using ClientDependency.Core;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi.Filters;
using umbraco.cms.businesslogic.packager;
using Constants = Umbraco.Core.Constants;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Web.Dynamics;
using umbraco;
using System.Text.RegularExpressions;

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
        /// Gets an entity by it's unique id if the entity supports that
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
            //no need to parse it
            if (!query.StartsWith("$"))
                return query;

            //get full path
            Func<int, IEnumerable<string>> getPath = delegate(int nodeid){
                var ent = Services.EntityService.Get(nodeid);
                return ent.Path.Split(',').Reverse();
            };

            //get nearest published item
            Func<IEnumerable<string>, int> getClosestPublishedAncestor = (path => 
            {
                
                foreach (var _id in path)
                {
                    var item = Umbraco.TypedContent(int.Parse(_id));

                    if (item != null)
                        return item.Id;
                }
                return -1;
            });

            var rootXpath = "descendant::*[@id={0}]";

            //parseable items:
            var vars = new Dictionary<string, Func<string, string>>();
            vars.Add("$current", q => {
                var _id = getClosestPublishedAncestor(getPath(id));
                return q.Replace("$current", string.Format(rootXpath, _id));
            });

            vars.Add("$parent", q =>
            {
                //remove the first item in the array if its the current node
                //this happens when current is published, but we are looking for its parent specifically
                var path = getPath(id);
                if(path.ElementAt(0) == id.ToString()){
                    path = path.Skip(1);
                }

                var _id = getClosestPublishedAncestor(path);
                return q.Replace("$parent", string.Format(rootXpath, _id));
            });


            vars.Add("$site", q =>
            {
                var _id = getClosestPublishedAncestor(getPath(id));
                return q.Replace("$site", string.Format(rootXpath, _id) + "/ancestor-or-self::*[@level = 1]");
            });


            vars.Add("$root", q =>
            {
                return q.Replace("$root", string.Empty);
            });


            foreach (var varible in vars)
            {
                if (query.StartsWith(varible.Key))
                {
                    query = varible.Value.Invoke(query);
                    break;
                }
            }

            return query;
        }
        
        public EntityBasic GetById(int id, UmbracoEntityTypes type)
        {
            return GetResultForId(id, type);
        }

        public IEnumerable<EntityBasic> GetByIds([FromUri]int[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForIds(ids, type);
        }

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
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> ExamineSearch(string query, UmbracoEntityTypes entityType, string searchFrom = null)
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

            var internalSearcher = (LuceneSearcher)ExamineManager.Instance.SearchProviderCollection[searcher];

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

        private IEnumerable<EntityBasic> GetResultForKeys(IEnumerable<Guid> keys, UmbracoEntityTypes entityType)
        {
            var keysArray = keys.ToArray();
            if (keysArray.Any() == false) return Enumerable.Empty<EntityBasic>();

            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var entities = Services.EntityService.GetAll(objectType.Value, keysArray)
                    .WhereNotNull()
                    .Select(Mapper.Map<EntityBasic>);

                // entities are in "some" order, put them back in order
                var xref = entities.ToDictionary(x => x.Id);
                var result = keysArray.Select(x => xref.ContainsKey(x) ? xref[x] : null).Where(x => x != null);

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

        private IEnumerable<EntityBasic> GetResultForIds(IEnumerable<int> ids, UmbracoEntityTypes entityType)
        {
            var idsArray = ids.ToArray();
            if (idsArray.Any() == false) return Enumerable.Empty<EntityBasic>();

            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var entities = Services.EntityService.GetAll(objectType.Value, idsArray)
                    .WhereNotNull()
                    .Select(Mapper.Map<EntityBasic>);

                // entities are in "some" order, put them back in order
                var xref = entities.ToDictionary(x => x.Id);
                var result = idsArray.Select(x => xref.ContainsKey(x) ? xref[x] : null).Where(x => x != null);

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
                case UmbracoEntityTypes.Template:
                    return UmbracoObjectTypes.Template;
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
