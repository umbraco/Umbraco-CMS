using System;

namespace Umbraco.Core.PropertyEditors
{
	internal class YesNoPropertyEditorValueConverter : IPropertyEditorValueConverter
	{		
		public bool CanConvertForEditor(Guid propertyEditorId)
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
			if (value == null) return new Attempt<object>(false, null);
			//if its already a bool
			if (TypeHelper.IsTypeAssignableFrom<bool>(value))
				return new Attempt<object>(true, value);
			//convert to string
			var asString = Convert.ToString(value);
			bool asBool;
			return bool.TryParse(asString, out asBool)
			       	? new Attempt<object>(true, asBool)
			       	: bool.TryParse(asString.Replace("1", "true").Replace("0", "false"), out asBool) //convert 0 or 1 to true or false
			       	  	? new Attempt<object>(true, asBool)
			       	  	: new Attempt<object>(false, null);
		}
	}
}