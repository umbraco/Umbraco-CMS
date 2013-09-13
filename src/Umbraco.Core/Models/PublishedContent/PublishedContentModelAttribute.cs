using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Indicates that the class is a published content model for a specified content type.
    /// </summary>
    /// <remarks>By default, the name of the class is assumed to be the content type alias. The
    /// <c>PublishedContentModelAttribute</c> can be used to indicate a different alias.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PublishedContentModelAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentModelAttribute"/> class with a content type alias.
        /// </summary>
        /// <param name="contentTypeAlias">The content type alias.</param>
        public PublishedContentModelAttribute(string contentTypeAlias)
        {
            if (string.IsNullOrWhiteSpace(contentTypeAlias))
                throw new ArgumentException("Argument cannot be null nor empty.", "contentTypeAlias");
            ContentTypeAlias = contentTypeAlias;
        }

        /// <summary>
        /// Gets or sets the content type alias.
        /// </summary>
        public string ContentTypeAlias { get; private set; }
    }
}
