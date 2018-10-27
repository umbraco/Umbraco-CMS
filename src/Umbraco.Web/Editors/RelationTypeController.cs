using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using RelationType = Umbraco.Web.Models.ContentEditing.RelationType;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller for editing relation types.
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.RelationTypes)]
    [EnableOverrideAuthorization]
    public class RelationTypeController : BackOfficeNotificationsController
    {
        /// <summary>
        /// Gets a relation type by ID.
        /// </summary>
        /// <param name="id">The relation type ID.</param>
        /// <returns>Returns the <see cref="RelationType"/>.</returns>
        public RelationType GetById(int id)
        {
            var relationType = Services.RelationService.GetRelationTypeById(id);

            if (relationType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IRelationType, RelationType>(relationType);
        }

        public void PostSave()
        {
            throw new NotImplementedException();
        }

        public void DeleteById()
        {
            throw new NotImplementedException();
        }
    }
}
