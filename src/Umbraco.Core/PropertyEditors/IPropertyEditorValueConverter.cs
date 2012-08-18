using System;
using Umbraco.Core.Dynamics;

namespace Umbraco.Core.PropertyEditors
{
	internal interface IPropertyEditorValueConverter
	{
		/// <summary>
		/// Returns true if this converter can perform the value conversion for the specified property editor id
		/// </summary>
		/// <param name="propertyEditorId"></param>
		/// <returns></returns>
		bool CanConvertForEditor(Guid propertyEditorId);

		/// <summary>
		/// Attempts to convert the value specified into a useable value on the front-end
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>
		/// This is used to convert the value stored in the repository into a usable value on the front-end.
		/// For example, if a 0 or 1 is stored for a boolean, we'd want to convert this to a real boolean.
		/// 
		/// Also note that the value might not come in as a 0 or 1 but as a "0" or "1"
		/// </remarks>
		Attempt<object> ConvertPropertyValue(object value);
	}
}