using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the value editor for the property editor during content editing
    /// </summary>
    public class ValueEditor
    {
        /// <summary>
        /// assign defaults
        /// </summary>
        public ValueEditor()
        {
            ValueType = "string";
            //set a default for validators
            Validators = Enumerable.Empty<ValidatorBase>();
        }

        /// <summary>
        /// Creates a new editor with the specified view
        /// </summary>
        /// <param name="view"></param>
        public ValueEditor(string view)
            : this()
        {
            View = view;
        }

        /// <summary>
        /// The full virtual path or the relative path to the current Umbraco folder for the angular view
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
        [JsonProperty("validation")]
        public IEnumerable<ValidatorBase> Validators { get; set; }

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
        internal Attempt<object> TryConvertValueToCrlType(string value)
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
                    valueType = typeof(int);
                    break;
                case DataTypeDatabaseType.Date:
                    valueType = typeof(DateTime);
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
        public virtual object DeserializeValue(ContentPropertyData editorValue, object currentValue)
        {
            var result = TryConvertValueToCrlType(editorValue.Value);
            if (result.Success == false)
            {
                throw new InvalidOperationException("The value " + editorValue + " cannot be converted to the type " + GetDatabaseType());
            }
            return result.Result;
        }

        /// <summary>
        /// A method used to serialize the databse value to a string value which is then used to be sent
        /// to the editor in JSON format.
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public virtual string SerializeValue(object dbValue)
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
                    //Dates will be formatted in 'o' format (otherwise known as xml format)
                    return dbValue.ToXmlString<DateTime>();
                default:
                    throw new ArgumentOutOfRangeException();
            }            
        }
    }
}