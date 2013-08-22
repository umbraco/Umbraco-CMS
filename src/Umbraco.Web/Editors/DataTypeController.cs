using System;
using System.Collections.Generic;
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
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using Constants = Umbraco.Core.Constants;

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
    [UmbracoApplicationAuthorize(Constants.Applications.Developer)]
    public class DataTypeController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Gets the content json for the content id
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

        public DataTypeDisplay GetEmpty()
        {
            var dt = new DataTypeDefinition(-1, Guid.Empty);
            return Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dt);
        }

        /// <summary>
        /// Returns the pre-values for the specified property editor
        /// </summary>
        /// <param name="editorId"></param>
        /// <param name="dataTypeId">The data type id for the pre-values, -1 if it is a new data type</param>
        /// <returns></returns>
        public IEnumerable<PreValueFieldDisplay> GetPreValues(Guid editorId, int dataTypeId = -1)
        {
            var propEd = PropertyEditorResolver.Current.GetById(editorId);
            if (propEd == null)
            {
                throw new InvalidOperationException("Could not find property editor with id " + editorId);
            }

            if (dataTypeId == -1)
            {
                //this is a new data type, so just return the field editors, there are no values yet
                return propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>);                
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
            if (dataType.ControlId == editorId)
            {
                //this is the currently assigned pre-value editor, return with values.
                return Mapper.Map<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>(dataType);
            }

            //return the pre value display without values
            return propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>);
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
            var dtService = (DataTypeService) ApplicationContext.Services.DataTypeService;

            //TODO: Check if the property editor has changed, if it has ensure we don't pass the 
            // existing values to the new property editor!

            //get the prevalues, current and new
            var preValDictionary = dataType.PreValues.ToDictionary(x => x.Key, x => x.Value);
            var currVal = ((DataTypeService) Services.DataTypeService).GetPreValuesCollectionByDataTypeId(dataType.PersistedDataType.Id);

            //we need to allow for the property editor to deserialize the prevalues
            var formattedVal = dataType.PropertyEditor.PreValueEditor.FormatDataForPersistence(
                preValDictionary,
                currVal);

            //create the pre-value collection to be saved
            var preVals = new PreValueCollection(formattedVal.ToDictionary(x => x.Key, x => x.Value));
            
            //save the data type
            dtService.SaveDataTypeAndPreValues(dataType.PersistedDataType, preVals, (int)Security.CurrentUser.Id);

            var display = Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(dataType.PersistedDataType);
            display.AddSuccessNotification(ui.Text("speechBubbles", "dataTypeSaved"), "");

            //now return the updated model
            return display;
        }
    }
}