using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
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

        public TrackedReferencesController(ITrackedReferencesService relationService)
        {
            _relationService = relationService;
        }

        /// <summary>
        /// Gets a page list of tracked references for the current item, so you can see where an item is being used.
        /// </summary>
        /// <remarks>
        /// Used by info tabs on content, media etc. and for the delete and unpublish of single items.
        /// This is basically finding childs of relations.
        /// </remarks>
        public ActionResult<PagedResult<RelationItem>> GetPagedReferences(int id, int pageNumber = 1, int pageSize = 100, bool filterMustBeIsDependency = false)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            return _relationService.GetPagedRelationsForItems(new[] { id }, pageNumber - 1, pageSize, filterMustBeIsDependency);
        }

        // Used on delete, finds

        /// <summary>
        /// Gets a page list of the child nodes of the current item used in any kind of relation.
        /// </summary>
        /// <remarks>
        /// Used when deleting and unpublishing a single item to check if this item has any descending items that are in any kind of relation.
        /// This is basically finding ...
        /// </remarks>
        public ActionResult<PagedResult<RelationItem>> GetPagedDescendantsInReferences(int parentId, int pageNumber = 1, int pageSize = 100, bool filterMustBeIsDependency = true)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            return _relationService.GetPagedDescendantsInReferences(parentId, pageNumber - 1, pageSize, filterMustBeIsDependency);
        }

        // Used by unpublish content. So this is basically finding parents of relations.

        /// <summary>
        /// Gets a page list of the items used in any kind of relation from selected integer ids.
        /// </summary>
        /// <remarks>
        /// Used when bulk deleting content/media and bulk unpublishing content (delete and unpublish on List view).
        /// This is basically finding ...
        /// </remarks>
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
