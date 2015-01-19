using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [SupportTags(typeof(TagPropertyEditorTagDefinition), ValueType = TagValueType.CustomTagList)]
    [PropertyEditor(Constants.PropertyEditors.TagsAlias, "Tags", "tags")]
    public class TagsPropertyEditor : PropertyEditor
    {
        public TagsPropertyEditor()
        {
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
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new TagPropertyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TagPreValueEditor();
        }

        internal class TagPropertyValueEditor : PropertyValueEditorWrapper
        {
            public TagPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
            }

            /// <summary>
            /// This needs to return IEnumerable{string}
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                var json = editorValue.Value as JArray;
                return json == null ? null : json.Select(x => x.Value<string>());
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
            internal override ManifestValueValidator RequiredValidator
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
            [ValueValidator("Required")]
            private class RequiredTagsValueValidator : ManifestValueValidator
            {
                /// <summary>
                /// Validates a null value or an empty json value
                /// </summary>
                /// <param name="value"></param>
                /// <param name="config"></param>
                /// <param name="preValues"></param>
                /// <param name="editor"></param>
                /// <returns></returns>
                public override IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor)
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

        internal class TagPreValueEditor : PreValueEditor
        {
            public TagPreValueEditor()
            {
                Fields.Add(new PreValueField(new ManifestPropertyValidator { Type = "Required" })
                {
                    Description = "Define a tag group",
                    Key = "group",
                    Name = "Tag group",
                    View = "requiredfield"
                });

                Fields.Add(new PreValueField(new ManifestPropertyValidator {Type = "Required"})
                {
                    Description = "Select whether to store the tags in cache as CSV (default) or as JSON. The only benefits of storage as JSON is that you are able to have commas in a tag value but this will require parsing the json in your views or using a property value converter",
                    Key = "storageType",
                    Name = "Storage Type",
                    View = "views/propertyeditors/tags/tags.prevalues.html"
                });
            }

            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var result = base.ConvertDbToEditor(defaultPreVals, persistedPreVals);

                //This is required because we've added this pre-value so old installs that don't have it will need to have a default.
                if (result.ContainsKey("storageType") == false || result["storageType"] == null || result["storageType"].ToString().IsNullOrWhiteSpace())
                {
                    result["storageType"] = TagCacheStorageType.Csv.ToString();
                }

                return result;
            }
        }
    }
}