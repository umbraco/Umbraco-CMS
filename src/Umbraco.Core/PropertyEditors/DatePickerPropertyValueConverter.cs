using System;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    internal class DatePickerPropertyValueConverter : PropertyValueConverter
	{
	    public override string AssociatedPropertyEditorAlias
	    {
            get { return Constants.PropertyEditors.DateAlias; }
	    }

        /// <summary>
        /// return a DateTime object even if the value is a string
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <param name="propertyDefinition"></param>
        /// <param name="isPreviewing"></param>
        /// <returns></returns>
	    public override Attempt<object> ConvertSourceToObject(object valueToConvert, PublishedPropertyDefinition propertyDefinition, bool isPreviewing)
	    {
            return valueToConvert.TryConvertTo(typeof(DateTime));
	    }
	}
}