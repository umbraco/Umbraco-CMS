// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Document picker property editor that stores the keys of any number of documents.
/// </summary>
/// <remarks>
///     <see cref="ContentPickerPropertyEditor" /> holds a single document. This editor is the one to reach for when
///     several are wanted, and together with the dedicated media, element and member pickers it replaces what the
///     multi node tree picker was used for.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultipleDocumentPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MultipleDocumentPickerPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleDocumentPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public MultipleDocumentPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
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
            ["description"] = "Keys of the selected documents",
        };

        if (configuration is MultipleDocumentPickerConfiguration { ValidationLimit: { } validationLimit })
        {
            if (validationLimit.Min is int min && min > 0)
            {
                schema["minItems"] = min;
            }

            if (validationLimit.Max is int max && max > 0)
            {
                schema["maxItems"] = max;
            }
        }

        return schema;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultipleDocumentPickerConfigurationEditor(_ioHelper);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleDocumentPickerPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Provides the value editor for the multiple document picker property editor.
    /// </summary>
    internal sealed class MultipleDocumentPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleDocumentPickerPropertyValueEditor" /> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
        /// <param name="localizedTextService">The localized text service.</param>
        /// <param name="contentService">The content service.</param>
        /// <param name="coreScopeProvider">The core scope provider.</param>
        public MultipleDocumentPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService,
            IContentService contentService,
            ICoreScopeProvider coreScopeProvider)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            Validators.Add(new TypedValidatorRunner<List<string>, MultipleDocumentPickerConfiguration>(
                new MinMaxValidator(localizedTextService),
                new AllowedTypeValidator(localizedTextService, contentService, coreScopeProvider)));
        }

        /// <inheritdoc/>
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            foreach (Guid key in Deserialize(_jsonSerializer, value))
            {
                yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Document, key));
            }
        }

        /// <summary>
        ///     Deserializes the provided value into the keys of the picked documents.
        /// </summary>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="value">The stored value.</param>
        /// <returns>The keys of the picked documents.</returns>
        internal static IEnumerable<Guid> Deserialize(IJsonSerializer jsonSerializer, object? value)
        {
            var asString = value as string ?? value?.ToString();

            return string.IsNullOrWhiteSpace(asString)
                ? []
                : jsonSerializer.Deserialize<IEnumerable<Guid>>(asString) ?? [];
        }
    }

    /// <summary>
    /// Validates that the number of selected documents is within the configured limits, if any.
    /// </summary>
    internal sealed class MinMaxValidator : ITypedValidator<List<string>, MultipleDocumentPickerConfiguration>
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
            MultipleDocumentPickerConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (configuration?.ValidationLimit is null)
            {
                return validationResults;
            }

            // A minimum applies only to a selection that is in use; whether an empty one is acceptable is decided by
            // the property's mandatory setting. Matches ItemCountValidationHelper.IsBelowMinimum.
            // TODO: call ItemCountValidationHelper once #23693 has merged up from v17.
            if (configuration.ValidationLimit.Min is int min and > 0 && value is { Count: > 0 } && value.Count < min)
            {
                validationResults.Add(new ValidationResult(
                    _localizedTextService.Localize(
                        "validation",
                        "entriesShort",
                        [min.ToString(), (min - value.Count).ToString()]),
                    ["value"]));
            }

            if (configuration.ValidationLimit.Max is int max and > 0 && value?.Count > max)
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

    /// <summary>
    /// Validates that all selected documents are of an allowed document type, if any are configured.
    /// </summary>
    internal sealed class AllowedTypeValidator : ITypedValidator<List<string>, MultipleDocumentPickerConfiguration>
    {
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IContentService _contentService;
        private readonly ICoreScopeProvider _coreScopeProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AllowedTypeValidator" /> class.
        /// </summary>
        /// <param name="localizedTextService">The localized text service.</param>
        /// <param name="contentService">The content service.</param>
        /// <param name="coreScopeProvider">The core scope provider.</param>
        public AllowedTypeValidator(
            ILocalizedTextService localizedTextService,
            IContentService contentService,
            ICoreScopeProvider coreScopeProvider)
        {
            _localizedTextService = localizedTextService;
            _contentService = contentService;
            _coreScopeProvider = coreScopeProvider;
        }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(
            List<string>? value,
            MultipleDocumentPickerConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            if (value is null || value.Count == 0 || configuration is null)
            {
                return [];
            }

            HashSet<Guid> allowedContentTypeKeys = AllowedContentTypeKeysParser.Parse(configuration.AllowedContentTypeIds);

            // No filter configured — all document types are allowed.
            if (allowedContentTypeKeys.Count == 0)
            {
                return [];
            }

            Guid[] keys = value
                .Where(v => Guid.TryParse(v, out _))
                .Select(Guid.Parse)
                .Distinct()
                .ToArray();

            using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            IContent[] documents = _contentService.GetByIds(keys).ToArray();
            scope.Complete();

            if (documents.Length != keys.Length)
            {
                return
                [
                    new ValidationResult(
                        _localizedTextService.Localize("validation", "missingContent"),
                        ["value"])
                ];
            }

            if (documents.Any(document => allowedContentTypeKeys.Contains(document.ContentType.Key) is false))
            {
                return
                [
                    new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidObjectType"),
                        ["value"])
                ];
            }

            return [];
        }
    }
}
