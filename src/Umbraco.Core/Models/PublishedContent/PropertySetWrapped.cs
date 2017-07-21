using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides an abstract base class for <c>IPropertySet</c> implementations that
    /// wrap and extend another <c>IPropertySet</c>.
    /// </summary>
    public abstract class PropertySetWrapped : IPropertySet
    {
        protected readonly IPropertySet Content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySetWrapped"/> class
        /// with an <c>IPropertySet<c> instance to wrap.</c>
        /// </summary>
        /// <param name="content">The content to wrap.</param>
        protected PropertySetWrapped(IPropertySet content)
        {
            Content = content;
        }

        /// <summary>
        /// Gets the wrapped content.
        /// </summary>
        /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
        public IPropertySet Unwrap() => Content;

        /// <inheritdoc />
        public PublishedContentType ContentType => Content.ContentType;

        /// <inheritdoc />
        public Guid Key => Content.Key;

        /// <inheritdoc />
        public IEnumerable<IPublishedProperty> Properties => Content.Properties;

        /// <inheritdoc />
        public IPublishedProperty GetProperty(string alias) => Content.GetProperty(alias);
    }
}
