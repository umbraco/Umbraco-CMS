using System;
using System.Collections;
using System.IO;
using System.Web;
using Umbraco.Core;
using umbraco;

namespace Umbraco.Web
{
	/// <summary>
	/// A helper class that provides many useful methods and functionality for using Umbraco in templates
	/// </summary>
	public class UmbracoHelper
	{
		private readonly UmbracoContext _umbracoContext;

		internal UmbracoHelper(UmbracoContext umbracoContext)
		{
			_umbracoContext = umbracoContext;
		}


		#region RenderMacro

		/// <summary>
		/// Renders the macro with the specified alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias)
		{
			return RenderMacro(alias, new { });
		}

		/// <summary>
		/// Renders the macro with the specified alias, passing in the specified parameters.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias, object parameters)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			var containerPage = new FormlessPage();
			var m = macro.GetMacro(alias);
			if (_umbracoContext.PageId == null)
			{
				throw new InvalidOperationException("Cannot render a macro when UmbracoContext.PageId is null.");
			}
			if (_umbracoContext.DocumentRequest == null)
			{
				throw new InvalidOperationException("Cannot render a macro when there is no current DocumentRequest.");
			}
			var macroProps = new Hashtable();
			foreach(var i in parameters.ToDictionary<object>())
			{
				//TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method of macro.cs 
				// looks for a lower case match. WTF. the whole macro concept needs to be rewritten.
				macroProps.Add(i.Key.ToLower(), i.Value);
			}			
			var macroControl = m.renderMacro(macroProps, 
				UmbracoContext.Current.DocumentRequest.UmbracoPage.Elements, 
				_umbracoContext.PageId.Value);
			containerPage.Controls.Add(macroControl);
			var output = new StringWriter();
			_umbracoContext.HttpContext.Server.Execute(containerPage, output, false);			
			return new HtmlString(output.ToString());
		}

		#endregion
	}
}
