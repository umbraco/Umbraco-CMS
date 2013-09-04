using System;
using System.Web;

namespace Umbraco.Core.PropertyEditors
{
	/// <summary>
	/// Value converter for the RTE so that it always returns IHtmlString so that Html.Raw doesn't have to be used.
	/// </summary>
	internal class TinyMcePropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			return Guid.Parse(Constants.PropertyEditors.TinyMCEv3).Equals(propertyEditorId);
		}

		/// <summary>
		/// Return IHtmlString so devs doesn't need to decode html
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual Attempt<object> ConvertPropertyValue(object value)
		{
			return Attempt<object>.Succ(new HtmlString(value.ToString()));
		}
	}
}