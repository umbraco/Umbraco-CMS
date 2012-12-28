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

        /// <summary>
        /// Sets the default template for the ContentType
        /// </summary>
        /// <param name="template">Default <see cref="ITemplate"/></param>
        void SetDefaultTemplate(ITemplate template);

        /// <summary>
        /// Removes a template from the list of allowed templates
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to remove</param>
        /// <returns>True if template was removed, otherwise False</returns>
        bool RemoveTemplate(ITemplate template);
    }
}