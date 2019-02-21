using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Collections;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Services
{
    //TODO: We should make this an interface and inject it into the ContentService
    internal class PropertyValidationService
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IDataTypeService _dataTypeService;

        public PropertyValidationService(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
        }

        //TODO: Remove this method in favor of the overload specifying all dependencies
        public PropertyValidationService()
            : this(Current.PropertyEditors, Current.Services.DataTypeService)
        {
        }

        /// <summary>
        /// Validates the content item's properties pass validation rules
        /// </summary>
        /// <para>If the content type is variant, then culture can be either '*' or an actual culture, but neither 'null' nor
        /// 'empty'. If the content type is invariant, then culture can be either '*' or null or empty.</para>
        public bool IsPropertyDataValid(IContentBase content, out Property[] invalidProperties, string culture = "*")
        {
            // select invalid properties
            invalidProperties = content.Properties.Where(x =>
            {
                // if culture is null, we validate invariant properties only
                // if culture is '*' we validate both variant and invariant properties, automatically
                // if culture is specific eg 'en-US' we both too, but explicitly

                var varies = x.PropertyType.VariesByCulture();

                if (culture == null)
                    return !(varies || IsPropertyValid(x, null)); // validate invariant property, invariant culture

                if (culture == "*")
                    return !IsPropertyValid(x, culture); // validate property, all cultures

                return varies
                    ? !IsPropertyValid(x, culture) // validate variant property, explicit culture
                    : !IsPropertyValid(x, null); // validate invariant property, explicit culture
            })
                .ToArray();

            return invalidProperties.Length == 0;
        }

        /// <summary>
        /// Gets a value indicating whether the property has valid values.
        /// </summary>
        public bool IsPropertyValid(Property property, string culture = "*", string segment = "*")
        {
            //NOTE - the pvalue and vvalues logic in here is borrowed directly from the Property.Values setter so if you are wondering what that's all about, look there.
            // The underlying Property._pvalue and Property._vvalues are not exposed but we can re-create these values ourselves which is what it's doing.

            culture = culture.NullOrWhiteSpaceAsNull();
            segment = segment.NullOrWhiteSpaceAsNull();

            Property.PropertyValue pvalue = null;

            // if validating invariant/neutral, and it is supported, validate
            // (including ensuring that the value exists, if mandatory)
            if ((culture == null || culture == "*") && (segment == null || segment == "*") && property.PropertyType.SupportsVariation(null, null))
            {
                // validate pvalue (which is the invariant value)
                pvalue = property.Values.FirstOrDefault(x => x.Culture == null && x.Segment == null);
                if (!IsValidPropertyValue(property, pvalue?.EditedValue))
                    return false;
            }

            // if validating only invariant/neutral, we are good
            if (culture == null && segment == null)
                return true;

            // if nothing else to validate, we are good
            if ((culture == null || culture == "*") && (segment == null || segment == "*") && !property.PropertyType.VariesByCulture())
                return true;

            // for anything else, validate the existing values (including mandatory),
            // but we cannot validate mandatory globally (we don't know the possible cultures and segments)

            // validate vvalues (which are the variant values)

            // if we don't have vvalues (property.Values is empty or only contains pvalue), validate null
            if (property.Values.Count == (pvalue == null ? 0 : 1))
                return culture == "*" || IsValidPropertyValue(property, null);

            // else validate vvalues (but don't revalidate pvalue)
            var pvalues = property.Values.Where(x =>
                    x != pvalue && // don't revalidate pvalue
                    property.PropertyType.SupportsVariation(x.Culture, x.Segment, true) && // the value variation is ok
                    (culture == "*" || x.Culture.InvariantEquals(culture)) && // the culture matches
                    (segment == "*" || x.Segment.InvariantEquals(segment))) // the segment matches
                .ToList();

            return pvalues.Count == 0 || pvalues.All(x => IsValidPropertyValue(property, x.EditedValue));
        }

        /// <summary>
        /// Boolean indicating whether the passed in value is valid
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns>True is property value is valid, otherwise false</returns>
        private bool IsValidPropertyValue(Property property, object value)
        {
            return IsPropertyValueValid(property.PropertyType, value);
        }

        /// <summary>
        /// Determines whether a value is valid for this property type.
        /// </summary>
        private bool IsPropertyValueValid(PropertyType propertyType, object value)
        {
            var editor = _propertyEditors[propertyType.PropertyEditorAlias];
            var configuration = _dataTypeService.GetDataType(propertyType.DataTypeId).Configuration;
            var valueEditor = editor.GetValueEditor(configuration);
            return !valueEditor.Validate(value, propertyType.Mandatory, propertyType.ValidationRegExp).Any();
        }
    }
}
