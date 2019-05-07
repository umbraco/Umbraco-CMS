using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Represents a link (e.g. to content, media or an external URL).
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the type of link.
        /// </summary>
        /// <value>
        /// The type of link.
        /// </value>
        public LinkType Type { get; set; }

        /// <summary>
        /// Gets or sets the UDI.
        /// </summary>
        /// <value>
        /// The UDI.
        /// </value>
        public Udi Udi { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public IPublishedContent Content { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }
    }
}
