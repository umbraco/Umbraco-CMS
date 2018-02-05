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
    [TagsPropertyEditor]
    [ValueEditor(Constants.PropertyEditors.Aliases.Tags, "Tags", "tags", Icon="icon-tags")]
    public class TagsPropertyEditor : PropertyEditor
    {
        private readonly ManifestValidatorCollection _validators;

        public TagsPropertyEditor(ManifestValidatorCollection validators, ILogger logger)
            : base(logger)
        {
            _validators = validators;
        }

        protected override ValueEditor CreateValueEditor() => new TagPropertyValueEditor(Attribute);

        protected override ConfigurationEditor CreateConfigurationEditor() => new TagConfigurationEditor(_validators);

        internal class TagPropertyValueEditor : ValueEditor
        {
            public TagPropertyValueEditor(ValueEditorAttribute attribute)
                : base(attribute)
            { }

            /// <inheritdoc />
            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                return editorValue.Value is JArray json
                    ? json.Select(x => x.Value<string>())
                    : null;
            }

            /// <inheritdoc />
            public override ManifestValidator RequiredValidator => new RequiredTagsValueValidator();

            /// <summary>
            /// Custom validator to validate a required value against an empty json value
            /// </summary>
            /// <remarks>
            /// This is required because the Tags property editor is not of type 'JSON', it's just string so the underlying
            /// validator does not validate against an empty json string
            /// </remarks>
            private class RequiredTagsValueValidator : ManifestValidator
            {
                /// <inheritdoc />
                public override string ValidationName => "Required";

                /// <inheritdoc />
                public override IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration, object validatorConfiguration)
                {
                    if (value == null)
                    {
                        yield return new ValidationResult("Value cannot be null", new[] { "value" });
                    }
                    else
                    {
                        var asString = value.ToString();

                        if (asString.DetectIsEmptyJson())
                        {
                            yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                        }

                        if (asString.IsNullOrWhiteSpace())
                        {
                            yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                        }
                    }
                }
            }
        }
    }
}
