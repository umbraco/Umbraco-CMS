using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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
        private readonly IRelationService _relationService;

        public RelationController(IRelationService relationService)
        {
            _relationService = relationService;
        }

        public RelationDisplay GetById(int id)
        {
            return Mapper.Map<IRelation, RelationDisplay>(_relationService.GetById(id));
        }

        //[EnsureUserPermissionForContent("childId")]
        public IEnumerable<RelationDisplay> GetByChildId(int childId, string relationTypeAlias = "")
        {
            var relations = _relationService.GetByChildId(childId).ToArray();

            if (relations.Any() == false)
            {
                return Enumerable.Empty<RelationDisplay>();
            }

            if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
            {
                return
                    Mapper.MapEnumerable<IRelation, RelationDisplay>(
                        relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)));
            }

            return Mapper.MapEnumerable<IRelation, RelationDisplay>(relations);
        }

        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundRelation = _relationService.GetById(id);

            if (foundRelation == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No relation found with the specified id");
            }

            _relationService.Delete(foundRelation);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
