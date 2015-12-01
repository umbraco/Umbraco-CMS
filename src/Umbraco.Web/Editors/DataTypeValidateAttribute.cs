using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An action filter that wires up the persisted entity of the DataTypeSave model and validates the whole request
    /// </summary>
    internal sealed class DataTypeValidateAttribute : ActionFilterAttribute
    {
        private readonly IDataTypeService _dataTypeService;

        public DataTypeValidateAttribute()
        {            
        }

        public DataTypeValidateAttribute(IDataTypeService dataTypeService)
        {
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _dataTypeService = dataTypeService;
        }

        private IDataTypeService DataTypeService
        {
            get { return _dataTypeService ?? ApplicationContext.Current.Services.DataTypeService; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var dataType = (DataTypeSave)actionContext.ActionArguments["dataType"];

            dataType.Name = dataType.Name.CleanForXss();
            dataType.Alias = dataType.Name.CleanForXss();

            //Validate that the property editor exists
            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(dataType.SelectedEditor);
            if (propertyEditor == null)
            {
                var message = string.Format("Property editor with id: {0} was not found", dataType.SelectedEditor);
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                return;
            }

            //assign the prop editor to the model
            dataType.PropertyEditor = propertyEditor;

            //Validate the data type exists or create one if required
            IDataTypeDefinition persisted;
            switch (dataType.Action)
            {
                case ContentSaveAction.Save:
                    persisted = DataTypeService.GetDataTypeDefinitionById(Convert.ToInt32(dataType.Id));
                    if (persisted == null)
                    {
                        var message = string.Format("Data type with id: {0} was not found", dataType.Id);
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                        return;
                    }
                    //map the model to the persisted instance
                    Mapper.Map(dataType, persisted);
                    break;
                case ContentSaveAction.SaveNew:
                    //create the persisted model from mapping the saved model
                    persisted = Mapper.Map<IDataTypeDefinition>(dataType);
                    ((DataTypeDefinition)persisted).ResetIdentity();
                    break;
                default:
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, new ArgumentOutOfRangeException());
                    return;
            }

            //now assign the persisted entity to the model so we can use it in the action
            dataType.PersistedDataType = persisted;

            //Validate each field
            foreach (var preVal in dataType.PreValues)
            {
                var postedValue = preVal.Value;
                
                foreach (var v in propertyEditor.PreValueEditor.Fields.Where(x => x.Key == preVal.Key).SelectMany(x => x.Validators))
                {
                    foreach (var result in v.Validate(postedValue, null, propertyEditor))
                    {
                        //if there are no member names supplied then we assume that the validation message is for the overall property
                        // not a sub field on the property editor
                        if (!result.MemberNames.Any())
                        {
                            //add a model state error for the entire property
                            actionContext.ModelState.AddModelError(string.Format("{0}.{1}", "Properties", preVal.Key), result.ErrorMessage);
                        }
                        else
                        {
                            //there's assigned field names so we'll combine the field name with the property name
                            // so that we can try to match it up to a real sub field of this editor
                            foreach (var field in result.MemberNames)
                            {
                                actionContext.ModelState.AddModelError(string.Format("{0}.{1}.{2}", "Properties", preVal.Key, field), result.ErrorMessage);
                            }
                        }
                    }
                }
            }

            if (actionContext.ModelState.IsValid == false)
            {
                //if it is not valid, do not continue and return the model state
                actionContext.Response = actionContext.Request.CreateValidationErrorResponse(actionContext.ModelState);
                return;
            }

        }
    }
}