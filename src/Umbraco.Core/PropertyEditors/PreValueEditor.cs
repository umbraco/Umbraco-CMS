using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a pre-value editor
    /// </summary>
    /// <remarks>
    /// A pre-value editor is made up of multiple pre-value fields, each field defines a key that the value is stored against.
    /// Each field can have any editor and the value from each field can store any data such as a simple string or a json structure. 
    /// 
    /// The Json serialization attributes are required for manifest property editors to work.
    /// </remarks>
    public class PreValueEditor
    {
        public PreValueEditor()
        {
            var fields = new List<PreValueField>();
               
            //the ctor checks if we have PreValueFieldAttributes applied and if so we construct our fields from them
            var props = TypeHelper.CachedDiscoverableProperties(GetType())
                .Where(x => x.Name != "Fields");
            foreach (var p in props)
            {
                var att = p.GetCustomAttributes(typeof (PreValueFieldAttribute), false).OfType<PreValueFieldAttribute>().SingleOrDefault();
                if (att != null)
                {
                    if (att.PreValueFieldType != null)
                    {
                        //try to create it
                        try
                        {
                            var instance = (PreValueField) Activator.CreateInstance(att.PreValueFieldType);
                            //overwrite values if they are assigned
                            if (!att.Key.IsNullOrWhiteSpace())
                            {
                                instance.Key = att.Key;
                            }
                            //if the key is still empty then assign it to be the property name
                            if (instance.Key.IsNullOrWhiteSpace())
                            {
                                instance.Key = p.Name;
                            }
                                
                            if (!att.Name.IsNullOrWhiteSpace())
                                instance.Name = att.Name;
                            if (!att.View.IsNullOrWhiteSpace())
                                instance.View = att.View;
                            if (!att.Description.IsNullOrWhiteSpace())
                                instance.Description = att.Description;
                            if (att.HideLabel)
                                instance.HideLabel = att.HideLabel;

                            //add the custom field
                            fields.Add(instance);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WarnWithException<PreValueEditor>("Could not create an instance of " + att.PreValueFieldType, ex);                            
                        }
                    }
                    else
                    {
                        fields.Add(MapAttributeToField(att, p));
                    }
                }
            }

            Fields = fields;
        }

        private static PreValueField MapAttributeToField(PreValueFieldAttribute att, PropertyInfo prop)
        {
            return new PreValueField
                {
                    //set the key to the property name if it is empty
                    Key = att.Key.IsNullOrWhiteSpace() ? prop.Name : att.Key,
                    Name = att.Name,
                    Description = att.Description,
                    HideLabel = att.HideLabel,
                    View = att.View.StartsWith("~/") ? IOHelper.ResolveUrl(att.View) : att.View
                };
        }

        /// <summary>
        /// A collection of pre-value fields to be edited
        /// </summary>
        /// <remarks>
        /// If fields are specified then the master View and Validators will be ignored
        /// </remarks>
        [JsonProperty("fields")]
        public List<PreValueField> Fields { get; private set; }

        /// <summary>
        /// A method to format the posted values from the editor to the values to be persisted
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue">
        /// The current value that has been persisted to the database for this pre-value editor. This value may be usesful for 
        /// how the value then get's deserialized again to be re-persisted. In most cases it will probably not be used.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will just return the Posted editorValue.
        /// 
        /// This can be overridden if perhaps you have a comma delimited string posted value but want to convert those to individual rows, or to convert
        /// a json structure to multiple rows.
        /// </remarks>
        public virtual IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
        {
            //convert to a string based value to be saved in the db
            return editorValue.ToDictionary(x => x.Key, x => new PreValue(x.Value == null ? null : x.Value.ToString()));
        }

        /// <summary>
        /// This can be used to re-format the currently saved pre-values that will be passed to the editor,
        /// by default this returns the merged default and persisted pre-values.
        /// </summary>
        /// <param name="defaultPreVals">
        /// The default/static pre-vals for the property editor
        /// </param>
        /// <param name="persistedPreVals">
        /// The persisted pre-vals for the property editor
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// This is generally not going to be used by anything unless a property editor wants to change the merging
        /// functionality or needs to convert some legacy persisted data, or convert the string values to strongly typed values in json (i.e. booleans)
        /// 
        /// IMPORTANT! When using this method the default pre values dictionary should not be modified which would change the property editor's global 
        /// singleton pre-values!
        /// </remarks>
        public virtual IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
        {
            //we'll make a copy since we'll merge into the defaults - but we don't want to overwrite the global singleton ones passed in!
            var defaultPreValCopy = new Dictionary<string, object>();
            if (defaultPreVals != null)
            {
                defaultPreValCopy = new Dictionary<string, object>(defaultPreVals);
            }

            if (persistedPreVals.IsDictionaryBased)
            {
                //we just need to merge the dictionaries now, the persisted will replace default.
                foreach (var item in persistedPreVals.PreValuesAsDictionary)
                {
                    //The persisted dictionary contains values of type PreValue which contain the ID and the Value, we don't care
                    // about the Id, just the value so ignore the id.
                    defaultPreValCopy[item.Key] = item.Value.Value;
                }
                //now we're going to try to see if any of the values are JSON, if they are we'll convert them to real JSON objects
                // so they can be consumed as real json in angular!
                ConvertItemsToJsonIfDetected(defaultPreValCopy);

                return defaultPreValCopy;
            }

            //it's an array so need to format it 
            var result = new Dictionary<string, object>();
            var asArray = persistedPreVals.PreValuesAsArray.ToArray();
            for (var i = 0; i < asArray.Length; i++)
            {
                //each item is of type PreValue but we don't want the ID, just the value so ignore the ID
                result.Add(i.ToInvariantString(), asArray[i].Value);
            }

            //now we're going to try to see if any of the values are JSON, if they are we'll convert them to real JSON objects
            // so they can be consumed as real json in angular!
            ConvertItemsToJsonIfDetected(result);

            return result;
        }

        protected void ConvertItemsToJsonIfDetected(IDictionary<string, object> result)
        {
            // convert values that are Json to true Json objects that can be consumed by Angular

            var keys = result.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                if ((result[keys[i]] is string) == false) continue;

                var asString = result[keys[i]].ToString();
                if (asString.DetectIsJson() == false) continue;

                try
                {
                    result[keys[i]] = JsonConvert.DeserializeObject(asString);
                }
                catch
                {
                    // swallow this exception, we thought it was Json but it really isn't so continue returning a string
                }
            }
        }

    }
}
