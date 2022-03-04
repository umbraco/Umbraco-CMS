using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.ModelBinders;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContentOrMedia)]
    public class TrackedReferencesController : BackOfficeNotificationsController
    {
        private readonly ITrackedReferencesService _relationService;
        private readonly IEntityService _entityService;

        public TrackedReferencesController(ITrackedReferencesService relationService,
            IEntityService entityService)
        {
            _relationService = relationService;
            _entityService = entityService;
        }

        // Used by info tabs on content, media etc. So this is basically finding childs of relations.
        public ActionResult<PagedResult<RelationItem>> GetPagedReferences(int id, int pageNumber = 1,
            int pageSize = 100, bool filterMustBeIsDependency = false)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            return _relationService.GetPagedRelationsForItems(new []{id}, pageNumber - 1, pageSize, filterMustBeIsDependency);
        }

        // Used on delete, finds
        public ActionResult<PagedResult<RelationItem>> GetPagedDescendantsInReferences(int parentId, int pageNumber = 1, int pageSize = 100, bool filterMustBeIsDependency = true)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }


            return _relationService.GetPagedDescendantsInReferences(parentId, pageNumber - 1, pageSize, filterMustBeIsDependency);

        }

        // Used by unpublish content. So this is basically finding parents of relations.
        [HttpGet]
        [HttpPost]
        public ActionResult<PagedResult<RelationItem>> GetPagedReferencedItems([FromJsonPath] int[] ids, int pageNumber = 1, int pageSize = 100, bool filterMustBeIsDependency = true)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            return _relationService.GetPagedItemsWithRelations(ids, pageNumber - 1, pageSize, filterMustBeIsDependency);

        }
    }

}
