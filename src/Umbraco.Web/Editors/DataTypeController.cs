using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using Constants = Umbraco.Core.Constants;
using System.Net.Http;
using System.Text;

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
    public class DataTypeController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Gets data type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataTypeDisplay GetByName(string name)
        {
            var dataType = Services.DataTypeService.GetDataTypeDefinitionByName(name);
            return dataType == null ? null : Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dataType);
        }

        /// <summary>
        /// Gets the datatype json for the datatype id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataTypeDisplay GetById(int id)
        {
            var dataType = Services.DataTypeService.GetDataTypeDefinitionById(id);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dataType);
        }

        /// <summary>
        /// Deletes a data type wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.DataTypeService.GetDataTypeDefinitionById(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            
            Services.DataTypeService.Delete(foundType, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public DataTypeDisplay GetEmpty(int parentId)
        {
            var dt = new DataTypeDefinition(parentId, "");
            return Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dt);
        }

        /// <summary>
        /// Returns a custom listview, based on a content type alias, if found
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns>a DataTypeDisplay</returns>
        public DataTypeDisplay GetCustomListView(string contentTypeAlias)
        {
            var dt = Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias);
            if (dt == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dt);
        }

        /// <summary>
        /// Creates a custom list view - give a document type alias
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        public DataTypeDisplay PostCreateCustomListView(string contentTypeAlias)
        {
            var dt = Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias);
            
            //if it doesnt exist yet, we will create it.
            if (dt == null)
            {
                dt = new DataTypeDefinition( Constants.PropertyEditors.ListViewAlias );
                dt.Name = Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias;
                Services.DataTypeService.Save(dt);
            }

            return Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dt);    
        }

        /// <summary>
        /// Returns the pre-values for the specified property editor
        /// </summary>
        /// <param name="editorAlias"></param>
        /// <param name="dataTypeId">The data type id for the pre-values, -1 if it is a new data type</param>
        /// <returns></returns>
        public IEnumerable<PreValueFieldDisplay> GetPreValues(string editorAlias, int dataTypeId = -1)
        {
            var propEd = PropertyEditorResolver.Current.GetByAlias(editorAlias);
            if (propEd == null)
            {
                throw new InvalidOperationException("Could not find property editor with alias " + editorAlias);
            }

            if (dataTypeId == -1)
            {
                //this is a new data type, so just return the field editors with default values
                return Mapper.Map<PropertyEditor, IEnumerable<PreValueFieldDisplay>>(propEd);
            }

            //we have a data type associated
            var dataType = Services.DataTypeService.GetDataTypeDefinitionById(dataTypeId);
            if (dataType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //now, lets check if the data type has the current editor selected, if that is true
            //we will need to wire up it's saved values. Otherwise it's an existing data type
            //that is changing it's underlying property editor, in which case there's no values.
            if (dataType.PropertyEditorAlias == editorAlias)
            {
                //this is the currently assigned pre-value editor, return with values.
                return Mapper.Map<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>(dataType);
            }

            //these are new pre-values, so just return the field editors with default values
            return Mapper.Map<PropertyEditor, IEnumerable<PreValueFieldDisplay>>(propEd);            
        }

        /// <summary>
        /// Deletes a data type container wth a given ID
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

            //finally we need to save the data type and it's pre-vals
            var dtService = ApplicationContext.Services.DataTypeService;

            //TODO: Check if the property editor has changed, if it has ensure we don't pass the 
            // existing values to the new property editor!

            //get the prevalues, current and new
            var preValDictionary = dataType.PreValues.ToDictionary(x => x.Key, x => x.Value);
            var currVal = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dataType.PersistedDataType.Id);

            //we need to allow for the property editor to deserialize the prevalues
            var formattedVal = dataType.PropertyEditor.PreValueEditor.ConvertEditorToDb(
                preValDictionary,
                currVal);

            try
            {
                //save the data type
                dtService.SaveDataTypeAndPreValues(dataType.PersistedDataType, formattedVal, (int)Security.CurrentUser.Id);
            }
            catch (DuplicateNameException ex)
            {
                ModelState.AddModelError("Name", ex.Message);
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            var display = Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dataType.PersistedDataType);
            display.AddSuccessNotification(ui.Text("speechBubbles", "dataTypeSaved"), "");

            //now return the updated model
            return display;
        }

        /// <summary>
        /// Move the media type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            var toMove = Services.DataTypeService.GetDataTypeDefinitionById(move.Id);
            if (toMove == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var result = Services.DataTypeService.Move(toMove, move.ParentId);
            if (result.Success)
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(toMove.Path, Encoding.UTF8, "application/json");
                return response;
            }

            switch (result.Result.StatusType)
            {
                case MoveOperationStatusType.FailedParentNotFound:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                case MoveOperationStatusType.FailedCancelledByEvent:
                    //returning an object of INotificationModel will ensure that any pending 
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                case MoveOperationStatusType.FailedNotAllowedByPath:
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy/notAllowedByPath"), "");
                    return Request.CreateValidationErrorResponse(notificationModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            Constants.Applications.Settings, Constants.Applications.Developer)]
        public IEnumerable<DataTypeBasic> GetAll()
        {
            return Services.DataTypeService
                     .GetAllDataTypeDefinitions()
                     .Select(Mapper.Map<IDataTypeDefinition, DataTypeBasic>).Where(x => x.IsSystemDataType == false);
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
            Constants.Applications.Settings, Constants.Applications.Developer)]
        public IDictionary<string, IEnumerable<DataTypeBasic>> GetGroupedDataTypes()
        {
            var dataTypes = Services.DataTypeService
                     .GetAllDataTypeDefinitions()
                     .Select(Mapper.Map<IDataTypeDefinition, DataTypeBasic>)
                     .ToArray();

            var propertyEditors = PropertyEditorResolver.Current.PropertyEditors.ToArray();

            foreach (var dataType in dataTypes)
            {
                var propertyEditor = propertyEditors.SingleOrDefault(x => x.Alias == dataType.Alias);
                if(propertyEditor != null)
                    dataType.HasPrevalues = propertyEditor.PreValueEditor.Fields.Any(); ;
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
            Constants.Applications.Settings, Constants.Applications.Developer)]
        public IDictionary<string, IEnumerable<DataTypeBasic>> GetGroupedPropertyEditors()
        {
            var datatypes = new List<DataTypeBasic>();
            
            var propertyEditors = PropertyEditorResolver.Current.PropertyEditors;
            foreach (var propertyEditor in propertyEditors)
            {
                var hasPrevalues = propertyEditor.PreValueEditor.Fields.Any();
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
            Constants.Applications.Settings, Constants.Applications.Developer)]
        public IEnumerable<PropertyEditorBasic> GetAllPropertyEditors()
        {
            return PropertyEditorResolver.Current.PropertyEditors
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }
        #endregion
    }
}