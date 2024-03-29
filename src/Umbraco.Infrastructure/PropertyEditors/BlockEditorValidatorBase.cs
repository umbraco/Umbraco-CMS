using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockEditorValidatorBase<TValue, TLayout> : ComplexEditorValidator
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IContentTypeService _contentTypeService;

    protected BlockEditorValidatorBase(IPropertyValidationService propertyValidationService, IContentTypeService contentTypeService)
        : base(propertyValidationService)
        => _contentTypeService = contentTypeService;

    protected IEnumerable<ElementTypeValidationModel> GetBlockEditorDataValidation(BlockEditorData<TValue, TLayout> blockEditorData)
    {
        // There is no guarantee that the client will post data for every property defined in the Element Type but we still
        // need to validate that data for each property especially for things like 'required' data to work.
        // Lookup all element types for all content/settings and then we can populate any empty properties.

        var itemDataGroups = new[]
        {
            new { Path = nameof(BlockValue<TLayout>.ContentData).ToFirstLowerInvariant(), Items = blockEditorData.BlockValue.ContentData },
            new { Path = nameof(BlockValue<TLayout>.SettingsData).ToFirstLowerInvariant(), Items = blockEditorData.BlockValue.SettingsData }
        };

        foreach (var group in itemDataGroups)
        {
            var allElementTypes = _contentTypeService.GetAll(group.Items.Select(x => x.ContentTypeKey).ToArray()).ToDictionary(x => x.Key);

            for (var i = 0; i < group.Items.Count; i++)
            {
                BlockItemData item = group.Items[i];
                if (!allElementTypes.TryGetValue(item.ContentTypeKey, out IContentType? elementType))
                {
                    throw new InvalidOperationException($"No element type found with key {item.ContentTypeKey}");
                }

                // now ensure missing properties
                foreach (IPropertyType elementTypeProp in elementType.CompositionPropertyTypes)
                {
                    if (!item.PropertyValues.ContainsKey(elementTypeProp.Alias))
                    {
                        // set values to null
                        item.PropertyValues[elementTypeProp.Alias] = new BlockItemData.BlockPropertyValue(null, elementTypeProp);
                        item.RawPropertyValues[elementTypeProp.Alias] = null;
                    }
                }

                var elementValidation = new ElementTypeValidationModel(item.ContentTypeAlias, item.Key);
                foreach (KeyValuePair<string, BlockItemData.BlockPropertyValue> prop in item.PropertyValues)
                {
                    elementValidation.AddPropertyTypeValidation(
                        new PropertyTypeValidationModel(prop.Value.PropertyType, prop.Value.Value, $"{group.Path}[{i}].{prop.Value.PropertyType.Alias}"));
                }

                yield return elementValidation;
            }
        }
    }
}
