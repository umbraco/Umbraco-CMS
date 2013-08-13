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
        public IUmbracoEntity GetById(int id)
        {
            return Services.EntityService.Get(id);
        }

        public IEnumerable<IUmbracoEntity> GetByIds([FromUri]int[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            return ids.Select(id => Services.EntityService.Get(id)).Where(entity => entity != null).ToList();
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
