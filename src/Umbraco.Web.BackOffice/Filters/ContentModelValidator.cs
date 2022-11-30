using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     A base class purely used for logging without generics
/// </summary>
internal abstract class ContentModelValidator
{
    protected ContentModelValidator(ILogger<ContentModelValidator> logger, IPropertyValidationService propertyValidationService)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        PropertyValidationService = propertyValidationService ??
                                    throw new ArgumentNullException(nameof(propertyValidationService));
    }

    public IPropertyValidationService PropertyValidationService { get; }
    protected ILogger<ContentModelValidator> Logger { get; }
}

/// <summary>
///     A validation helper class used with ContentItemValidationFilterAttribute to be shared between content, media,
///     etc...
/// </summary>
/// <typeparam name="TPersisted"></typeparam>
/// <typeparam name="TModelSave"></typeparam>
/// <typeparam name="TModelWithProperties"></typeparam>
/// <remarks>
///     If any severe errors occur then the response gets set to an error and execution will not continue. Property
///     validation
///     errors will just be added to the ModelState.
/// </remarks>
internal abstract class ContentModelValidator<TPersisted, TModelSave, TModelWithProperties> : ContentModelValidator
    where TPersisted : class, IContentBase
    where TModelSave : IContentSave<TPersisted>
    where TModelWithProperties : IContentProperties<ContentPropertyBasic>
{
    protected ContentModelValidator(
        ILogger<ContentModelValidator> logger,
        IPropertyValidationService propertyValidationService)
        : base(logger, propertyValidationService)
    {
    }

    /// <summary>
    ///     Ensure the content exists
    /// </summary>
    /// <param name="postedItem"></param>
    /// <param name="actionContext"></param>
    /// <returns></returns>
    public virtual bool ValidateExistingContent(TModelSave? postedItem, ActionExecutingContext actionContext)
    {
        TPersisted? persistedContent = postedItem?.PersistedContent;
        if (persistedContent == null)
        {
            actionContext.Result = new NotFoundObjectResult("content was not found");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Ensure all of the ids in the post are valid
    /// </summary>
    /// <param name="model"></param>
    /// <param name="modelWithProperties"></param>
    /// <param name="actionContext"></param>
    /// <returns></returns>
    public virtual bool ValidateProperties(TModelSave? model, IContentProperties<ContentPropertyBasic>? modelWithProperties, ActionExecutingContext actionContext)
    {
        TPersisted? persistedContent = model?.PersistedContent;
        return ValidateProperties(modelWithProperties?.Properties.ToList() ?? new List<ContentPropertyBasic>(), persistedContent?.Properties.ToList(), actionContext);
    }

    /// <summary>
    ///     This validates that all of the posted properties exist on the persisted entity
    /// </summary>
    /// <param name="postedProperties"></param>
    /// <param name="persistedProperties"></param>
    /// <param name="actionContext"></param>
    /// <returns></returns>
    protected bool ValidateProperties(List<ContentPropertyBasic>? postedProperties, List<IProperty>? persistedProperties, ActionExecutingContext actionContext)
    {
        if (postedProperties is null)
        {
            return false;
        }

        foreach (ContentPropertyBasic p in postedProperties)
        {
            if (persistedProperties?.Any(property => property.Alias == p.Alias) == false)
            {
                // TODO: Do we return errors here ? If someone deletes a property whilst their editing then should we just
                //save the property data that remains? Or inform them they need to reload... not sure. This problem exists currently too i think.

                var message = $"property with alias: {p.Alias} was not found";
                actionContext.Result = new NotFoundObjectResult(new InvalidOperationException(message));
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Validates the data for each property
    /// </summary>
    /// <param name="model"></param>
    /// <param name="modelWithProperties"></param>
    /// <param name="dto"></param>
    /// <param name="modelState"></param>
    /// <returns></returns>
    /// <remarks>
    ///     All property data validation goes into the model state with a prefix of "Properties"
    /// </remarks>
    public virtual bool ValidatePropertiesData(
        TModelSave? model,
        TModelWithProperties? modelWithProperties,
        ContentPropertyCollectionDto? dto,
        ModelStateDictionary modelState)
    {
        var properties = modelWithProperties?.Properties.ToDictionary(x => x.Alias, x => x);

        if (dto is not null)
        {
            foreach (ContentPropertyDto p in dto.Properties)
            {
                IDataEditor? editor = p.PropertyEditor;

                if (editor == null)
                {
                    var message =
                        $"Could not find property editor \"{p.DataType?.EditorAlias}\" for property with id {p.Id}.";

                    Logger.LogWarning(message);
                    continue;
                }

                //get the posted value for this property, this may be null in cases where the property was marked as readonly which means
                //the angular app will not post that value.
                if (properties is null || !properties.TryGetValue(p.Alias, out ContentPropertyBasic? postedProp))
                {
                    continue;
                }

                var postedValue = postedProp.Value;

                ValidatePropertyValue(model, modelWithProperties, editor, p, postedValue, modelState);
            }
        }


        return modelState.IsValid;
    }

    /// <summary>
    ///     Validates a property's value and adds the error to model state if found
    /// </summary>
    /// <param name="model"></param>
    /// <param name="modelWithProperties"></param>
    /// <param name="editor"></param>
    /// <param name="property"></param>
    /// <param name="postedValue"></param>
    /// <param name="modelState"></param>
    protected virtual void ValidatePropertyValue(
        TModelSave? model,
        TModelWithProperties? modelWithProperties,
        IDataEditor editor,
        ContentPropertyDto property,
        object? postedValue,
        ModelStateDictionary modelState)
    {
        if (property is null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        if (property.DataType is null)
        {
            throw new InvalidOperationException($"{nameof(property)}.{nameof(property.DataType)} cannot be null");
        }

        foreach (ValidationResult validationResult in PropertyValidationService.ValidatePropertyValue(
                     editor,
                     property.DataType,
                     postedValue,
                     property.IsRequired ?? false,
                     property.ValidationRegExp,
                     property.IsRequiredMessage,
                     property.ValidationRegExpMessage))
        {
            AddPropertyError(model, modelWithProperties, editor, property, validationResult, modelState);
        }
    }

    protected virtual void AddPropertyError(
        TModelSave? model,
        TModelWithProperties? modelWithProperties,
        IDataEditor editor,
        ContentPropertyDto property,
        ValidationResult validationResult,
        ModelStateDictionary modelState) =>
        modelState.AddPropertyError(validationResult, property.Alias, property.Culture ?? string.Empty, property.Segment ?? string.Empty);
}
