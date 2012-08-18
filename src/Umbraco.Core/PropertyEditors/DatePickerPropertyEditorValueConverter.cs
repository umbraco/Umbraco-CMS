using System;
using System.Linq;

namespace Umbraco.Core.PropertyEditors
{
	internal class DatePickerPropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool CanConvertForEditor(Guid propertyEditorId)
		{
			return (new[]
				{
					Guid.Parse("b6fb1622-afa5-4bbf-a3cc-d9672a442222"),
					Guid.Parse("23e93522-3200-44e2-9f29-e61a6fcbb79a")
				}).Contains(propertyEditorId);
		}

		/// <summary>
		/// return a DateTime object even if the value is a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Attempt<object> ConvertPropertyValue(object value)
		{
			if (value == null) return new Attempt<object>(false, null);
			//if its already a DateTime
			if (TypeHelper.IsTypeAssignableFrom<DateTime>(value))
				return new Attempt<object>(true, value);
			//convert to string
			var asString = Convert.ToString(value);
			DateTime dt;
			return DateTime.TryParse(asString, out dt) 
			       	? new Attempt<object>(true, dt) 
			       	: new Attempt<object>(false, null);
		}
	}
}