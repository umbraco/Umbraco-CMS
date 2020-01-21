using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Represents the model for the current Umbraco view.
    /// </summary>
    public class ContentModel : IContentModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentModel"/> class with a content.
        /// </summary>
        /// <param name="content"></param>
        public ContentModel(IPublishedContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            Content = content;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public IPublishedContent Content { get; }
    }
}
