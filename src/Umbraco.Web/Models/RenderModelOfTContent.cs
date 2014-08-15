using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Models
{
    public class RenderModel<TContent> : RenderModel
        where TContent : IPublishedContent
    {
        /// <summary>
        /// Constructor specifying both the IPublishedContent and the CultureInfo
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        public RenderModel(TContent content, CultureInfo culture)
            : base(content, culture)
        {
            Content = content;
        }

        /// <summary>
        /// Constructor to set the IPublishedContent and the CurrentCulture is set by the UmbracoContext
        /// </summary>
        /// <param name="content"></param>
        public RenderModel(TContent content)
            : base(content)
        {
            Content = content;
        }

        /// <summary>
        /// Returns the current IPublishedContent object
        /// </summary>
        public new TContent Content { get; private set; }
    }
}
