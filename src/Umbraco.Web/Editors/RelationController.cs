using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    public class RelationController : UmbracoAuthorizedJsonController
    {
        public RelationDisplay GetById(int id)
        {
            return Mapper.Map<IRelation, RelationDisplay>(Services.RelationService.GetById(id));
        }

        //[EnsureUserPermissionForContent("childId")]
        public IEnumerable<RelationDisplay> GetByChildId(int childId, string relationTypeAlias = "")
        {
            var relations = Services.RelationService.GetByChildId(childId).ToArray();

            if (relations.Any() == false)
            {
                return Enumerable.Empty<RelationDisplay>();
            }

            if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
            {
                return
                    Mapper.Map<IEnumerable<IRelation>, IEnumerable<RelationDisplay>>(
                        relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)));
            }

            return Mapper.Map<IEnumerable<IRelation>, IEnumerable<RelationDisplay>>(relations);
        }

        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundRelation = Services.RelationService.GetById(id);

            if (foundRelation == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No relation found with the specified id");
            }

            Services.RelationService.Delete(foundRelation);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
