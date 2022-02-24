using System.Collections.Generic;
using System.Linq;
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
        private readonly IRelationWithRelationTypesService _relationService;
        private readonly IEntityService _entityService;

        public TrackedReferencesController(IRelationWithRelationTypesService relationService, IEntityService entityService)
        {
            _relationService = relationService;
            _entityService = entityService;
        }

        public ActionResult<PagedResult<EntityBasic>> GetPagedReferences(int id, string entityType, int pageNumber = 1, int pageSize = 100)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            var relationTypes = new string[]
            {
                Constants.Conventions.RelationTypes.RelatedDocumentAlias,
                Constants.Conventions.RelationTypes.RelatedMediaAlias
            };

            UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(entityType);
            IEnumerable<IUmbracoEntity> relations = _relationService.GetPagedParentEntitiesByChildId(id, pageNumber - 1, pageSize, out var totalRecords, relationTypes, objectType);

            return GetRelationsPagedResult(totalRecords, pageNumber, pageSize, objectType, relations);
        }

        public ActionResult<PagedResult<EntityBasic>> GetPagedDescendantsInReferences(int parentId, string entityType, int pageNumber = 1, int pageSize = 100)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            IEntitySlim currentEntity = _entityService.Get(parentId);
            if (currentEntity is null)
            {
                return NotFound();
            }

            IEnumerable<IEntitySlim> entities = _entityService.GetDescendants(currentEntity.Id);
            var ids = entities.Select(x => x.Id).ToArray();

            UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(entityType);
            IEnumerable<IUmbracoEntity> relations = _relationService.GetPagedEntitiesForItemsInRelation(ids, pageNumber - 1, pageSize, out var totalRecords, objectType);

            return GetRelationsPagedResult(totalRecords, pageNumber, pageSize, objectType, relations);
        }

        /// <summary>
        ///     Get entities of the items in relation from selected integer ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="entityType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <remarks>
        ///     We allow for POST because there could be quite a lot of ids
        /// </remarks>
        [HttpGet]
        [HttpPost]
        public ActionResult<PagedResult<EntityBasic>> CheckLinkedItems([FromJsonPath] int[] ids, string entityType, int pageNumber = 1, int pageSize = 100)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("Both pageNumber and pageSize must be greater than zero");
            }

            UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(entityType);
            IEnumerable<IUmbracoEntity> relations = _relationService.GetPagedEntitiesForItemsInRelation(ids, pageNumber - 1, pageSize, out var totalRecords, objectType);

            return GetRelationsPagedResult(totalRecords, pageNumber, pageSize, objectType, relations);
        }

        private PagedResult<EntityBasic> GetRelationsPagedResult(long totalRecords, int pageNumber, int pageSize, UmbracoObjectTypes objectType, IEnumerable<IUmbracoEntity> relations)
        {
            var udiType = ObjectTypes.GetUdiType(objectType);

            return new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
            {
                Items = relations.Cast<ContentEntitySlim>().Select(rel => new EntityBasic
                {
                    Id = rel.Id,
                    Key = rel.Key,
                    Udi = Udi.Create(udiType, rel.Key),
                    Icon = rel.ContentTypeIcon,
                    Name = rel.Name,
                    Alias = rel.ContentTypeAlias
                })
            };
        }
    }
}
