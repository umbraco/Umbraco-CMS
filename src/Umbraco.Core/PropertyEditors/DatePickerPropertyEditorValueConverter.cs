using System;
using System.Linq;

namespace Umbraco.Core.PropertyEditors
{
	internal class DatePickerPropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			return (new[]
				{
					Guid.Parse(Constants.PropertyEditors.DateTime),
					Guid.Parse(Constants.PropertyEditors.Date)
				}).Contains(propertyEditorId);
		}

		/// <summary>
		/// return a DateTime object even if the value is a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Attempt<object> ConvertPropertyValue(object value)
		{
			return value.TryConvertTo(typeof(DateTime));
		}
	}
}