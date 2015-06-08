using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the value editor for the property editor during content editing
    /// </summary>
    /// <remarks>
    /// The Json serialization attributes are required for manifest property editors to work
    /// </remarks>
    public class PropertyValueEditor : IValueEditor
    {
        /// <summary>
        /// assign defaults
        /// </summary>
        public PropertyValueEditor()
        {
            ValueType = "string";
            //set a default for validators
            Validators = new List<IPropertyValidator>();
        }

        /// <summary>
        /// Creates a new editor with the specified view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="validators">Allows adding custom validators during construction instead of specifying them later</param>
        public PropertyValueEditor(string view, params IPropertyValidator[] validators)
            : this()
        {
            View = view;
            foreach (var v in validators)
            {
                Validators.Add(v);
            }
        }

        private PreValueCollection _preVals;
        protected PreValueCollection PreValues
        {
            get
            {
                if (_preVals == null)
                {
                    throw new InvalidOperationException("Pre values cannot be accessed until the Configure method has been called");
                }
                return _preVals;
            }
        }

        /// <summary>
        /// This is called to configure the editor for display with it's prevalues, useful when properties need to change dynamically
        /// depending on what is in the pre-values.
        /// </summary>
        /// <param name="preValues"></param>
        /// <remarks>
        /// This cannot be used to change the value being sent to the editor, ConfigureEditor will be called *after* ConvertDbToEditor, pre-values
        /// should not be used to modify values.
        /// </remarks>
        public virtual void ConfigureForDisplay(PreValueCollection preValues)
        {
            if (preValues == null) throw new ArgumentNullException("preValues");
            _preVals = preValues;
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
        public List<IPropertyValidator> Validators { get; private set; }

        /// <summary>
        /// Returns the validator used for the required field validation which is specified on the PropertyType
        /// </summary>
        /// <remarks>
        /// This will become legacy as soon as we implement overridable pre-values.
        /// 
        /// The default validator used is the RequiredValueValidator but this can be overridden by property editors
        /// if they need to do some custom validation, or if the value being validated is a json object.
        /// </remarks>
        public virtual ManifestValueValidator RequiredValidator
        {
            get { return new RequiredManifestValueValidator(); }
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
        public virtual ManifestValueValidator RegexValidator
        {
            get { return new RegexValidator(); }
        }

        /// <summary>
        /// Returns the true DataTypeDatabaseType from the string representation ValueType
        /// </summary>
        /// <returns></returns>
        public DataTypeDatabaseType GetDatabaseType()
        {
            switch (ValueType.ToUpper(CultureInfo.InvariantCulture))
            {
                case "INT":
                case "INTEGER":
                    return DataTypeDatabaseType.Integer;
                case "STRING":
                    return DataTypeDatabaseType.Nvarchar;
                case "TEXT":
                case "JSON":
                case "XML":
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
        /// If this is is true than the editor will be displayed full width without a label
        /// </summary>
        [JsonProperty("hideLabel")]
        public bool HideLabel { get; set; }

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
            //this is a custom check to avoid any errors, if it's a string and it's empty just make it null
            var s = value as string;
            if (s != null)
            {
                if (s.IsNullOrWhiteSpace())
                {
                    value = null;
                }
            }

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
                    //NOTE: This is allowing type of 'long' because I think json.net will deserialize a numerical value as long
                    // instead of int. Even though our db will not support this (will get truncated), we'll at least parse to this.
                    
                    valueType = typeof(long?);

                    //if parsing is successful, we need to return as an Int, we're only dealing with long's here because of json.net, we actually
                    //don't support long values and if we return a long value it will get set as a 'long' on the Property.Value (object) and then
                    //when we compare the values for dirty tracking we'll be comparing an int -> long and they will not match.
                    var result = value.TryConvertTo(valueType);
                    return result.Success && result.Result != null
                        ? Attempt<object>.Succeed((int)(long)result.Result) 
                        : result;

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
        public virtual object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
        {
            //if it's json but it's empty json, then return null
            if (ValueType.InvariantEquals("JSON") && editorValue.Value != null && editorValue.Value.ToString().DetectIsEmptyJson())
            {
                return null;
            }

            var result = TryConvertValueToCrlType(editorValue.Value);
            if (result.Success == false)
            {
                LogHelper.Warn<PropertyValueEditor>("The value " + editorValue.Value + " cannot be converted to the type " + GetDatabaseType());
                return null;
            }
            return result.Result;
        }

        //TODO: Change the result to object so we can pass back JSON or json converted clr types if we want!

        /// <summary>
        /// A method used to format the database value to a value that can be used by the editor
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        /// <remarks>
        /// The object returned will automatically be serialized into json notation. For most property editors
        /// the value returned is probably just a string but in some cases a json structure will be returned.
        /// </remarks>
        public virtual object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (property.Value == null) return string.Empty;

            switch (GetDatabaseType())
            {
                case DataTypeDatabaseType.Ntext:                    
                case DataTypeDatabaseType.Nvarchar:
                    //if it is a string type, we will attempt to see if it is json stored data, if it is we'll try to convert
                    //to a real json object so we can pass the true json object directly to angular!
                    var asString = property.Value.ToString();
                    if (asString.DetectIsJson())
                    {
                        try
                        {
                            var json = JsonConvert.DeserializeObject(asString);
                            return json;
                        }
                        catch
                        {
                            //swallow this exception, we thought it was json but it really isn't so continue returning a string
                        }
                    }
                    return property.Value.ToString();
                case DataTypeDatabaseType.Integer:
                    //we can just ToString() any of these types
                    return property.Value.ToString();
                case DataTypeDatabaseType.Date:
                    var date = property.Value.TryConvertTo<DateTime?>();
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
        /// Converts the property db value to an XML fragment
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will just return the value of ConvertDbToString but ensure that if the db value type is nvarchar or text
        /// it is a CDATA fragment, otherwise it is just a text fragment.
        /// 
        /// This method by default will only return XText or XCData which must be wrapped in an element!
        /// 
        /// If the value is empty we will not return as CDATA since that will just take up more space in the file.
        /// </remarks>
        public virtual XNode ConvertDbToXml(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            //check for null or empty value, we don't want to return CDATA if that is the case
            if (property.Value == null || property.Value.ToString().IsNullOrWhiteSpace())
            {
                return new XText(ConvertDbToString(property, propertyType, dataTypeService));
            }

            switch (GetDatabaseType())
            {
                case DataTypeDatabaseType.Date:
                case DataTypeDatabaseType.Integer:
                    return new XText(ConvertDbToString(property, propertyType, dataTypeService));                    
                case DataTypeDatabaseType.Nvarchar:
                case DataTypeDatabaseType.Ntext:
                    //put text in cdata
                    return new XCData(ConvertDbToString(property, propertyType, dataTypeService));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Converts the property value for use in the front-end cache
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        public virtual string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (property.Value == null)
                return string.Empty;

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