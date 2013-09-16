using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of any umbraco object
    /// </summary>
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {
        [EnsureUserPermissionForContent("id")]
        [UmbracoApplicationAuthorize(Constants.Applications.Content)]
        public EntityBasic GetDocumentById(int id)
        {
            return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        }

        [EnsureUserPermissionForContent("id")]
        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        [FilterAllowedOutgoingContent(typeof(IEnumerable<EntityBasic>))]
        public IEnumerable<EntityBasic> GetDocumentChildren(int id)
        {
            return GetChildren(id, UmbracoObjectTypes.Document);
        }

        [FilterAllowedOutgoingContent(typeof(IEnumerable<EntityBasic>))]
        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        public IEnumerable<EntityBasic> GetDocumentsByIds([FromUri]int[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            return GetEntitiesById(ids, UmbracoObjectTypes.Document);
        }

        [FilterAllowedOutgoingContent(typeof(IEnumerable<EntityBasic>))]
        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        public IEnumerable<EntityBasic> SearchDocuments([FromUri]string query)
        {
            var internalSearcher = ExamineManager.Instance.SearchProviderCollection[Constants.Examine.InternalSearcher];
            var criteria = internalSearcher.CreateSearchCriteria("content", BooleanOperation.Or);
            var fields = new[] { "id", "__nodeName", "bodyText" };
            var term = new[] { query.ToLower().Escape() };
            var operation = criteria.GroupedOr(fields, term).Compile();

            var results = internalSearcher.Search(operation)
                .Select(x =>  int.Parse(x["id"]));

            return GetDocumentsByIds(results.ToArray());
        }

        /// <summary>
        /// The user must have access to either content or media for this to return data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoApplicationAuthorizeAttribute(
            Constants.Applications.Media, 
            Constants.Applications.Content)]
        [EnsureUserPermissionForMedia("id")]
        public EntityBasic GetMediaById(int id)
        {
            return GetEntityById(id, UmbracoObjectTypes.Media);
        }

        /// <summary>
        /// The user must have access to either content or media for this to return data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoApplicationAuthorizeAttribute(
            Constants.Applications.Media,
            Constants.Applications.Content)]
        [EnsureUserPermissionForMedia("id")]
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<EntityBasic>))]
        public IEnumerable<EntityBasic> GetMediaChildren(int id)
        {
            return GetChildren(id, UmbracoObjectTypes.Media);
        }


        /// <summary>
        /// The user must have access to either content or media for this to return data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoApplicationAuthorizeAttribute(
            Constants.Applications.Media,
            Constants.Applications.Content)]
        [EnsureUserPermissionForMedia("id")]
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<EntityBasic>))]
        public IEnumerable<EntityBasic> SearchMedia([FromUri]string query)
        {
            var internalSearcher = ExamineManager.Instance.SearchProviderCollection[Constants.Examine.InternalSearcher];
            var criteria = internalSearcher.CreateSearchCriteria("media", BooleanOperation.Or);
            var fields = new[] { "id", "__nodeName"};
            var term = new[] { query.ToLower().Escape() };
            var operation = criteria.GroupedOr(fields, term).Compile();

            var results = internalSearcher.Search(operation)
                .Select(x => int.Parse(x["id"]));

            return GetMediaByIds(results.ToArray());
        }

        /// <summary>
        /// The user must have access to either content or media for this to return data
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [UmbracoApplicationAuthorizeAttribute(
            Constants.Applications.Media,
            Constants.Applications.Content)]
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<EntityBasic>))]
        public IEnumerable<EntityBasic> GetMediaByIds([FromUri]int[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            return GetEntitiesById(ids, UmbracoObjectTypes.Media);
        }

        //TODO: Need to add app level security for all of this below

        public EntityBasic GetById(int id, UmbracoObjectTypes? type = null)
        {
            return type == null 
                ? Mapper.Map<EntityBasic>(Services.EntityService.Get(id)) 
                : GetEntityById(id, type.Value);
        }

        public IEnumerable<EntityBasic> GetByIds([FromUri]int[] ids, UmbracoObjectTypes? type = null)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            return type == null 
                ? ids.Select(id => Mapper.Map<EntityBasic>(Services.EntityService.Get(id))) 
                : GetEntitiesById(ids, type.Value);
        }

        public IEnumerable<EntityBasic> GetChildren(int id, UmbracoObjectTypes? type = null)
        {
            return type == null 
                ? Services.EntityService.GetChildren(id).Select(Mapper.Map<EntityBasic>)
                : GetChildren(id, type.Value);
        }

        public IEnumerable<EntityBasic> GetAncestors(int id, string type = null)
        {
            var ids = Services.EntityService.Get(id).Path.Split(',').Select(int.Parse);
            
            return string.IsNullOrEmpty(type) 
                ? ids.Select(m => Mapper.Map<EntityBasic>(Services.EntityService.Get(m))) 
                : GetEntitiesById(ids.ToArray(), (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes),type) );
        }

        public IEnumerable<EntityBasic> GetAll(UmbracoObjectTypes type = UmbracoObjectTypes.Document)
        {
            return Services.EntityService.GetAll(type).Select(Mapper.Map<EntityBasic>);
        }
        
        private EntityBasic GetEntityById(int id, UmbracoObjectTypes type)
        {
            return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, type));
        }

        private IEnumerable<EntityBasic> GetChildren(int id, UmbracoObjectTypes type)
        {
            return Services.EntityService.GetChildren(id, type).Select(Mapper.Map<EntityBasic>);
        }

        private IEnumerable<EntityBasic> GetEntitiesById(IEnumerable<int> ids, UmbracoObjectTypes type)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            return ids.Select(id => Mapper.Map<EntityBasic>(Services.EntityService.Get(id, type)));
        }

    }
}
