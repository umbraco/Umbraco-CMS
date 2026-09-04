// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Member picker property editor that stores the keys of any number of members.
/// </summary>
/// <remarks>
///     <see cref="MemberPickerPropertyEditor" /> holds a single member. This editor is the one to reach for when
///     several are wanted, and together with the dedicated document, media and element pickers it replaces what the
///     multi node tree picker was used for.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultipleMemberPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MultipleMemberPickerPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleMemberPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public MultipleMemberPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(IEnumerable<Guid>);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration)
    {
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("array", "null"),
            ["items"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "uuid",
                ["pattern"] = ValueSchemaPatterns.Uuid,
            },
            ["description"] = "Keys of the selected members",
        };

        if (configuration is MultipleMemberPickerConfiguration { ValidationLimit: { } validationLimit }
            && validationLimit.Max is int max && max > 0)
        {
            schema["maxItems"] = max;
        }

        return schema;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultipleMemberPickerConfigurationEditor(_ioHelper);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleMemberPickerPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Provides the value editor for the multiple member picker property editor.
    /// </summary>
    internal sealed class MultipleMemberPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleMemberPickerPropertyValueEditor" /> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
        /// <param name="localizedTextService">The localized text service.</param>
        /// <param name="memberService">The member service.</param>
        /// <param name="coreScopeProvider">The core scope provider.</param>
        public MultipleMemberPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService,
            IMemberService memberService,
            ICoreScopeProvider coreScopeProvider)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            Validators.Add(new TypedValidatorRunner<List<string>, MemberPickerConfigurationBase>(
                new MinMaxValidator(localizedTextService),
                new MultipleMemberTypeFilterValidator(localizedTextService, memberService, coreScopeProvider)));
        }

        /// <inheritdoc/>
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            foreach (Guid key in Deserialize(_jsonSerializer, value))
            {
                yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Member, key));
            }
        }

        /// <summary>
        ///     Deserializes the provided value into the keys of the picked members.
        /// </summary>
        internal static IEnumerable<Guid> Deserialize(IJsonSerializer jsonSerializer, object? value)
        {
            var asString = value as string ?? value?.ToString();

            return string.IsNullOrWhiteSpace(asString)
                ? []
                : jsonSerializer.Deserialize<IEnumerable<Guid>>(asString) ?? [];
        }
    }

    /// <summary>
    /// Validates that the number of selected members is within the configured limits, if any.
    /// </summary>
    internal sealed class MinMaxValidator : ITypedValidator<List<string>, MemberPickerConfigurationBase>
    {
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MinMaxValidator" /> class.
        /// </summary>
        /// <param name="localizedTextService">The localized text service.</param>
        public MinMaxValidator(ILocalizedTextService localizedTextService)
            => _localizedTextService = localizedTextService;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(
            List<string>? value,
            MemberPickerConfigurationBase? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (configuration is not MultipleMemberPickerConfiguration { ValidationLimit: { } validationLimit })
            {
                return validationResults;
            }

            // A minimum applies only to a selection that is in use; whether an empty one is acceptable is decided by
            // the property's mandatory setting. Matches ItemCountValidationHelper.IsBelowMinimum.
            // TODO: call ItemCountValidationHelper once #23693 has merged up from v17.
            if (validationLimit.Min is int min and > 0 && value is { Count: > 0 } && value.Count < min)
            {
                validationResults.Add(new ValidationResult(
                    _localizedTextService.Localize(
                        "validation",
                        "entriesShort",
                        [min.ToString(), (min - value.Count).ToString()]),
                    ["value"]));
            }

            if (validationLimit.Max is int max and > 0 && value?.Count > max)
            {
                validationResults.Add(new ValidationResult(
                    _localizedTextService.Localize(
                        "validation",
                        "entriesExceed",
                        [max.ToString(), (value.Count - max).ToString()]),
                    ["value"]));
            }

            return validationResults;
        }
    }
}
