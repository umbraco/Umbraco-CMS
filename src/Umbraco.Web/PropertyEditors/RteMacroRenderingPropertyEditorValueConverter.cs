using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
	/// <summary>
	/// A value converter for TinyMCE that will ensure any macro content is rendered properly even when 
	/// used dynamically.
	/// </summary>
	internal class RteMacroRenderingPropertyEditorValueConverter : TinyMcePropertyEditorValueConverter
	{

		/// <summary>
		/// Return IHtmlString so devs doesn't need to decode html
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override Attempt<object> ConvertPropertyValue(object value)
		{
			//TODO: we need to make a few new classes, like a new MacroParser, etc... and move a bunch of the logic
			// out of 'Item' that does all of this but it will be tricky because of the way that it creates a 'Macro' class
			// we may have to do some server Execute trickyness.
			// Then, we need to add this to the resolver and remove the base class and make it abstract.

			var sb = new StringBuilder();
			var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
			MacroTagParser.ParseMacros(
				value.ToString(),
				//callback for when text block is found
				textBlock => sb.Append(textBlock),
				//callback for when macro syntax is found
				(macroAlias, macroAttributes) => sb.Append(umbracoHelper.RenderMacro(
					macroAlias, 
					//needs to be explicitly casted to Dictionary<string, object>
					macroAttributes.ConvertTo(x => (string)x, x => (object)x)).ToString()));

			return new Attempt<object>(true, new HtmlString(sb.ToString()));
		}
	}
}