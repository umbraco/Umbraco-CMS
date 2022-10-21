// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
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
    private readonly IIOHelper _ioHelper;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ManifestValueValidatorCollection _validators;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
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
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public TagsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ManifestValueValidatorCollection validators,
        IIOHelper ioHelper,
        ILocalizedTextService localizedTextService,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _validators = validators;
        _ioHelper = ioHelper;
        _localizedTextService = localizedTextService;
        _editorConfigurationParser = editorConfigurationParser;
    }

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<TagPropertyValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new TagConfigurationEditor(_validators, _ioHelper, _localizedTextService, _editorConfigurationParser);

    internal class TagPropertyValueEditor : DataValueEditor
    {
        public TagPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
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

            if (editorValue.Value is JArray json)
            {
                return json.HasValues ? json.Select(x => x.Value<string>()) : null;
            }

            if (string.IsNullOrWhiteSpace(value) == false)
            {
                return value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
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
