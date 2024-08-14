using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class BlockEditorValidatorBase<TValue, TLayout> : ComplexEditorValidator
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IBlockEditorElementTypeCache _elementTypeCache;

    protected BlockEditorValidatorBase(IPropertyValidationService propertyValidationService, IBlockEditorElementTypeCache elementTypeCache)
        : base(propertyValidationService)
        => _elementTypeCache = elementTypeCache;

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

        var propertiesJsonPathPart = nameof(BlockItemData.Properties).ToFirstLowerInvariant();

        foreach (var group in itemDataGroups)
        {
            var allElementTypes = _elementTypeCache.GetAll(group.Items.Select(x => x.ContentTypeKey).ToArray()).ToDictionary(x => x.Key);

            for (var i = 0; i < group.Items.Count; i++)
            {
                BlockItemData item = group.Items[i];
                if (!allElementTypes.TryGetValue(item.ContentTypeKey, out IContentType? elementType))
                {
                    throw new InvalidOperationException($"No element type found with key {item.ContentTypeKey}");
                }

                // NOTE: for now this only validates the property data actually sent by the client, not all element properties.
                //       we need to ensure that all properties for all languages have a matching "item" entry here, to handle validation of
                //       required properties (see comment in the top of this method). a separate task has been created, get in touch with KJA.
                var elementValidation = new ElementTypeValidationModel(item.ContentTypeAlias, item.Key);
                for (var j = 0; j < item.Properties.Count; j++)
                {
                    BlockPropertyValue blockPropertyValue = item.Properties[j];
                    IPropertyType? propertyType = blockPropertyValue.PropertyType;
                    if (propertyType is null)
                    {
                        throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to validate them.", nameof(blockEditorData));
                    }

                    elementValidation.AddPropertyTypeValidation(
                        new PropertyTypeValidationModel(propertyType, blockPropertyValue.Value, $"{group.Path}[{i}].{propertiesJsonPathPart}[{j}]"));
                }

                yield return elementValidation;
            }
        }
    }
}
