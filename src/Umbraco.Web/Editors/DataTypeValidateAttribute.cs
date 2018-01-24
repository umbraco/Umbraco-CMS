using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An action filter that wires up the persisted entity of the DataTypeSave model and validates the whole request
    /// </summary>
    internal sealed class DataTypeValidateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // injecting in attributes is not easy.
            // eventually, actionContext should give access to the service factory
            // but for the time being, have to rely on the global locator
            var dataTypeService = Current.Services.DataTypeService;

            var dataType = (DataTypeSave) actionContext.ActionArguments["dataType"];

            dataType.Name = dataType.Name.CleanForXss('[', ']', '(', ')', ':');
            dataType.Alias = dataType.Alias == null ? dataType.Name : dataType.Alias.CleanForXss('[', ']', '(', ')', ':');

            // get the property editor, ensuring that it exits
            if (!Current.PropertyEditors.TryGet(dataType.EditorAlias, out var propertyEditor))
            {
                var message = $"Property editor \"{dataType.EditorAlias}\" was not found.";
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                return;
            }

            // assign
            dataType.PropertyEditor = propertyEditor;

            // validate that the data type exists, or create one if required
            IDataType persisted;
            switch (dataType.Action)
            {
                case ContentSaveAction.Save:
                    persisted = dataTypeService.GetDataType(Convert.ToInt32(dataType.Id));
                    if (persisted == null)
                    {
                        var message = $"Data type with id {dataType.Id} was not found.";
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                        return;
                    }
                    // map the model to the persisted instance
                    Mapper.Map(dataType, persisted);
                    break;

                case ContentSaveAction.SaveNew:
                    // create the persisted model from mapping the saved model
                    persisted = Mapper.Map<IDataType>(dataType);
                    ((DataType) persisted).ResetIdentity();
                    break;

                default:
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, new ArgumentOutOfRangeException());
                    return;
            }

            // assign (so it's available in the action)
            dataType.PersistedDataType = persisted;

            // validate the configuration
            // which is posted as a set of fields with key (string) and value (object)
            foreach (var field in dataType.ConfigurationFields)
            {
                var value = field.Value;
                var editorField = propertyEditor.ConfigurationEditor.Fields.SingleOrDefault(x => x.Key == field.Key);
                if (editorField == null) continue;

                foreach (var validator in editorField.Validators)
                foreach (var result in validator.Validate(value, null, null))
                    actionContext.ModelState.AddValidationError(result, "Properties", field.Key);
            }

            if (actionContext.ModelState.IsValid == false)
            {
                // if it is not valid, do not continue and return the model state
                actionContext.Response = actionContext.Request.CreateValidationErrorResponse(actionContext.ModelState);
            }
        }
    }
}
