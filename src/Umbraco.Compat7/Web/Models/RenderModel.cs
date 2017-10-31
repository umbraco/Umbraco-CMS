using System;
using System.Globalization;
using Umbraco.Core.Models.PublishedContent;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Models
{
    public class RenderModel : IRenderModel
    {
        public RenderModel(IPublishedContent content, CultureInfo culture)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            Content = content;
            CurrentCulture = culture;
        }

        public RenderModel(IPublishedContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (UmbracoContext.Current == null)
            {
                throw new InvalidOperationException("Cannot construct a RenderModel without specifying a CultureInfo when no UmbracoContext has been initialized");
            }
            Content = content;
            CurrentCulture = UmbracoContext.Current.PublishedRequest.Culture;
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
