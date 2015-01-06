﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
	[PluginController("UmbracoApi")]
	[UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
	public class RelationController : ContentControllerBase
	{
		public RelationController()
			: this(UmbracoContext.Current)
		{
		}

		public RelationController(UmbracoContext umbracoContext)
			: base(umbracoContext)
		{
		}

		public IRelation GetById(int id)
		{
			return Services.RelationService.GetById(id);
		}

		[EnsureUserPermissionForContent("childId")]
		public IEnumerable<IRelation> GetByChildId(int childId, string relationTypeAlias = "")
		{
			var relations = Services.RelationService.GetByChildId(childId);

			if (relations == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			if (!string.IsNullOrWhiteSpace(relationTypeAlias))
			{
				return relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias));
			}

			return relations;
		}

		[HttpDelete]
		[HttpPost]
		public HttpResponseMessage DeleteById(int id)
		{
			var foundRelation = GetObjectFromRequest(() => Services.RelationService.GetById(id));

			if (foundRelation == null)
			{
				return HandleContentNotFound(id, false);
			}

			Services.RelationService.Delete(foundRelation);

			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}