using System;
using System.Web;

namespace Umbraco.Core.PropertyEditors
{
	internal class TinyMcePropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool CanConvertForEditor(Guid propertyEditorId)
		{
			return Guid.Parse("5e9b75ae-face-41c8-b47e-5f4b0fd82f83").Equals(propertyEditorId);
		}

		/// <summary>
		/// Return IHtmlString so devs doesn't need to decode html
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Attempt<object> ConvertPropertyValue(object value)
		{
			return new Attempt<object>(true, new HtmlString(value.ToString()));
		}
	}
}