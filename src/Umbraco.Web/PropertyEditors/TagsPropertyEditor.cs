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
    [SupportTags(typeof(TagPropertyEditorTagDefinition), ValueType = TagValueType.CustomTagList)]
    [ValueEditor(Constants.PropertyEditors.Aliases.Tags, "Tags", "tags", Icon="icon-tags")]
    public class TagsPropertyEditor : PropertyEditor
    {
        private ManifestValidatorCollection _validators;

        public TagsPropertyEditor(ManifestValidatorCollection validators, ILogger logger)
            : base(logger)
        {
            _validators = validators;
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"group", "default"},
                    {"storageType", TagCacheStorageType.Csv.ToString()}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        /// <summary>
        /// Override to supply the default group
        /// </summary>
        public override IDictionary<string, object> DefaultConfiguration
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
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

            /// <summary>
            /// Returns the validator used for the required field validation which is specified on the PropertyType
            /// </summary>
            /// <remarks>
            /// This will become legacy as soon as we implement overridable pre-values.
            ///
            /// The default validator used is the RequiredValueValidator but this can be overridden by property editors
            /// if they need to do some custom validation, or if the value being validated is a json object.
            /// </remarks>
            public override ManifestValidator RequiredValidator
            {
                get { return new RequiredTagsValueValidator(); }
            }

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
