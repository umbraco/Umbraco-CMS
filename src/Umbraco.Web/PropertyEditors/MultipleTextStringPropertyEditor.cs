using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.MultipleTextstring, "Repeatable textstrings", "multipletextbox", ValueType = ValueTypes.Text, Icon="icon-ordered-list", Group="lists")]
    public class MultipleTextStringPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public MultipleTextStringPropertyEditor(ILogger logger) : base(logger)
        { }

        protected override ValueEditor CreateValueEditor() => new MultipleTextStringPropertyValueEditor(Attribute);

        protected override ConfigurationEditor CreateConfigurationEditor() => new MultipleTextStringConfigurationEditor();

        /// <summary>
        /// Custom value editor so we can format the value for the editor and the database
        /// </summary>
        internal class MultipleTextStringPropertyValueEditor : ValueEditor
        {
            public MultipleTextStringPropertyValueEditor(ValueEditorAttribute attribute)
                : base(attribute)
            { }

            /// <summary>
            /// The value passed in from the editor will be an array of simple objects so we'll need to parse them to get the string
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            /// <remarks>
            /// We will also check the pre-values here, if there are more items than what is allowed we'll just trim the end
            /// </remarks>
            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                var asArray = editorValue.Value as JArray;
                if (asArray == null)
                {
                    return null;
                }

                var preVals = editorValue.PreValues.FormatAsDictionary();
                var max = -1;
                if (preVals.Any())
                {
                    try
                    {
                        var json = JsonConvert.DeserializeObject<JObject>(preVals.First().Value.Value);
                        max = int.Parse(json["Maximum"].ToString());
                    }
                    catch (Exception)
                    {
                        //swallow
                        max = -1;
                    }
                }

                //The legacy property editor saved this data as new line delimited! strange but we have to maintain that.
                var array = asArray.OfType<JObject>()
                                   .Where(x => x["value"] != null)
                                   .Select(x => x["value"].Value<string>());

                //only allow the max if over 0
                if (max > 0)
                {
                    return string.Join(Environment.NewLine, array.Take(max));
                }

                return string.Join(Environment.NewLine, array);
            }

            /// <summary>
            /// We are actually passing back an array of simple objects instead of an array of strings because in angular a primitive (string) value
            /// cannot have 2 way binding, so to get around that each item in the array needs to be an object with a string.
            /// </summary>
            /// <param name="property"></param>
            /// <param name="propertyType"></param>
            /// <param name="dataTypeService"></param>
            /// <returns></returns>
            /// <remarks>
            /// The legacy property editor saved this data as new line delimited! strange but we have to maintain that.
            /// </remarks>
            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                return property.GetValue() == null
                                  ? new JObject[] {}
                                  : property.GetValue().ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(x => JObject.FromObject(new {value = x}));


            }

        }
    }
}
