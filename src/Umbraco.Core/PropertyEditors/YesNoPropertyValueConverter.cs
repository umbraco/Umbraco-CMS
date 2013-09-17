using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    internal class YesNoPropertyValueConverter : PropertyValueConverter
	{
        public override string AssociatedPropertyEditorAlias
        {
            get { return Constants.PropertyEditors.TrueFalseAlias; }
        }

        /// <summary>
        /// Convert from string boolean or 0 or 1 to real boolean
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <param name="propertyDefinition"></param>
        /// <param name="isPreviewing"></param>
        /// <returns></returns>
        public override Attempt<object> ConvertSourceToObject(object valueToConvert, PublishedPropertyDefinition propertyDefinition, bool isPreviewing)
        {
            return valueToConvert.TryConvertTo(typeof(bool));
        }
	}
}