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
    /// The API controller used for using the list of sections
    /// </summary>
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {
        public EntityDisplay GetById(int id)
        {

            return map((UmbracoEntity)Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        }

        public IEnumerable<EntityDisplay> GetByIds([FromUri]int[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            return ids.Select(id => map(((UmbracoEntity)Services.EntityService.Get(id, UmbracoObjectTypes.Document)))).Where(entity => entity != null).ToList();
        }

        private EntityDisplay map(UmbracoEntity input)
        {
            EntityDisplay output = new EntityDisplay();
            output.Name = input.Name;
            output.Id = input.Id;
            output.Key = input.Key;
            output.Icon = input.ContentTypeIcon;
            return output;
        }

        //public IEnumerable<UmbracoEntity> GetContentByIds(int[] ids)
        //{
        //    var list = new List<UmbracoEntity>();
        //    foreach (var id in ids)
        //        list.Add((UmbracoEntity)Services.EntityService.Get(id));

        //    return list;
        //}

        //public UmbracoEntity GetContentById(int id)
        //{
        //    return (UmbracoEntity)Services.EntityService.Get(id);
        //}
    }
}
