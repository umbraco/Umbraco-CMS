using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
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
        public IDataTypeService DataTypeService { get; }

        public PropertyEditorCollection PropertyEditors { get; }

        public DataTypeValidateAttribute()
            : this(Current.Factory.GetInstance<IDataTypeService>(), Current.Factory.GetInstance<PropertyEditorCollection>())
        {
        }

        /// <summary>
        /// For use in unit tests. Not possible to use as attribute ctor.
        /// </summary>
        /// <param name="dataTypeService"></param>
        /// <param name="propertyEditors"></param>
        public DataTypeValidateAttribute(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors)
        {
            DataTypeService = dataTypeService;
            PropertyEditors = propertyEditors;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var dataType = (DataTypeSave) actionContext.ActionArguments["dataType"];

            dataType.Name = dataType.Name.CleanForXss('[', ']', '(', ')', ':');
            dataType.Alias = dataType.Alias == null ? dataType.Name : dataType.Alias.CleanForXss('[', ']', '(', ')', ':');

            // get the property editor, ensuring that it exits
            if (!PropertyEditors.TryGet(dataType.EditorAlias, out var propertyEditor))
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
                    persisted = DataTypeService.GetDataType(Convert.ToInt32(dataType.Id));
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
            var configurationEditor = propertyEditor.GetConfigurationEditor();
            foreach (var field in dataType.ConfigurationFields)
            {
                var editorField = configurationEditor.Fields.SingleOrDefault(x => x.Key == field.Key);
                if (editorField == null) continue;

                // run each IValueValidator (with null valueType and dataTypeConfiguration: not relevant here) - fixme - editing
                foreach (var validator in editorField.Validators)
                foreach (var result in validator.Validate(field.Value, null, null))
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
