// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
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
///     Content property editor that stores UDI.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ContentPicker,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class ContentPickerPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public ContentPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(string);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("string", "null"),
        ["format"] = "uuid",
        ["pattern"] = ValueSchemaPatterns.Uuid,
        ["description"] = "GUID of the selected document",
    };

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ContentPickerConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ContentPickerPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Provides the value editor for the content picker property editor.
    /// </summary>
    internal sealed class ContentPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentPickerPropertyValueEditor" /> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
        /// <param name="coreScopeProvider">The core scope provider.</param>
        /// <param name="contentService">The content service.</param>
        /// <param name="localizedTextService">The localized text service.</param>
        public ContentPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ICoreScopeProvider coreScopeProvider,
            IContentService contentService,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            Validators.Add(new TypedValidatorRunner<string, ContentPickerConfiguration>(
                new AllowedTypeValidator(localizedTextService, contentService, coreScopeProvider)));
        }

        /// <inheritdoc />
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
            => EntityPickerValueParser.ParseKeys(value, Constants.UdiEntityType.Document)
                .Select(key => new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Document, key)));

        /// <inheritdoc />
        /// <remarks>
        /// Starting in v14, the passed in value is always a GUID. The value is stored as a document UDI string.
        /// </remarks>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue) =>
            editorValue.Value is not null
            && Guid.TryParse(editorValue.Value.ToString(), out Guid guidValue)
                ? GuidUdi.Create(Constants.UdiEntityType.Document, guidValue).ToString()
                : null;

        /// <inheritdoc />
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var stringValue = property.GetValue(culture, segment)?.ToString();

            if (stringValue.IsNullOrWhiteSpace())
            {
                return null;
            }

            // this string can actually be an Int value from old versions => convert to it's guid counterpart
            if (int.TryParse(stringValue, out var oldInt))
            {
                // todo: This is a temporary code path that should be removed ASAP
                Attempt<Guid> conversionAttempt = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>()
                    .GetKeyForId(oldInt, UmbracoObjectTypes.Document);
                return conversionAttempt.Success ? conversionAttempt.Result : null;
            }

            // A value holding several documents presents the first, as this editor holds one.
            return EntityPickerValueParser.ParseKeys(stringValue, Constants.UdiEntityType.Document)
                .Cast<Guid?>()
                .FirstOrDefault();
        }
    }

    /// <summary>
    ///    Validates that the selected content matches the allowed content types configured for the property editor.
    /// </summary>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    internal sealed class AllowedTypeValidator(ILocalizedTextService localizedTextService, IContentService contentService, ICoreScopeProvider coreScopeProvider)
        : ITypedValidator<string, ContentPickerConfiguration>
    {
        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(
            string? value,
            ContentPickerConfiguration? configuration,
            string? valueType,
            PropertyValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(value) ||
                configuration is null ||
                Guid.TryParse(value, out Guid id) is false)
            {
                return [];
            }

            HashSet<Guid> allowedContentTypeKeys = AllowedContentTypeKeysParser.Parse(configuration.AllowedContentTypeIds);

            // No filter configured — all content types are allowed.
            if (allowedContentTypeKeys.Count == 0)
            {
                return [];
            }

            using ICoreScope scope = coreScopeProvider.CreateCoreScope();
            Guid? key = contentService.GetById(id)?.ContentType?.Key;
            scope.Complete();

            if (key is null)
            {
                return [new ValidationResult(
                            localizedTextService.Localize(
                                "validation",
                                "missingContent"),
                            ["value"])];
            }

            if (allowedContentTypeKeys.Contains(key.Value) is false)
            {
                return [new ValidationResult(
                            localizedTextService.Localize(
                                "validation",
                                "invalidObjectType"),
                            ["value"])];
            }

            return [];
        }
    }
}
