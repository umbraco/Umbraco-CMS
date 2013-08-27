using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the value editor for the property editor during content editing
    /// </summary>
    /// <remarks>
    /// The Json serialization attributes are required for manifest property editors to work
    /// </remarks>
    public class ValueEditor
    {
        /// <summary>
        /// assign defaults
        /// </summary>
        public ValueEditor()
        {
            ValueType = "string";
            //set a default for validators
            Validators = new List<ValidatorBase>();
        }

        /// <summary>
        /// Creates a new editor with the specified view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="validators">Allows adding custom validators during construction instead of specifying them later</param>
        public ValueEditor(string view, params ValidatorBase[] validators)
            : this()
        {
            View = view;
            foreach (var v in validators)
            {
                Validators.Add(v);
            }
        }

        /// <summary>
        /// Defines the view to use for the editor, this can be one of 3 things:
        /// * the full virtual path or 
        /// * the relative path to the current Umbraco folder 
        /// * a simple view name which will map to the views/propertyeditors/{view}/{view}.html
        /// </summary>
        [JsonProperty("view", Required = Required.Always)]
        public string View { get; set; }

        /// <summary>
        /// The value type which reflects how it is validated and stored in the database
        /// </summary>
        [JsonProperty("valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// A collection of validators for the pre value editor
        /// </summary>
        [JsonProperty("validation", ItemConverterType = typeof(ManifestValidatorConverter))]
        public List<ValidatorBase> Validators { get; private set; }

        /// <summary>
        /// Returns the validator used for the required field validation which is specified on the PropertyType
        /// </summary>
        /// <remarks>
        /// This will become legacy as soon as we implement overridable pre-values.
        /// 
        /// The default validator used is the RequiredValueValidator but this can be overridden by property editors
        /// if they need to do some custom validation, or if the value being validated is a json object.
        /// </remarks>
        internal virtual ValueValidator RequiredValidator
        {
            get { return new RequiredValueValidator(); }
        }

        /// <summary>
        /// Returns the validator used for the regular expression field validation which is specified on the PropertyType
        /// </summary>
        /// <remarks>
        /// This will become legacy as soon as we implement overridable pre-values.
        /// 
        /// The default validator used is the RegexValueValidator but this can be overridden by property editors
        /// if they need to do some custom validation, or if the value being validated is a json object.
        /// </remarks>
        internal virtual ValueValidator RegexValidator
        {
            get { return new RegexValueValidator(); }
        }

        /// <summary>
        /// Returns the true DataTypeDatabaseType from the string representation ValueType
        /// </summary>
        /// <returns></returns>
        public DataTypeDatabaseType GetDatabaseType()
        {
            switch (ValueType.ToUpper())
            {
                case "INT":
                case "INTEGER":
                    return DataTypeDatabaseType.Integer;
                case "STRING":
                    return DataTypeDatabaseType.Nvarchar;
                case "TEXT":
                    return DataTypeDatabaseType.Ntext;
                case "DATETIME":
                case "DATE":
                case "TIME":
                    return DataTypeDatabaseType.Date;
                default:
                    throw new FormatException("The ValueType does not match a known value type");
            }
        }

        /// <summary>
        /// Set this to true if the property editor is for display purposes only
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Used to try to convert the string value to the correct CLR type based on the DatabaseDataType specified for this value editor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal Attempt<object> TryConvertValueToCrlType(object value)
        {
            Type valueType;
            //convert the string to a known type
            switch (GetDatabaseType())
            {
                case DataTypeDatabaseType.Ntext:
                case DataTypeDatabaseType.Nvarchar:
                    valueType = typeof(string);
                    break;
                case DataTypeDatabaseType.Integer:
                    //ensure these are nullable so we can return a null if required
                    valueType = typeof(int?);
                    break;
                case DataTypeDatabaseType.Date:
                    //ensure these are nullable so we can return a null if required
                    valueType = typeof(DateTime?);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return value.TryConvertTo(valueType);
        }

        /// <summary>
        /// A method to deserialize the string value that has been saved in the content editor
        /// to an object to be stored in the database.
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue">
        /// The current value that has been persisted to the database for this editor. This value may be usesful for 
        /// how the value then get's deserialized again to be re-persisted. In most cases it will probably not be used.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will attempt to automatically convert the string value to the value type supplied by ValueType.
        /// 
        /// If overridden then the object returned must match the type supplied in the ValueType, otherwise persisting the 
        /// value to the DB will fail when it tries to validate the value type.
        /// </remarks>
        public virtual object FormatDataForPersistence(ContentPropertyData editorValue, object currentValue)
        {
            var result = TryConvertValueToCrlType(editorValue.Value);
            if (result.Success == false)
            {
                throw new InvalidOperationException("The value " + editorValue + " cannot be converted to the type " + GetDatabaseType());
            }
            return result.Result;
        }

        //TODO: Change the result to object so we can pass back JSON or json converted clr types if we want!

        /// <summary>
        /// A method used to format the databse value to a value that can be used by the editor
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// The object returned will automatically be serialized into json notation. For most property editors
        /// the value returned is probably just a string but in some cases a json structure will be returned.
        /// </remarks>
        public virtual object FormatDataForEditor(object dbValue)
        {
            if (dbValue == null) return string.Empty;

            switch (GetDatabaseType())
            {
                case DataTypeDatabaseType.Ntext:                    
                case DataTypeDatabaseType.Nvarchar:                    
                case DataTypeDatabaseType.Integer:
                    //we can just ToString() any of these types
                    return dbValue.ToString();
                case DataTypeDatabaseType.Date:                    
                    var date = dbValue.TryConvertTo<DateTime?>();
                    if (date.Success == false || date.Result == null)
                    {
                        return string.Empty;
                    }
                    //Dates will be formatted as yyyy-MM-dd HH:mm:ss
                    return date.Result.Value.ToIsoString();
                default:
                    throw new ArgumentOutOfRangeException();
            }            
        }

        /// <summary>
        /// Converts the property value for use in the front-end cache
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual object FormatValueForCache(Property property)
        {
            if (property.Value == null)
            {
                return string.Empty;
            }

            switch (GetDatabaseType())
            {
                case DataTypeDatabaseType.Nvarchar:
                case DataTypeDatabaseType.Ntext:
                    property.Value.ToXmlString<string>();
                    return property.Value.ToXmlString<string>();
                case DataTypeDatabaseType.Integer:
                    return property.Value.ToXmlString(property.Value.GetType());                
                case DataTypeDatabaseType.Date:
                    //treat dates differently, output the format as xml format
                    if (property.Value == null)
                    {
                        return string.Empty;
                    }
                    var date = property.Value.TryConvertTo<DateTime?>();
                    if (date.Success == false || date.Result == null)
                    {
                        return string.Empty;
                    }
                    return date.Result.ToXmlString<DateTime>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}