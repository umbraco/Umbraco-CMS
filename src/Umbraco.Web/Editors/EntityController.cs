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
        public EntityBasic GetById(int id)
        {
            return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        }


        //TODO: This should probably be change to GetContentByIds since it will be different for media, etc...!

        //TODO: Because this is a publicly accessible API, we need to filter the results for what the currently logged in user
        // is actually allowed to access. We'll need to enhance the FilterAllowedOutgoingContent to acheive that.

        public IEnumerable<EntityBasic> GetByIds([FromUri]int[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            return ids.Select(id =>
                              Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document)));
        }

    }
}
