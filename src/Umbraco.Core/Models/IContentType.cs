using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a ContentType, which Content is based on
    /// </summary>
    public interface IContentType : IContentTypeComposition
    {
        /// <summary>
        /// Gets the default Template of the ContentType
        /// </summary>
        ITemplate DefaultTemplate { get; }

        /// <summary>
        /// Gets or Sets a list of Templates which are allowed for the ContentType
        /// </summary>
        IEnumerable<ITemplate> AllowedTemplates { get; set; }
    }
}