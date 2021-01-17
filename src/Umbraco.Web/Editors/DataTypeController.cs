using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using System.Net.Http;
using System.Text;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using System.Web.Http.Controllers;

namespace Umbraco.Web.Editors
{

    /// <summary>
    /// The API controller used for editing data types
    /// </summary>
    /// <remarks>
    /// The security for this controller is defined to allow full CRUD access to data types if the user has access to either:
    /// Content Types, Member Types or Media Types ... and of course to Data Types
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.DataTypes, Constants.Trees.DocumentTypes, Constants.Trees.MediaTypes, Constants.Trees.MemberTypes)]
    [EnableOverrideAuthorization]
    [DataTypeControllerConfiguration]
    public class DataTypeController : BackOfficeNotificationsController
    {
        private readonly PropertyEditorCollection _propertyEditors;

        public DataTypeController(PropertyEditorCollection propertyEditors, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _propertyEditors = propertyEditors;
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class DataTypeControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))
                ));
            }
        }

        /// <summary>
        /// Gets data type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataTypeDisplay GetByName(string name)
        {
            var dataType = Services.DataTypeService.GetDataType(name);
            return dataType == null ? null : Mapper.Map<IDataType, DataTypeDisplay>(dataType);
        }

        /// <summary>
        /// Gets the datatype json for the datatype id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTypeDisplay GetById(int id)
        {
            var dataType = Services.DataTypeService.GetDataType(id);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IDataType, DataTypeDisplay>(dataType);
        }

        /// <summary>
        /// Gets the datatype json for the datatype guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTypeDisplay GetById(Guid id)
        {
            var dataType = Services.DataTypeService.GetDataType(id);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IDataType, DataTypeDisplay>(dataType);
        }

        /// <summary>
        /// Gets the datatype json for the datatype udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTypeDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var dataType = Services.DataTypeService.GetDataType(guidUdi.Guid);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IDataType, DataTypeDisplay>(dataType);
        }

        /// <summary>
        /// Deletes a data type with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.DataTypeService.GetDataType(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.DataTypeService.Delete(foundType, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public DataTypeDisplay GetEmpty(int parentId)
        {
            // cannot create an "empty" data type, so use something by default.
            var editor = _propertyEditors[Constants.PropertyEditors.Aliases.Label];
            var dt = new DataType(editor, parentId);
            return Mapper.Map<IDataType, DataTypeDisplay>(dt);
        }

        /// <summary>
        /// Returns a custom listview, based on a content type alias, if found
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns>a DataTypeDisplay</returns>
        public DataTypeDisplay GetCustomListView(string contentTypeAlias)
        {
            var dt = Services.DataTypeService.GetDataType(Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias);
            if (dt == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IDataType, DataTypeDisplay>(dt);
        }

        /// <summary>
        /// Creates a custom list view - give a document type alias
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        public DataTypeDisplay PostCreateCustomListView(string contentTypeAlias)
        {
            var dt = Services.DataTypeService.GetDataType(Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias);

            //if it doesn't exist yet, we will create it.
            if (dt == null)
            {
                var editor = _propertyEditors[Constants.PropertyEditors.Aliases.ListView];
                dt = new DataType(editor) { Name = Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias };
                Services.DataTypeService.Save(dt);
            }

            return Mapper.Map<IDataType, DataTypeDisplay>(dt);
        }

        /// <summary>
        /// Returns the pre-values for the specified property editor
        /// </summary>
        /// <param name="editorAlias"></param>
        /// <param name="dataTypeId">The data type id for the pre-values, -1 if it is a new data type</param>
        /// <returns></returns>
        public IEnumerable<DataTypeConfigurationFieldDisplay> GetPreValues(string editorAlias, int dataTypeId = -1)
        {
            var propEd = _propertyEditors[editorAlias];
            if (propEd == null)
            {
                throw new InvalidOperationException("Could not find property editor with alias " + editorAlias);
            }

            if (dataTypeId == -1)
            {
                //this is a new data type, so just return the field editors with default values
                return Mapper.Map<IDataEditor, IEnumerable<DataTypeConfigurationFieldDisplay>>(propEd);
            }

            //we have a data type associated
            var dataType = Services.DataTypeService.GetDataType(dataTypeId);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //now, lets check if the data type has the current editor selected, if that is true
            //we will need to wire up it's saved values. Otherwise it's an existing data type
            //that is changing it's underlying property editor, in which case there's no values.
            if (dataType.EditorAlias == editorAlias)
            {
                //this is the currently assigned pre-value editor, return with values.
                return Mapper.Map<IDataType, IEnumerable<DataTypeConfigurationFieldDisplay>>(dataType);
            }

            //these are new pre-values, so just return the field editors with default values
            return Mapper.Map<IDataEditor, IEnumerable<DataTypeConfigurationFieldDisplay>>(propEd);
        }

        /// <summary>
        /// Deletes a data type container with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteContainer(int id)
        {
            Services.DataTypeService.DeleteContainer(id, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateContainer(int parentId, string name)
        {
            var result = Services.DataTypeService.CreateContainer(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        /// <summary>
        /// Saves the data type
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        [DataTypeValidate]
        public DataTypeDisplay PostSave(DataTypeSave dataType)
        {
            //If we've made it here, then everything has been wired up and validated by the attribute

            // TODO: Check if the property editor has changed, if it has ensure we don't pass the
            // existing values to the new property editor!

            // get the current configuration,
            // get the new configuration as a dictionary (this is how we get it from model)
            // and map to an actual configuration object
            var currentConfiguration = dataType.PersistedDataType.Configuration;
            var configurationDictionary = dataType.ConfigurationFields.ToDictionary(x => x.Key, x => x.Value);
            var configuration = dataType.PropertyEditor.GetConfigurationEditor().FromConfigurationEditor(configurationDictionary, currentConfiguration);

            dataType.PersistedDataType.Configuration = configuration;

            // save the data type
            try
            {
                Services.DataTypeService.Save(dataType.PersistedDataType, Security.CurrentUser.Id);
            }
            catch (DuplicateNameException ex)
            {
                ModelState.AddModelError("Name", ex.Message);
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            // map back to display model, and return
            var display = Mapper.Map<IDataType, DataTypeDisplay>(dataType.PersistedDataType);
            display.AddSuccessNotification(Services.TextService.Localize("speechBubbles", "dataTypeSaved"), "");
            return display;
        }

        /// <summary>
        /// Move the media type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            var toMove = Services.DataTypeService.GetDataType(move.Id);
            if (toMove == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var result = Services.DataTypeService.Move(toMove, move.ParentId);
            if (result.Success)
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(toMove.Path, Encoding.UTF8, "text/plain");
                return response;
            }

            switch (result.Result.Result)
            {
                case MoveOperationStatusType.FailedParentNotFound:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                case MoveOperationStatusType.FailedCancelledByEvent:
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                case MoveOperationStatusType.FailedNotAllowedByPath:
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy", "notAllowedByPath"), "");
                    return Request.CreateValidationErrorResponse(notificationModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public HttpResponseMessage PostRenameContainer(int id, string name)
        {
            var result = Services.DataTypeService.RenameContainer(id, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result)
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        /// <summary>
        /// Returns the references (usages) for the data type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTypeReferences GetReferences(int id)
        {
            var result = new DataTypeReferences();
            var usages = Services.DataTypeService.GetReferences(id);

            foreach(var groupOfEntityType in usages.GroupBy(x => x.Key.EntityType))
            {
                //get all the GUIDs for the content types to find
                var guidsAndPropertyAliases = groupOfEntityType.ToDictionary(i => ((GuidUdi)i.Key).Guid, i => i.Value);

                if (groupOfEntityType.Key == ObjectTypes.GetUdiType(UmbracoObjectTypes.DocumentType))
                    result.DocumentTypes = GetContentTypeUsages(Services.ContentTypeService.GetAll(guidsAndPropertyAliases.Keys), guidsAndPropertyAliases);
                else if (groupOfEntityType.Key == ObjectTypes.GetUdiType(UmbracoObjectTypes.MediaType))
                    result.MediaTypes = GetContentTypeUsages(Services.MediaTypeService.GetAll(guidsAndPropertyAliases.Keys), guidsAndPropertyAliases);
                else if (groupOfEntityType.Key == ObjectTypes.GetUdiType(UmbracoObjectTypes.MemberType))
                    result.MemberTypes = GetContentTypeUsages(Services.MemberTypeService.GetAll(guidsAndPropertyAliases.Keys), guidsAndPropertyAliases);
            }

            return result;
        }

        /// <summary>
        /// Maps the found content types and usages to the resulting model
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="usages"></param>
        /// <returns></returns>
        private IEnumerable<DataTypeReferences.ContentTypeReferences> GetContentTypeUsages(
            IEnumerable<IContentTypeBase> cts,
            IReadOnlyDictionary<Guid, IEnumerable<string>> usages)
        {
            return cts.Select(x => new DataTypeReferences.ContentTypeReferences
            {
                Id = x.Id,
                Key = x.Key,
                Alias = x.Alias,
                Icon = x.Icon,
                Name = x.Name,
                Udi = new GuidUdi(ObjectTypes.GetUdiType(UmbracoObjectTypes.DocumentType), x.Key),
                //only select matching properties
                Properties = x.PropertyTypes.Where(p => usages[x.Key].InvariantContains(p.Alias))
                    .Select(p => new DataTypeReferences.ContentTypeReferences.PropertyTypeReferences
                    {
                        Alias = p.Alias,
                        Name = p.Name
                    })
            });
        }

        #region ReadOnly actions to return basic data - allow access for: content ,media, members, settings, developer
        /// <summary>
        /// Gets the content json for all data types
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Permission is granted to this method if the user has access to any of these sections: Content, media, settings, developer, members
        /// </remarks>
        [UmbracoApplicationAuthorize(
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
            Constants.Applications.Settings, Constants.Applications.Packages)]
        public IEnumerable<DataTypeBasic> GetAll()
        {
            return Services.DataTypeService
                     .GetAll()
                     .Select(Mapper.Map<IDataType, DataTypeBasic>).Where(x => x.IsSystemDataType == false);
        }

        /// <summary>
        /// Returns all data types grouped by their property editor group
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Permission is granted to this method if the user has access to any of these sections: Content, media, settings, developer, members
        /// </remarks>
        [UmbracoTreeAuthorize(
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
            Constants.Applications.Settings, Constants.Applications.Packages)]
        public IDictionary<string, IEnumerable<DataTypeBasic>> GetGroupedDataTypes()
        {
            var dataTypes = Services.DataTypeService
                     .GetAll()
                     .Select(Mapper.Map<IDataType, DataTypeBasic>)
                     .ToArray();

            var propertyEditors = Current.PropertyEditors.ToArray();

            foreach (var dataType in dataTypes)
            {
                var propertyEditor = propertyEditors.SingleOrDefault(x => x.Alias == dataType.Alias);
                if (propertyEditor != null)
                    dataType.HasPrevalues = propertyEditor.GetConfigurationEditor().Fields.Any();
            }

            var grouped = dataTypes
                .GroupBy(x => x.Group.IsNullOrWhiteSpace() ? "" : x.Group.ToLower())
                .ToDictionary(group => group.Key, group => group.OrderBy(d => d.Name).AsEnumerable());

            return grouped;
        }

        /// <summary>
        /// Returns all property editors grouped
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Permission is granted to this method if the user has access to any of these sections: Content, media, settings, developer, members
        /// </remarks>
        [UmbracoTreeAuthorize(
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
            Constants.Applications.Settings, Constants.Applications.Packages)]
        public IDictionary<string, IEnumerable<DataTypeBasic>> GetGroupedPropertyEditors()
        {
            var datatypes = new List<DataTypeBasic>();
            var showDeprecatedPropertyEditors = Current.Configs.Settings().Content.ShowDeprecatedPropertyEditors;

            var propertyEditors = Current.PropertyEditors
                .Where(x=>x.IsDeprecated == false || showDeprecatedPropertyEditors);
            foreach (var propertyEditor in propertyEditors)
            {
                var hasPrevalues = propertyEditor.GetConfigurationEditor().Fields.Any();
                var basic = Mapper.Map<DataTypeBasic>(propertyEditor);
                basic.HasPrevalues = hasPrevalues;
                datatypes.Add(basic);
            }

            var grouped = datatypes
                .GroupBy(x => x.Group.IsNullOrWhiteSpace() ? "" : x.Group.ToLower())
                .ToDictionary(group => group.Key, group => group.OrderBy(d => d.Name).AsEnumerable());

            return grouped;
        }


        /// <summary>
        /// Gets all property editors defined
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Permission is granted to this method if the user has access to any of these sections: Content, media, settings, developer, members
        /// </remarks>
        [UmbracoTreeAuthorize(
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
            Constants.Applications.Settings, Constants.Applications.Packages)]
        public IEnumerable<PropertyEditorBasic> GetAllPropertyEditors()
        {
            return Current.PropertyEditors
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }
        #endregion
    }
}
