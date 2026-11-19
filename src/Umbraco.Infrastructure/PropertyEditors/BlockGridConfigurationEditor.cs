// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockGridConfigurationEditor : ConfigurationEditor<BlockGridConfiguration>
{
    private static readonly RangeConfigurationValidator _rangeConfigurationValidator = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridConfigurationEditor"/> class with the specified IO helper.
    /// </summary>
    /// <param name="ioHelper">An implementation of <see cref="IIOHelper"/> used to assist with file and directory operations.</param>
    public BlockGridConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(IDictionary<string, object> configuration)
        => [.. base.Validate(configuration), .. ValidateAreaLimits(configuration)];

    // Area limits are nested inside the blocks configuration, so the configuration field validators don't reach them.
    private static IEnumerable<ValidationResult> ValidateAreaLimits(IDictionary<string, object> configuration)
    {
        if (configuration.TryGetValue("blocks", out var blocks) is false || ToJsonNode(blocks) is not JsonArray blockArray)
        {
            return [];
        }

        return blockArray
            .OfType<JsonObject>()
            .Select(block => GetValue(block, "areas"))
            .OfType<JsonArray>()
            .SelectMany(areas => areas.OfType<JsonObject>())
            .SelectMany(area => _rangeConfigurationValidator.Validate(GetValue(area, "validationLimit"), null, null, PropertyValidationContext.Empty()));
    }

    private static JsonNode? ToJsonNode(object? value)
        => value as JsonNode ?? JsonSerializer.SerializeToNode(value);

    private static JsonNode? GetValue(JsonObject jsonObject, string key)
        => jsonObject.FirstOrDefault(property => property.Key.InvariantEquals(key)).Value;
}
