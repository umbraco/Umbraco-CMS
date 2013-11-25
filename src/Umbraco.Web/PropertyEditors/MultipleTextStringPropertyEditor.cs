using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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
    [PropertyEditor(Constants.PropertyEditors.MultipleTextstringAlias, "Multiple Textbox", "multipletextbox", ValueType = "TEXT")]
    public class MultipleTextStringPropertyEditor : PropertyEditor
    {
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new MultipleTextStringPropertyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MultipleTextStringPreValueEditor();
        }

        /// <summary>
        /// A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
        /// </summary>
        internal class MultipleTextStringPreValueEditor : PreValueEditor
        {
            public MultipleTextStringPreValueEditor()
            {
                //create the fields
                Fields.Add(new PreValueField(new IntegerValidator())
                {
                    Description = "Enter the minimum amount of text boxes to be displayed",
                    Key = "min",
                    View = "requiredfield",
                    Name = "Minimum"
                });
                Fields.Add(new PreValueField(new IntegerValidator())
                {
                    Description = "Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited",
                    Key = "max",
                    View = "requiredfield",
                    Name = "Maximum"
                });
            }

            /// <summary>
            /// Need to change how we persist the values so they are compatible with the legacy way we store values
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
            {
                //the values from the editor will be min/max fieds and we need to format to json in one field
                var min = (editorValue.ContainsKey("min") ? editorValue["min"].ToString() : "0").TryConvertTo<int>();
                var max = (editorValue.ContainsKey("max") ? editorValue["max"].ToString() : "0").TryConvertTo<int>();

                var json = JObject.FromObject(new {Minimum = min.Success ? min.Result : 0, Maximum = max.Success ? max.Result : 0});

                return new Dictionary<string, PreValue> { { "0", new PreValue(json.ToString(Formatting.None)) } };
            }

            /// <summary>
            /// Need to deal with the legacy way of storing pre-values and turn them into nice values for the editor
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var preVals = persistedPreVals.FormatAsDictionary();
                var stringVal = preVals.Any() ? preVals.First().Value.Value : "";
                var returnVal = new Dictionary<string, object> { { "min", 0 }, { "max", 0 } };
                if (stringVal.IsNullOrWhiteSpace() == false)
                {
                    try
                    {
                        var json = JsonConvert.DeserializeObject<JObject>(stringVal);
                        if (json["Minimum"] != null)
                        {
                            //by default pre-values are sent out with an id/value pair
                            returnVal["min"] = json["Minimum"].Value<int>();
                        }
                        if (json["Maximum"] != null)
                        {
                            returnVal["max"] = json["Maximum"].Value<int>();
                        }
                    }
                    catch (Exception e)
                    {
                        //this shouldn't happen unless there's already a bad formatted pre-value
                        LogHelper.WarnWithException<MultipleTextStringPreValueEditor>("Could not deserialize value to json " + stringVal, e);
                        return returnVal;
                    }
                }

                return returnVal;
            }
        }

        /// <summary>
        /// Custom value editor so we can format the value for the editor and the database
        /// </summary>
        internal class MultipleTextStringPropertyValueEditor : PropertyValueEditorWrapper
        {
            public MultipleTextStringPropertyValueEditor(PropertyValueEditor wrapped) : base(wrapped)
            {
            }
            
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
                return property.Value == null
                                  ? new JObject[] {}
                                  : property.Value.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(x => JObject.FromObject(new {value = x}));


            }

        }
    }
}
