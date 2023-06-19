// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a tags property editor.
/// </summary>
[TagsPropertyEditor]
[DataEditor(
    Constants.PropertyEditors.Aliases.Tags,
    "Tags",
    "tags",
    Icon = "icon-tags",
    ValueEditorIsReusable = true)]
public class TagsPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly ITagPropertyIndexValueFactory _tagPropertyIndexValueFactory;
    private readonly IIOHelper _ioHelper;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ManifestValueValidatorCollection _validators;

    // Scheduled for removal in v12
    [Obsolete("Use non-obsoleted ctor. This will be removed in Umbraco 13.")]
    public TagsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ManifestValueValidatorCollection validators,
        IIOHelper ioHelper,
        ILocalizedTextService localizedTextService)
        : this(
            dataValueEditorFactory,
            validators,
            ioHelper,
            localizedTextService,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>(),
            StaticServiceProvider.Instance.GetRequiredService<ITagPropertyIndexValueFactory>())
    {
    }

    [Obsolete("Use non-obsoleted ctor. This will be removed in Umbraco 13.")]
    public TagsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ManifestValueValidatorCollection validators,
        IIOHelper ioHelper,
        ILocalizedTextService localizedTextService,
        IEditorConfigurationParser editorConfigurationParser)
        : this(
            dataValueEditorFactory,
            validators,
            ioHelper,
            localizedTextService,
            editorConfigurationParser,
            StaticServiceProvider.Instance.GetRequiredService<ITagPropertyIndexValueFactory>())
    {

    }

    public TagsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ManifestValueValidatorCollection validators,
        IIOHelper ioHelper,
        ILocalizedTextService localizedTextService,
        IEditorConfigurationParser editorConfigurationParser,
        ITagPropertyIndexValueFactory tagPropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _validators = validators;
        _ioHelper = ioHelper;
        _localizedTextService = localizedTextService;
        _editorConfigurationParser = editorConfigurationParser;
        _tagPropertyIndexValueFactory = tagPropertyIndexValueFactory;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _tagPropertyIndexValueFactory;


    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<TagPropertyValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new TagConfigurationEditor(_validators, _ioHelper, _localizedTextService, _editorConfigurationParser);

    internal class TagPropertyValueEditor : DataValueEditor, IDataValueTags
    {
        private readonly IDataTypeService _dataTypeService;

        public TagPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IDataTypeService dataTypeService)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _dataTypeService = dataTypeService;
        }

        /// <inheritdoc />
        public IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
        {
            var strValue = value?.ToString();
            if (string.IsNullOrWhiteSpace(strValue)) return Enumerable.Empty<ITag>();

            var tagConfiguration = ConfigurationEditor.ConfigurationAs<TagConfiguration>(dataTypeConfiguration) ?? new TagConfiguration();

            if (tagConfiguration.Delimiter == default)
                tagConfiguration.Delimiter = ',';

            IEnumerable<string> tags;

            switch (tagConfiguration.StorageType)
            {
                case TagsStorageType.Csv:
                    tags = strValue.Split(new[] { tagConfiguration.Delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                    break;

                case TagsStorageType.Json:
                    try
                    {
                        tags = JsonConvert.DeserializeObject<string[]>(strValue)?.Select(x => x.Trim()) ?? Enumerable.Empty<string>();
                    }
                    catch (JsonException)
                    {
                        //cannot parse, malformed
                        tags = Enumerable.Empty<string>();
                    }

                    break;

                default:
                    throw new NotSupportedException($"Value \"{tagConfiguration.StorageType}\" is not a valid TagsStorageType.");
            }

            return tags.Select(x => new Tag
            {
                Group = tagConfiguration.Group,
                Text = x,
                LanguageId = languageId,
            });
        }


        /// <inheritdoc />
        public override IValueRequiredValidator RequiredValidator => new RequiredJsonValueValidator();

        /// <inheritdoc />
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            var value = editorValue.Value?.ToString();

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var tagConfiguration = editorValue.DataTypeConfiguration as TagConfiguration ?? new TagConfiguration();
            if (tagConfiguration.Delimiter == default)
                tagConfiguration.Delimiter = ',';

            string[] trimmedTags = Array.Empty<string>();

            if (editorValue.Value is JArray json)
            {
                trimmedTags = json.HasValues ? json.Select(x => x.Value<string>()).OfType<string>().ToArray() : Array.Empty<string>();
            }
            else if (string.IsNullOrWhiteSpace(value) == false)
            {
                trimmedTags = value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
            }

            if (trimmedTags.Length == 0)
            {
                return null;
            }

            switch (tagConfiguration.StorageType)
            {
                case TagsStorageType.Csv:
                    return string.Join(tagConfiguration.Delimiter.ToString(), trimmedTags).NullOrWhiteSpaceAsNull();

                case TagsStorageType.Json:
                    return trimmedTags.Length == 0 ? null : JsonConvert.SerializeObject(trimmedTags);
            }

            return null;
        }

        /// <summary>
        ///     Custom validator to validate a required value against an empty json value.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This validator is required because the default RequiredValidator uses ValueType to
        ///         determine whether a property value is JSON, and for tags the ValueType is string although
        ///         the underlying data is JSON. Yes, this makes little sense.
        ///     </para>
        /// </remarks>
        private class RequiredJsonValueValidator : IValueRequiredValidator
        {
            /// <inheritdoc />
            public IEnumerable<ValidationResult> ValidateRequired(object? value, string valueType)
            {
                if (value == null)
                {
                    yield return new ValidationResult("Value cannot be null", new[] { "value" });
                    yield break;
                }

                if (value.ToString()!.DetectIsEmptyJson())
                {
                    yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                }
            }
        }
    }
}
