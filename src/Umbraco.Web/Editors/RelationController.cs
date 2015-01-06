using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Relation = Umbraco.Web.Models.ContentEditing.Relation;

namespace Umbraco.Web.Editors
{
	[PluginController("UmbracoApi")]
	[UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
    public class RelationController : UmbracoAuthorizedJsonController
	{
		public RelationController()
			: this(UmbracoContext.Current)
		{
		}

		public RelationController(UmbracoContext umbracoContext)
			: base(umbracoContext)
		{
		}

        public Relation GetById(int id)
		{
			return Mapper.Map<IRelation, Relation>(Services.RelationService.GetById(id));
		}

        //[EnsureUserPermissionForContent("childId")]
        public IEnumerable<Relation> GetByChildId(int childId, string relationTypeAlias = "")
		{
		    var relations = Services.RelationService.GetByChildId(childId).ToArray();

			if (relations.Any() == false)
			{
			    return Enumerable.Empty<Relation>();
			}

			if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
			{
			    return
			        Mapper.Map<IEnumerable<IRelation>, IEnumerable<Relation>>(
			            relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)));
			}

			return Mapper.Map<IEnumerable<IRelation>, IEnumerable<Relation>>(relations);
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