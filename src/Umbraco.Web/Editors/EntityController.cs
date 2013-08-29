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

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of any umbraco object
    /// </summary>
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {

        [UmbracoApplicationAuthorize(Constants.Applications.Content)]
        public EntityBasic GetDocumentById(int id)
        {
            return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        }

        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        public IEnumerable<EntityBasic> GetDocumentChildren(int id)
        {
            return GetChildren(id, UmbracoObjectTypes.Document);
        }

        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        public IEnumerable<EntityBasic> GetDocumentsByIds([FromUri]int[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            return GetEntitiesById(ids, UmbracoObjectTypes.Document);
        }

        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Media)]
        public EntityBasic GetMediaById(int id)
        {
            return GetEntityById(id, UmbracoObjectTypes.Media);
        }

        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Media)]
        public IEnumerable<EntityBasic> GetMediaChildren(int id)
        {
            return GetChildren(id, UmbracoObjectTypes.Media);
        }

        [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Media)]
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
