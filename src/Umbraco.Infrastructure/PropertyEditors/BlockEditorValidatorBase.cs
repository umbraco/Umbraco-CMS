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

    /// <summary>
    /// Gets the validation models for the content and settings of the blocks in a block editor value.
    /// </summary>
    /// <param name="blockEditorData">The block editor data to validate.</param>
    /// <param name="validationContext">The context of the property value being validated.</param>
    /// <returns>The validation models, one for each block item validated per culture and segment.</returns>
    /// <remarks>
    /// <para>
    /// Culture variant values are validated for the blocks exposed in their culture. Culture invariant values are
    /// validated for the blocks exposed in any culture, as they apply to every culture the blocks are exposed in.
    /// Settings are validated along with their content.
    /// </para>
    /// <para>
    /// Segments are not managed, so other than the segment in <paramref name="validationContext"/>, the segments
    /// validated are the default segment and the ones present among the block values.
    /// </para>
    /// <para>
    /// A mandatory property is only required to have an entry for the default segment. A missing entry for another
    /// segment is not validated, as that segment falls back to the default segment. An entry for a segment being
    /// validated is validated like any other, so an empty value fails a mandatory check.
    /// </para>
    /// </remarks>
    protected IEnumerable<ElementTypeValidationModel> GetBlockEditorDataValidation(BlockEditorData<TValue, TLayout> blockEditorData, PropertyValidationContext validationContext)
    {
        var elementTypeValidation = new List<ElementTypeValidationModel>();
        var isWildcardCulture = validationContext.Culture == "*";
        var validationContextCulture = isWildcardCulture ? null : validationContext.Culture.NullOrWhiteSpaceAsNull();

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

            // Include null for the invariant culture, so culture-invariant values are validated for each segment too.
            foreach (var culture in validationContextCulturesBeingValidated.Prepend<string?>(null))
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
            elementTypeValidation.AddRange(GetBlockEditorDataValidation(blockEditorData, validationContextCulture, validationContext.Segment));

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

                // in non-segmented validation paths, we need to include a null value for non-existing properties, so the
                // validation service has something to validate mandatory properties against.
                if (segment is null)
                {
                    var handledPropertyTypeAliases = elementValidation.PropertyTypeValidation.Select(v => v.PropertyType.Alias).ToList();
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

                        elementValidation.AddPropertyTypeValidation(
                            new PropertyTypeValidationModel(propertyType, null, $"{group.Path}[{i}].{valuesJsonPathPart}[{JsonPathExpression.MissingPropertyValue(propertyType.Alias, culture, null)}].value"));
                        handledPropertyTypeAliases.Add(propertyType.Alias);
                    }
                }

                yield return elementValidation;
            }
        }
    }
}
