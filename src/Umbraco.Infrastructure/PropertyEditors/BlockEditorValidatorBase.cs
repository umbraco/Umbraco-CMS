using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

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

        if (validationContextCulture is null)
        {
            // make sure we extend validation to variant block value (element level variation)
            IEnumerable<string> validationContextCulturesBeingValidated = isWildcardCulture
                ? blockEditorData.BlockValue.Expose.Select(e => e.Culture).WhereNotNull().Distinct()
                : validationContext.CulturesBeingValidated;
            foreach (var culture in validationContextCulturesBeingValidated)
            {
                foreach (var segment in validationContext.SegmentsBeingValidated.DefaultIfEmpty(null))
                {
                    elementTypeValidation.AddRange(GetBlockEditorDataValidation(blockEditorData, culture, segment));
                }
            }
        }
        else
        {
            // make sure we extend validation to invariant block values (no element level variation)
            foreach (var segment in validationContext.SegmentsBeingValidated.DefaultIfEmpty(null))
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

        var itemDataGroups = new[]
        {
            new { Path = ContentDataGroupJsonPath, Items = blockEditorData.BlockValue.ContentData.Where(cd => exposedContentKeys.Contains(cd.Key)).ToArray() },
            new { Path = SettingsDataGroupJsonPath, Items = blockEditorData.BlockValue.SettingsData.Where(sd => exposedSettingsKeys.Contains(sd.Key)).ToArray() }
        };

        var valuesJsonPathPart = nameof(BlockItemData.Values).ToFirstLowerInvariant();

        foreach (var group in itemDataGroups)
        {
            Guid[] elementTypeKeys = group.Items.Select(x => x.ContentTypeKey).ToArray();
            if (elementTypeKeys.Length == 0)
            {
                continue;
            }

            var allElementTypes = _elementTypeCache.GetMany(elementTypeKeys).ToDictionary(x => x.Key);

            for (var i = 0; i < group.Items.Length; i++)
            {
                BlockItemData item = group.Items[i];
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
                        if (propertyType.VariesBySegment() != (segment is not null) || blockPropertyValue.Segment.InvariantEquals(segment) is false)
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
}
