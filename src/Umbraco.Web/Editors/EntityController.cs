using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of any umbraco object
    /// </summary>
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {
        public EntityBasic GetDocumentById(int id)
        {
            return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        }
        public IEnumerable<EntityBasic> GetDocumentChildren(int id)
        {
            return getChildren(id, UmbracoObjectTypes.Document);
        }
        public IEnumerable<EntityBasic> GetDocumentsByIds([FromUri]int[] ids)
        {
           return getEntitiesById(ids, UmbracoObjectTypes.Document);
        }



        public EntityBasic GetMediaById(int id)
        {
            return getEntityById(id, UmbracoObjectTypes.Media);
        }
        public IEnumerable<EntityBasic> GetMediaChildren(int id)
        {
            return getChildren(id, UmbracoObjectTypes.Media);
        }
        public IEnumerable<EntityBasic> GetMediaByIds([FromUri]int[] ids)
        {
            return getEntitiesById(ids, UmbracoObjectTypes.Media);
        }





        public EntityBasic GetById(int id, string type = null)
        {
            if(string.IsNullOrEmpty(type))
                return Mapper.Map<EntityBasic>(Services.EntityService.Get(id));

            return getEntityById(id, (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), type));
        }
        public IEnumerable<EntityBasic> GetByIds([FromUri]int[] ids, string type = null)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            if (string.IsNullOrEmpty(type))
            return ids.Select(id =>
                              Mapper.Map<EntityBasic>(Services.EntityService.Get(id)));

            return getEntitiesById(ids, (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), type));
        }
        public IEnumerable<EntityBasic> GetChildren(int id, string type = null)
        {
            if (id == null) throw new ArgumentNullException("id");

            if (string.IsNullOrEmpty(type))
                return Services.EntityService.GetChildren(id).Select(x => Mapper.Map<EntityBasic>(x));

            return getChildren(id, (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), type));
        }
        public IEnumerable<EntityBasic> GetAncestors(int id, string type = null)
        {
            if (id == null) throw new ArgumentNullException("id");

            var ids = Services.EntityService.Get(id).Path.Split(',').Select(x => int.Parse(x));
            

            if (string.IsNullOrEmpty(type))
                return ids.Select(m =>
                              Mapper.Map<EntityBasic>(Services.EntityService.Get(m)));

            return getEntitiesById(ids.ToArray(), (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes),type) );
        }
        public IEnumerable<EntityBasic> GetAll(string type = "Document")
        {
            return Services.EntityService.GetAll((UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), type)).Select(x =>
                              Mapper.Map<EntityBasic>(x));
        }



        private EntityBasic getEntityById(int id, UmbracoObjectTypes type)
        {
            return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, type));
        }

        private IEnumerable<EntityBasic> getChildren(int id, UmbracoObjectTypes type)
        {
            return Services.EntityService.GetChildren(id, type)
                    .Select(child => 
                        Mapper.Map<EntityBasic>(child));
        }

        private IEnumerable<EntityBasic> getAncestors(int id, UmbracoObjectTypes type)
        {
            var ids = Services.EntityService.Get(id).Path.Split(',').Select(x => int.Parse(x));
            return getEntitiesById(ids.ToArray(), type);
        }

        private IEnumerable<EntityBasic> getEntitiesById(int[] ids, UmbracoObjectTypes type)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            return ids.Select(id =>
                              Mapper.Map<EntityBasic>(Services.EntityService.Get(id, type)));
        }

    }
}
