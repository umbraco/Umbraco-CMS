using System;

namespace Umbraco.Core.PropertyEditors
{
	internal class YesNoPropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			return Guid.Parse(Constants.PropertyEditors.TrueFalse).Equals(propertyEditorId);
		}

		/// <summary>
		/// Convert from string boolean or 0 or 1 to real boolean
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Attempt<object> ConvertPropertyValue(object value)
		{
			return value.TryConvertTo(typeof(bool));
		}
	}
}