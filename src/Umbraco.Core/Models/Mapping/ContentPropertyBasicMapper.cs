using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Creates a base generic ContentPropertyBasic from a Property
/// </summary>
internal class ContentPropertyBasicMapper<TDestination>
    where TDestination : ContentPropertyBasic, new()
{
    private readonly IEntityService _entityService;
    private readonly ILogger<ContentPropertyBasicMapper<TDestination>> _logger;
    private readonly PropertyEditorCollection _propertyEditors;

    public ContentPropertyBasicMapper(
        IDataTypeService dataTypeService,
        IEntityService entityService,
        ILogger<ContentPropertyBasicMapper<TDestination>> logger,
        PropertyEditorCollection propertyEditors)
    {
        _logger = logger;
        _propertyEditors = propertyEditors;
        DataTypeService = dataTypeService;
        _entityService = entityService;
    }

    protected IDataTypeService DataTypeService { get; }

    /// <summary>
    ///     Assigns the PropertyEditor, Id, Alias and Value to the property
    /// </summary>
    /// <returns></returns>
    public virtual void Map(IProperty property, TDestination dest, MapperContext context)
    {
        IDataEditor? editor = property.PropertyType is not null ? _propertyEditors[property.PropertyType.PropertyEditorAlias] : null;
        if (editor == null)
        {
            _logger.LogError(
                new NullReferenceException("The property editor with alias " +
                                           property.PropertyType?.PropertyEditorAlias + " does not exist"),
                "No property editor '{PropertyEditorAlias}' found, converting to a Label",
                property.PropertyType?.PropertyEditorAlias);

            editor = _propertyEditors[Constants.PropertyEditors.Aliases.Label];

            if (editor == null)
            {
                throw new InvalidOperationException(
                    $"Could not resolve the property editor {Constants.PropertyEditors.Aliases.Label}");
            }
        }

        dest.Id = property.Id;
        dest.Alias = property.Alias;
        dest.PropertyEditor = editor;
        dest.Editor = editor.Alias;
        dest.SupportsReadOnly = editor.SupportsReadOnly;
        dest.DataTypeKey = property.PropertyType!.DataTypeKey;

        // if there's a set of property aliases specified, we will check if the current property's value should be mapped.
        // if it isn't one of the ones specified in 'includeProperties', we will just return the result without mapping the Value.
        var includedProperties = context.GetIncludedProperties();
        if (includedProperties != null && !includedProperties.Contains(property.Alias))
        {
            return;
        }

        // Get the culture from the context which will be set during the mapping operation for each property
        var culture = context.GetCulture();

        // a culture needs to be in the context for a property type that can vary
        if (culture == null && property.PropertyType.VariesByCulture())
        {
            throw new InvalidOperationException(
                $"No culture found in mapping operation when one is required for the culture variant property type {property.PropertyType.Alias}");
        }

        // set the culture to null if it's an invariant property type
        culture = !property.PropertyType.VariesByCulture() ? null : culture;

        dest.Culture = culture;

        // Get the segment, which is always allowed to be null even if the propertyType *can* be varied by segment.
        // There is therefore no need to perform the null check like with culture above.
        var segment = !property.PropertyType.VariesBySegment() ? null : context.GetSegment();
        dest.Segment = segment;

        // if no 'IncludeProperties' were specified or this property is set to be included - we will map the value and return.
        dest.Value = editor.GetValueEditor().ToEditor(property, culture, segment);
    }
}
