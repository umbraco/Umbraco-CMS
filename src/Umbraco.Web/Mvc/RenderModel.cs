using System.Globalization;
using System.Xml;
using Umbraco.Core.Models;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Represents the model for the current rendering page in Umbraco
	/// </summary>
	public class RenderModel
	{	
		/// <summary>
		/// Returns the current IPublishedContent object
		/// </summary>
		public IPublishedContent Content { get; internal set; }

		/// <summary>
		/// Returns the current Culture assigned to the page being rendered
		/// </summary>
		public CultureInfo CurrentCulture { get; internal set; }
	}
}