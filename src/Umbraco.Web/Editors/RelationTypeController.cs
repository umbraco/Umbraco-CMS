using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using System.Web.Http.Controllers;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller for editing relation types.
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.RelationTypes)]
    [EnableOverrideAuthorization]
    [RelationTypeControllerConfiguration]
    public class RelationTypeController : BackOfficeNotificationsController
    {
        public RelationTypeController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class RelationTypeControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))
                ));
            }
        }

        /// <summary>
        /// Gets a relation type by id
        /// </summary>
        /// <param name="id">The relation type ID.</param>
        /// <returns>Returns the <see cref="RelationTypeDisplay"/>.</returns>
        public RelationTypeDisplay GetById(int id)
        {
            var relationType = Services.RelationService.GetRelationTypeById(id);
            if (relationType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IRelationType, RelationTypeDisplay>(relationType);
        }

        /// <summary>
        /// Gets a relation type by guid
        /// </summary>
        /// <param name="id">The relation type ID.</param>
        /// <returns>Returns the <see cref="RelationTypeDisplay"/>.</returns>
        public RelationTypeDisplay GetById(Guid id)
        {
            var relationType = Services.RelationService.GetRelationTypeById(id);
            if (relationType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IRelationType, RelationTypeDisplay>(relationType);
        }

        /// <summary>
        /// Gets a relation type by udi
        /// </summary>
        /// <param name="id">The relation type ID.</param>
        /// <returns>Returns the <see cref="RelationTypeDisplay"/>.</returns>
        public RelationTypeDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var relationType = Services.RelationService.GetRelationTypeById(guidUdi.Guid);
            if (relationType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IRelationType, RelationTypeDisplay>(relationType);
        }

        public PagedResult<RelationDisplay> GetPagedResults(int id, int pageNumber = 1, int pageSize = 100)
        {

            if (pageNumber <= 0 || pageSize <= 0)
            {
                throw new NotSupportedException("Both pageNumber and pageSize must be greater than zero");
            }

            // Ordering do we need to pass through?
            var relations = Services.RelationService.GetPagedByRelationTypeId(id, pageNumber -1, pageSize, out long totalRecords);

            return new PagedResult<RelationDisplay>(totalRecords, pageNumber, pageSize)
            {
                Items = relations.Select(x => Mapper.Map<RelationDisplay>(x))
            };
        }

        /// <summary>
        /// Gets a list of object types which can be associated via relations.
        /// </summary>
        /// <returns>A list of available object types.</returns>
        public List<ObjectType> GetRelationObjectTypes()
        {
            var objectTypes = new List<ObjectType>
            {
                new ObjectType{Id = UmbracoObjectTypes.Document.GetGuid(), Name = UmbracoObjectTypes.Document.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.Media.GetGuid(), Name = UmbracoObjectTypes.Media.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.Member.GetGuid(), Name = UmbracoObjectTypes.Member.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.DocumentType.GetGuid(), Name = UmbracoObjectTypes.DocumentType.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.MediaType.GetGuid(), Name = UmbracoObjectTypes.MediaType.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.MemberType.GetGuid(), Name = UmbracoObjectTypes.MemberType.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.DataType.GetGuid(), Name = UmbracoObjectTypes.DataType.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.MemberGroup.GetGuid(), Name = UmbracoObjectTypes.MemberGroup.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.Stylesheet.GetGuid(), Name = UmbracoObjectTypes.Stylesheet.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.ROOT.GetGuid(), Name = UmbracoObjectTypes.ROOT.GetFriendlyName()},
                new ObjectType{Id = UmbracoObjectTypes.RecycleBin.GetGuid(), Name = UmbracoObjectTypes.RecycleBin.GetFriendlyName()},
            };

            return objectTypes;
        }

        /// <summary>
        /// Creates a new relation type.
        /// </summary>
        /// <param name="relationType">The relation type to create.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> containing the persisted relation type's ID.</returns>
        public HttpResponseMessage PostCreate(RelationTypeSave relationType)
        {
            var relationTypePersisted = new RelationType(relationType.Name, relationType.Name.ToSafeAlias(true), relationType.IsBidirectional, relationType.ParentObjectType, relationType.ChildObjectType);

            try
            {
                Services.RelationService.Save(relationTypePersisted);

                return Request.CreateResponse(HttpStatusCode.OK, relationTypePersisted.Id);
            }
            catch (Exception ex)
            {
                Logger.Error<string>(GetType(), ex, "Error creating relation type with {Name}", relationType.Name);
                return Request.CreateNotificationValidationErrorResponse("Error creating relation type.");
            }
        }

        /// <summary>
        /// Updates an existing relation type.
        /// </summary>
        /// <param name="relationType">The relation type to update.</param>
        /// <returns>A display object containing the updated relation type.</returns>
        public RelationTypeDisplay PostSave(RelationTypeSave relationType)
        {
            var relationTypePersisted = Services.RelationService.GetRelationTypeById(relationType.Key);

            if (relationTypePersisted == null)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Relation type does not exist"));
            }

            Mapper.Map(relationType, relationTypePersisted);

            try
            {
                Services.RelationService.Save(relationTypePersisted);
                var display = Mapper.Map<RelationTypeDisplay>(relationTypePersisted);
                display.AddSuccessNotification("Relation type saved", "");

                return display;
            }
            catch (Exception ex)
            {
                Logger.Error<object>(GetType(), ex, "Error saving relation type with {Id}", relationType.Id);
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Something went wrong when saving the relation type"));
            }
        }

        /// <summary>
        /// Deletes a relation type with a given ID.
        /// </summary>
        /// <param name="id">The ID of the relation type to delete.</param>
        /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
        [HttpPost]
        [HttpDelete]
        public HttpResponseMessage DeleteById(int id)
        {
            var relationType = Services.RelationService.GetRelationTypeById(id);

            if(relationType == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            Services.RelationService.Delete(relationType);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
