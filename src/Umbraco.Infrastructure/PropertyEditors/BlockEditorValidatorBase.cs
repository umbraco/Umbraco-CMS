using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a base class for implementing validation logic for block editor values and layouts in Umbraco.
/// </summary>
public abstract class BlockEditorValidatorBase<TValue, TLayout> : ComplexEditorValidator
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly IBlockEditorElementTypeCache _elementTypeCache;

    protected BlockEditorValidatorBase(IPropertyValidationService propertyValidationService, IBlockEditorElementTypeCache elementTypeCache)
        : base(propertyValidationService)
        => _elementTypeCache = elementTypeCache;

    protected IEnumerable<ElementTypeValidationModel> GetBlockEditorDataValidation(BlockEditorData<TValue, TLayout> blockEditorData, PropertyValidationContext validationContext)
    {
        var elementTypeValidation = new List<ElementTypeValidationModel>();
        var isWildcardCulture = validationContext.Culture == "*";
        var validationContextCulture = isWildcardCulture ? null : validationContext.Culture.NullOrWhiteSpaceAsNull();
        elementTypeValidation.AddRange(GetBlockEditorDataValidation(blockEditorData, validationContextCulture, validationContext.Segment));

        // We don't have managed segments, so we will simply trust the ones present in the model.
        var segmentsByCulture = blockEditorData
            .BlockValue.ContentData.SelectMany(cd => cd.Values)
            .Union(blockEditorData.BlockValue.SettingsData.SelectMany(cd => cd.Values))
            .GroupBy(v => v.Culture)
            .Select(g => new
            {
                Culture = g.Key,
                Segments = g.Select(v => v.Segment).WhereNotNull().Distinct().Union([null]).ToArray(),
            })
            .ToArray();

        if (validationContextCulture is null)
        {
            // make sure we extend validation to variant block value (element level variation)
            IEnumerable<string> validationContextCulturesBeingValidated = isWildcardCulture
                ? blockEditorData.BlockValue.Expose.Select(e => e.Culture).WhereNotNull().Distinct()
                : validationContext.CulturesBeingValidated;
            foreach (var culture in validationContextCulturesBeingValidated)
            {
                var segmentsToValidate = segmentsByCulture.FirstOrDefault(s => s.Culture.InvariantEquals(culture))?.Segments ?? [];
                foreach (var segment in segmentsToValidate.DefaultIfEmpty(null))
                {
                    elementTypeValidation.AddRange(GetBlockEditorDataValidation(blockEditorData, culture, segment));
                }
            }
        }
        else
        {
            // make sure we extend validation to invariant block values (no element level variation).
            // only the segments present among the invariant values apply here; segments found on another
            // culture's values are not overrides of the invariant ones.
            var segmentsToValidate = segmentsByCulture.FirstOrDefault(s => s.Culture is null)?.Segments ?? [];
            foreach (var segment in segmentsToValidate.DefaultIfEmpty(null))
            {
                elementTypeValidation.AddRange(GetBlockEditorDataValidation(blockEditorData, null, segment));
            }
        }

        return elementTypeValidation;
    }

    protected virtual string ContentDataGroupJsonPath =>
        nameof(BlockValue<TLayout>.ContentData).ToFirstLowerInvariant();

    protected virtual string SettingsDataGroupJsonPath =>
        nameof(BlockValue<TLayout>.SettingsData).ToFirstLowerInvariant();

    private IEnumerable<ElementTypeValidationModel> GetBlockEditorDataValidation(BlockEditorData<TValue, TLayout> blockEditorData, string? culture, string? segment)
    {
        // There is no guarantee that the client will post data for every property defined in the Element Type but we still
        // need to validate that data for each property especially for things like 'required' data to work.
        // Lookup all element types for all content/settings and then we can populate any empty properties.

        if (blockEditorData.Layout is null)
        {
            yield break;
        }

        Guid[] exposedContentKeys = blockEditorData.BlockValue.Expose
            .Where(expose => culture is null || expose.Culture == culture)
            .Select(expose => expose.ContentKey)
            .Distinct()
            .ToArray();
        Guid[] exposedSettingsKeys = blockEditorData.Layout
            .Where(layout => layout.SettingsKey.HasValue && exposedContentKeys.Contains(layout.ContentKey))
            .Select(layout => layout.SettingsKey!.Value)
            .ToArray();

        // The JSON paths must point at the position of an item in the submitted data, so the position is captured before the exposure filter is applied.
        var itemDataGroups = new[]
        {
            new { Path = ContentDataGroupJsonPath, Items = GetExposedItems(blockEditorData.BlockValue.ContentData, exposedContentKeys) },
            new { Path = SettingsDataGroupJsonPath, Items = GetExposedItems(blockEditorData.BlockValue.SettingsData, exposedSettingsKeys) }
        };

        var valuesJsonPathPart = nameof(BlockItemData.Values).ToFirstLowerInvariant();

        foreach (var group in itemDataGroups)
        {
            Guid[] elementTypeKeys = group.Items.Select(x => x.Item.ContentTypeKey).ToArray();
            if (elementTypeKeys.Length == 0)
            {
                continue;
            }

            var allElementTypes = _elementTypeCache.GetMany(elementTypeKeys).ToDictionary(x => x.Key);

            foreach ((BlockItemData item, int i) in group.Items)
            {
                if (!allElementTypes.TryGetValue(item.ContentTypeKey, out IContentType? elementType))
                {
                    throw new InvalidOperationException($"No element type found with key {item.ContentTypeKey}");
                }

                var elementValidation = new ElementTypeValidationModel(item.ContentTypeAlias, item.Key);
                for (var j = 0; j < item.Values.Count; j++)
                {
                    BlockPropertyValue blockPropertyValue = item.Values[j];
                    IPropertyType? propertyType = blockPropertyValue.PropertyType;
                    if (propertyType is null)
                    {
                        throw new ArgumentException("One or more block properties did not have a resolved property type. Block editor values must be resolved before attempting to validate them.", nameof(blockEditorData));
                    }

                    if (propertyType.VariesByCulture() != (culture is not null) || blockPropertyValue.Culture.InvariantEquals(culture) is false)
                    {
                        continue;
                    }

                    if (segment != "*")
                    {
                        if (propertyType.VariesBySegment() is false && segment is not null)
                        {
                            continue;
                        }

                        if (propertyType.VariesBySegment() && blockPropertyValue.Segment.InvariantEquals(segment) is false)
                        {
                            continue;
                        }
                    }

                    elementValidation.AddPropertyTypeValidation(
                        new PropertyTypeValidationModel(propertyType, blockPropertyValue.Value, $"{group.Path}[{i}].{valuesJsonPathPart}[{j}].value"));
                }

                var handledPropertyTypeAliases = elementValidation.PropertyTypeValidation.Select(v => v.PropertyType.Alias).ToArray();
                foreach (IPropertyType propertyType in elementType.CompositionPropertyTypes)
                {
                    if (handledPropertyTypeAliases.Contains(propertyType.Alias))
                    {
                        continue;
                    }

                    if (propertyType.VariesByCulture() != (culture is not null))
                    {
                        continue;
                    }

                    if (segment == "*" || propertyType.VariesBySegment() != (segment is not null))
                    {
                        continue;
                    }

                    elementValidation.AddPropertyTypeValidation(
                        new PropertyTypeValidationModel(propertyType, null, $"{group.Path}[{i}].{valuesJsonPathPart}[{JsonPathExpression.MissingPropertyValue(propertyType.Alias, culture, segment)}].value"));
                }

                yield return elementValidation;
            }
        }
    }

    private static (BlockItemData Item, int Index)[] GetExposedItems(IEnumerable<BlockItemData> items, Guid[] exposedKeys)
        => items
            .Select((item, index) => (Item: item, Index: index))
            .Where(x => exposedKeys.Contains(x.Item.Key))
            .ToArray();
}
