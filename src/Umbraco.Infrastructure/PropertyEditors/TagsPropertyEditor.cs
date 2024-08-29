// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
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
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Text)]
public class TagsPropertyEditor : DataEditor
{
    private readonly ITagPropertyIndexValueFactory _tagPropertyIndexValueFactory;
    private readonly IIOHelper _ioHelper;

    public TagsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        ITagPropertyIndexValueFactory tagPropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _tagPropertyIndexValueFactory = tagPropertyIndexValueFactory;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _tagPropertyIndexValueFactory;


    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<TagPropertyValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new TagConfigurationEditor(_ioHelper);

    internal class TagPropertyValueEditor : DataValueEditor, IDataValueTags
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDataTypeService _dataTypeService;

        public TagPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IDataTypeService dataTypeService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            _dataTypeService = dataTypeService;
        }

        /// <inheritdoc />
        public IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
        {
            TagConfiguration tagConfiguration = ConfigurationEditor.ConfigurationAs<TagConfiguration>(dataTypeConfiguration) ?? new TagConfiguration();

            var tags = ParseTags(value, tagConfiguration);
            if (tags.Any() is false)
            {
                return Enumerable.Empty<ITag>();
            }

            return tags.Select(x => new Tag
            {
                Group = tagConfiguration.Group,
                Text = x,
                LanguageId = languageId,
            });
        }

        private string[] ParseTags(object? value, TagConfiguration tagConfiguration)
        {
            var strValue = value?.ToString();
            if (string.IsNullOrWhiteSpace(strValue))
            {
                return Array.Empty<string>();
            }

            if (tagConfiguration.Delimiter == default)
            {
                tagConfiguration.Delimiter = ',';
            }

            IEnumerable<string> tags;

            switch (tagConfiguration.StorageType)
            {
                case TagsStorageType.Csv:
                    tags = strValue.Split(new[] { tagConfiguration.Delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                    break;

                case TagsStorageType.Json:
                    try
                    {
                        tags = _jsonSerializer.Deserialize<string[]>(strValue)?.Select(x => x.Trim()) ?? Enumerable.Empty<string>();
                    }
                    catch
                    {
                        //cannot parse, malformed
                        tags = Enumerable.Empty<string>();
                    }

                    break;

                default:
                    throw new NotSupportedException($"Value \"{tagConfiguration.StorageType}\" is not a valid TagsStorageType.");
            }

            return tags.ToArray();
        }

        /// <inheritdoc />
        public override IValueRequiredValidator RequiredValidator => new RequiredJsonValueValidator();

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var val = property.GetValue(culture, segment);
            if (val is null)
            {
                return null;
            }

            IDataType? dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
            TagConfiguration configuration = dataType?.ConfigurationObject as TagConfiguration ?? new TagConfiguration();
            var tags = ParseTags(val, configuration);

            return tags.Any() ? tags : null;
        }

        /// <inheritdoc />
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is not IEnumerable<string> stringValues)
            {
                return null;
            }

            var trimmedTags = stringValues.Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() == false).ToArray();
            if (trimmedTags.Any() is false)
            {
                return null;
            }

            TagConfiguration tagConfiguration = editorValue.DataTypeConfiguration as TagConfiguration ?? new TagConfiguration();
            if (tagConfiguration.Delimiter == default)
            {
                tagConfiguration.Delimiter = ',';
            }

            switch (tagConfiguration.StorageType)
            {
                case TagsStorageType.Csv:
                    return string.Join(tagConfiguration.Delimiter.ToString(), trimmedTags).NullOrWhiteSpaceAsNull();

                case TagsStorageType.Json:
                    return trimmedTags.Length == 0 ? null : _jsonSerializer.Serialize(trimmedTags);
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
