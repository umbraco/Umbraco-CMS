using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
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

        /// <summary>
        /// Returns the pre-values for the specified property editor
        /// </summary>
        /// <param name="editorId"></param>
        /// <returns></returns>
        public IEnumerable<PreValueFieldDisplay> GetPreValues(Guid editorId)
        {
            var propEd = PropertyEditorResolver.Current.GetById(editorId);
            if (propEd == null)
            {
                throw new InvalidOperationException("Could not find property editor with id " + editorId);
            }

            return propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>);
        }

        //TODO: Generally there probably won't be file uploads for pre-values but we should allow them just like we do for the content editor

        /// <summary>
        /// Saves the data type
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public DataTypeDisplay PostSave(DataTypeSave dataType)
        {
            //Validate that the property editor exists
            var propertyEditor = PropertyEditorResolver.Current.GetById(dataType.SelectedEditor);
            if (propertyEditor == null)
            {
                var message = string.Format("Property editor with id: {0} was not found", dataType.SelectedEditor);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
            }

            //Validate the data type exists or create one if required
            IDataTypeDefinition persisted;
            switch (dataType.Action)
            {
                case ContentSaveAction.Save:
                    persisted = ApplicationContext.Services.DataTypeService.GetDataTypeDefinitionById(dataType.Id);
                    if (persisted == null)
                    {
                        var message = string.Format("Data type with id: {0} was not found", dataType.Id);
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                    }
                    //map the model to the persisted instance
                    Mapper.Map(dataType, persisted);
                    break;
                case ContentSaveAction.SaveNew:
                    //create the persisted model from mapping the saved model
                    persisted = Mapper.Map<IDataTypeDefinition>(dataType);
                    break;
                default:
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //Validate each field
            foreach (var preVal in dataType.PreValues)
            {
                var postedValue = preVal.Value;

                foreach (var v in propertyEditor.PreValueEditor.Fields.SelectMany(x => x.Validators))
                {
                    foreach (var result in v.Validate(postedValue, preVal.Key, propertyEditor))
                    {
                        //if there are no member names supplied then we assume that the validation message is for the overall property
                        // not a sub field on the property editor
                        if (!result.MemberNames.Any())
                        {
                            //add a model state error for the entire property
                            ModelState.AddModelError(string.Format("{0}.{1}", "Properties", preVal.Key), result.ErrorMessage);
                        }
                        else
                        {
                            //there's assigned field names so we'll combine the field name with the property name
                            // so that we can try to match it up to a real sub field of this editor
                            foreach (var field in result.MemberNames)
                            {
                                ModelState.AddModelError(string.Format("{0}.{1}.{2}", "Properties", preVal.Key, field), result.ErrorMessage);
                            }
                        }
                    }
                }
            }            

            if (ModelState.IsValid == false)
            {
                //if it is not valid, do not continue and return the model state
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, ModelState));
            }

            //finally we need to save the data type and it's pre-vals
            var dtService = (DataTypeService) ApplicationContext.Services.DataTypeService;
            var preVals = Mapper.Map<PreValueCollection>(dataType.PreValues);
            dtService.SaveDataTypeAndPreValues(persisted, preVals, (int)Security.CurrentUser.Id);

            var display = Mapper.Map<IDataTypeDefinition, DataTypeDisplay>(persisted);
            display.AddSuccessNotification(ui.Text("speechBubbles", "dataTypeSaved"), "");

            //now return the updated model
            return display;
        }
    }
}