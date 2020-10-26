using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Media)]
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    public class TrackedReferencesController : BackOfficeNotificationsController
    {
        public TrackedReferencesController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper) : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        public PagedResult<EntityBasic> GetPagedReferences(int id, string entityType, int pageNumber = 1, int pageSize = 100)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                throw new NotSupportedException("Both pageNumber and pageSize must be greater than zero");
            }

            var objectType = ObjectTypes.GetUmbracoObjectType(entityType);
            var udiType = ObjectTypes.GetUdiType(objectType);
            var relationTypes = new string[]
            {
                Constants.Conventions.RelationTypes.RelatedDocumentAlias,
                Constants.Conventions.RelationTypes.RelatedMediaAlias
            };

            var relations = Services.RelationService.GetPagedParentEntitiesByChildId(id, pageNumber - 1, pageSize, out var totalRecords, relationTypes, objectType);

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

        [HttpGet]
        public bool HasReferencesInDescendants(int id, string entityType)
        {
            var currentEntity = this.Services.EntityService.Get(id);
            
            if (currentEntity != null)
            {
                var currentObjectType = ObjectTypes.GetUmbracoObjectType(currentEntity.NodeObjectType);

                var objectType = ObjectTypes.GetUmbracoObjectType(entityType);
                var relationTypes = new string[]
                {
                    Constants.Conventions.RelationTypes.RelatedDocumentAlias,
                    Constants.Conventions.RelationTypes.RelatedMediaAlias
                };

                var pageSize = 1000;
                var currentPage = 0;

                var entities = new List<IEntitySlim>();

                do
                {
                    entities = this.Services.EntityService.GetPagedDescendants(id, currentObjectType, currentPage, pageSize, out _)
                        .ToList();

                    var ids = entities.Select(x => x.Id).ToArray();

                    var relations =
                        this.Services.RelationService.GetPagedParentEntitiesByChildIds(ids, 0, int.MaxValue, out _,
                            relationTypes, objectType);

                    if (relations.Any())
                    {
                        return true;
                    }

                    currentPage++;

                } while (entities.Count == pageSize);
            }
            
            return false;
        }
    }
}
