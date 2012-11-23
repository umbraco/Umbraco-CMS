using System;

namespace Umbraco.Core.PropertyEditors
{
	internal class YesNoPropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			return Guid.Parse("38b352c1-e9f8-4fd8-9324-9a2eab06d97a").Equals(propertyEditorId);
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