using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Models
{
	/// <summary>
	/// Represents the model for the current rendering page in Umbraco
	/// </summary>
	public class RenderModel : IRenderModel
	{
		/// <summary>
		/// Constructor specifying both the IPublishedContent and the CultureInfo
		/// </summary>
		/// <param name="content"></param>
		/// <param name="culture"></param>
		public RenderModel(IPublishedContent content, CultureInfo culture)
		{
            if (content == null) throw new ArgumentNullException("content");
			if (culture == null) throw new ArgumentNullException("culture");
			Content = content;
			CurrentCulture = culture;
		}

		/// <summary>
		/// Constructor to set the IPublishedContent and the CurrentCulture is set by the UmbracoContext
		/// </summary>
		/// <param name="content"></param>
		public RenderModel(IPublishedContent content)
		{
            if (content == null) throw new ArgumentNullException("content");
			if (UmbracoContext.Current == null)
			{
				throw new InvalidOperationException("Cannot construct a RenderModel without specifying a CultureInfo when no UmbracoContext has been initialized");
			}
			Content = content;
			CurrentCulture = UmbracoContext.Current.PublishedContentRequest.Culture;
		}

		/// <summary>
		/// Returns the current IPublishedContent object
		/// </summary>
		public IPublishedContent Content { get; private set; }

		/// <summary>
		/// Returns the current Culture assigned to the page being rendered
		/// </summary>
		public CultureInfo CurrentCulture { get; private set; }
	}
}