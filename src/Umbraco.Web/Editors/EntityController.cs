using System.Collections.Generic;
using AutoMapper;
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

        public IEnumerable<IUmbracoEntity> GetByIds(int[] ids)
        {
            var list = new List<IUmbracoEntity>();
            foreach(var id in ids)
                list.Add(Services.EntityService.Get(id));

            return list;
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