using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockEditorValidatorBase : ComplexEditorValidator
{
    private readonly IContentTypeService _contentTypeService;

    protected BlockEditorValidatorBase(IPropertyValidationService propertyValidationService, IContentTypeService contentTypeService)
        : base(propertyValidationService)
        => _contentTypeService = contentTypeService;

    protected IEnumerable<ElementTypeValidationModel> GetBlockEditorDataValidation(BlockEditorData blockEditorData)
    {
        // There is no guarantee that the client will post data for every property defined in the Element Type but we still
        // need to validate that data for each property especially for things like 'required' data to work.
        // Lookup all element types for all content/settings and then we can populate any empty properties.
        var allElements = blockEditorData.BlockValue.ContentData.Concat(blockEditorData.BlockValue.SettingsData).ToList();
        var allElementTypes = _contentTypeService.GetAll(allElements.Select(x => x.ContentTypeKey).ToArray()).ToDictionary(x => x.Key);

        foreach (BlockItemData row in allElements)
        {
            if (!allElementTypes.TryGetValue(row.ContentTypeKey, out IContentType? elementType))
            {
                throw new InvalidOperationException($"No element type found with key {row.ContentTypeKey}");
            }

            // now ensure missing properties
            foreach (IPropertyType elementTypeProp in elementType.CompositionPropertyTypes)
            {
                if (!row.PropertyValues.ContainsKey(elementTypeProp.Alias))
                {
                    // set values to null
                    row.PropertyValues[elementTypeProp.Alias] = new BlockItemData.BlockPropertyValue(null, elementTypeProp);
                    row.RawPropertyValues[elementTypeProp.Alias] = null;
                }
            }

            var elementValidation = new ElementTypeValidationModel(row.ContentTypeAlias, row.Key);
            foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in row.PropertyValues)
            {
                elementValidation.AddPropertyTypeValidation(
                    new PropertyTypeValidationModel(prop.Value.PropertyType, prop.Value.Value));
            }

            yield return elementValidation;
        }
    }
}
