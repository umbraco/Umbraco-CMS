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

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing data types
    /// </summary>
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the developer application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.DataTypes)]
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
            
            Services.DataTypeService.Delete(foundType, UmbracoUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public DataTypeDisplay GetEmpty()
        {
            var dt = new DataTypeDefinition("");
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

        //TODO: Generally there probably won't be file uploads for pre-values but we should allow them just like we do for the content editor

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
        /// Gets the content json for all data types added by the user
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Permission is granted to this method if the user has access to any of these sections: Content, media, settings, developer, members
        /// </remarks>        
        [UmbracoApplicationAuthorize(
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
            Constants.Applications.Settings, Constants.Applications.Developer)]
        public IEnumerable<DataTypeBasic> GetAllUserConfigured()
        {
            //find all user configured for re-reference
            return Services.DataTypeService
                .GetAllDataTypeDefinitions()
                //TODO: This is pretty nasty :(
                .Where(x => x.Id > 1045)
                .Select(Mapper.Map<IDataTypeDefinition, DataTypeBasic>).Where(x => x.IsSystemDataType == false);

            //find all custom editors added by non-core manifests

            //find the rest
        }

        /// <summary>
        /// Gets the content json for all user added property editors
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Permission is granted to this method if the user has access to any of these sections: Content, media, settings, developer, members
        /// </remarks>    
        [UmbracoTreeAuthorize(
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
            Constants.Applications.Settings, Constants.Applications.Developer)]
        public IEnumerable<PropertyEditorBasic> GetAllUserPropertyEditors()
        {
            return PropertyEditorResolver.Current.PropertyEditors
                .OrderBy(x => x.Name)
                .Where(x => x.ValueEditor.View.IndexOf("app_plugins", StringComparison.InvariantCultureIgnoreCase) >= 0)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }

        /// <summary>
        /// Returns all configured data types and all potential data types that could exist based on unused property editors grouped
        /// by their property editor defined group.
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
            var datadefs = Services.DataTypeService
                                    .GetAllDataTypeDefinitions()
                                    .ToArray();

            var datatypes = new List<DataTypeBasic>();

            //this is a very specific map - if a property editor does not have prevalue - and there is a datatype already using this type, return that.
            //also, we exclude all the system listviews from the list
            var propertyEditors = PropertyEditorResolver.Current.PropertyEditors;
            foreach (var propertyEditor in propertyEditors)
            {
                var hasPrevalues = propertyEditor.PreValueEditor.Fields.Any();

                //check if a data type exists for this property editor
                var dataDef = datadefs.FirstOrDefault(x => x.PropertyEditorAlias == propertyEditor.Alias);

                //if no prevalues and a datatype exists with this property editor
                if (hasPrevalues == false && dataDef != null)
                {
                    //exclude system list views
                    if (dataDef.Name.InvariantStartsWith(Constants.Conventions.DataTypes.ListViewPrefix) == false)
                    {
                        datatypes.Add(Mapper.Map<DataTypeBasic>(dataDef));
                    }   
                }
                else
                {
                    //else, just add a clean property editor
                    var basic = Mapper.Map<DataTypeBasic>(propertyEditor);
                    basic.HasPrevalues = hasPrevalues;
                    datatypes.Add(basic);
                }
            }


            var grouped = datatypes
                .GroupBy(x => x.Group.IsNullOrWhiteSpace() ? "" : x.Group.ToLower())
                .ToDictionary(group => group.Key, group => group.AsEnumerable());

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