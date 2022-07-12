using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Extensions;
using DataType = Umbraco.Cms.Core.Models.DataType;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     An attribute/filter that wires up the persisted entity of the DataTypeSave model and validates the whole request
/// </summary>
internal sealed class DataTypeValidateAttribute : TypeFilterAttribute
{
    public DataTypeValidateAttribute() : base(typeof(DataTypeValidateFilter))
    {
    }

    private class DataTypeValidateFilter : IActionFilter
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditorCollection;
        private readonly IUmbracoMapper _umbracoMapper;

        public DataTypeValidateFilter(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditorCollection, IUmbracoMapper umbracoMapper)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
            _propertyEditorCollection = propertyEditorCollection ??
                                        throw new ArgumentNullException(nameof(propertyEditorCollection));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var dataType = (DataTypeSave?)context.ActionArguments["dataType"];
            if (dataType is not null)
            {
                dataType.Name = dataType.Name?.CleanForXss('[', ']', '(', ')', ':');
                dataType.Alias = dataType.Alias == null
                    ? dataType.Name!
                    : dataType.Alias.CleanForXss('[', ']', '(', ')', ':');
            }

            // get the property editor, ensuring that it exits
            if (!_propertyEditorCollection.TryGet(dataType?.EditorAlias, out IDataEditor? propertyEditor))
            {
                var message = $"Property editor \"{dataType?.EditorAlias}\" was not found.";
                context.Result = new UmbracoProblemResult(message, HttpStatusCode.NotFound);
                return;
            }

            if (dataType is null)
            {
                return;
            }

            // assign
            dataType.PropertyEditor = propertyEditor;

            // validate that the data type exists, or create one if required
            IDataType? persisted;
            switch (dataType.Action)
            {
                case ContentSaveAction.Save:
                    persisted = _dataTypeService.GetDataType(Convert.ToInt32(dataType.Id));
                    if (persisted == null)
                    {
                        var message = $"Data type with id {dataType.Id} was not found.";
                        context.Result = new UmbracoProblemResult(message, HttpStatusCode.NotFound);
                        return;
                    }

                    // map the model to the persisted instance
                    _umbracoMapper.Map(dataType, persisted);
                    break;

                case ContentSaveAction.SaveNew:
                    // create the persisted model from mapping the saved model
                    persisted = _umbracoMapper.Map<IDataType>(dataType);
                    ((DataType?)persisted)?.ResetIdentity();
                    break;

                default:
                    context.Result = new UmbracoProblemResult($"Data type action {dataType.Action} was not found.", HttpStatusCode.NotFound);
                    return;
            }

            // assign (so it's available in the action)
            dataType.PersistedDataType = persisted;

            // validate the configuration
            // which is posted as a set of fields with key (string) and value (object)
            IConfigurationEditor configurationEditor = propertyEditor.GetConfigurationEditor();

            if (dataType.ConfigurationFields is not null)
            {
                foreach (DataTypeConfigurationFieldSave field in dataType.ConfigurationFields)
                {
                    ConfigurationField? editorField =
                        configurationEditor.Fields.SingleOrDefault(x => x.Key == field.Key);
                    if (editorField == null)
                    {
                        continue;
                    }

                    // run each IValueValidator (with null valueType and dataTypeConfiguration: not relevant here)
                    foreach (IValueValidator validator in editorField.Validators)
                    {
                        foreach (ValidationResult result in validator.Validate(field.Value, null, null))
                        {
                            context.ModelState.AddValidationError(result, "Properties", field.Key ?? string.Empty);
                        }
                    }
                }
            }

            if (context.ModelState.IsValid == false)
            {
                // if it is not valid, do not continue and return the model state
                context.Result = new ValidationErrorResult(context.ModelState);
            }
        }
    }
}
