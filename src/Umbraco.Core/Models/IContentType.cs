using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a ContentType, which Content is based on
    /// </summary>
    public interface IContentType : IContentTypeComposition
    {
        /// <summary>
        /// Gets or Sets the path to the default Template of the ContentType
        /// </summary>
        string DefaultTemplate { get; set; }

        /// <summary>
        /// Gets or Sets a list of Template names/paths which are allowed for the ContentType
        /// </summary>
        IEnumerable<string> AllowedTemplates { get; set; }
    }
}