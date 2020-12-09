using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a tags property editor.
    /// </summary>
    [TagsPropertyEditor]
    [DataEditor(
        Constants.PropertyEditors.Aliases.Tags,
        "Tags",
        "tags",
        Icon = "icon-tags")]
    public class TagsPropertyEditor : DataEditor
    {
        private readonly ManifestValueValidatorCollection _validators;
        private readonly IIOHelper _ioHelper;

        public TagsPropertyEditor(
            ManifestValueValidatorCollection validators,
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _validators = validators;
            _ioHelper = ioHelper;
        }

        protected override IDataValueEditor CreateValueEditor() => new TagPropertyValueEditor(DataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper, JsonSerializer, Attribute);

        protected override IConfigurationEditor CreateConfigurationEditor() => new TagConfigurationEditor(_validators, _ioHelper, LocalizedTextService);

        internal class TagPropertyValueEditor : DataValueEditor
        {
            public TagPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper, IJsonSerializer jsonSerializer, DataEditorAttribute attribute)
                : base(dataTypeService, localizationService,localizedTextService, shortStringHelper, jsonSerializer, attribute)
            { }

            /// <inheritdoc />
            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                var value = editorValue?.Value?.ToString();

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                if (editorValue.Value is JArray json)
                {
                    return json.Select(x => x.Value<string>());
                }

                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    return value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }

                return null;
            }

            /// <inheritdoc />
            public override IValueRequiredValidator RequiredValidator => new RequiredJsonValueValidator();

            /// <summary>
            /// Custom validator to validate a required value against an empty json value.
            /// </summary>
            /// <remarks>
            /// <para>This validator is required because the default RequiredValidator uses ValueType to
            /// determine whether a property value is JSON, and for tags the ValueType is string although
            /// the underlying data is JSON. Yes, this makes little sense.</para>
            /// </remarks>
            private class RequiredJsonValueValidator : IValueRequiredValidator
            {
                /// <inheritdoc />
                public IEnumerable<ValidationResult> ValidateRequired(object value, string valueType)
                {
                    if (value == null)
                    {
                        yield return new ValidationResult("Value cannot be null", new[] {"value"});
                        yield break;
                    }

                    if (value.ToString().DetectIsEmptyJson())
                        yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                }
            }
        }
    }
}
