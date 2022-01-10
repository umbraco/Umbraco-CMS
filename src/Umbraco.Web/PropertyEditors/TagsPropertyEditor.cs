using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

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

        public TagsPropertyEditor(ManifestValueValidatorCollection validators, ILogger logger)
            : base(logger)
        {
            _validators = validators;
        }

        protected override IDataValueEditor CreateValueEditor() => new TagPropertyValueEditor(Attribute);

        protected override IConfigurationEditor CreateConfigurationEditor() => new TagConfigurationEditor(_validators);

        internal class TagPropertyValueEditor : DataValueEditor
        {
            public TagPropertyValueEditor(DataEditorAttribute attribute)
                : base(attribute)
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
                    return json.HasValues ? json.Select(x => x.Value<string>()) : null;
                }

                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    return value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
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
