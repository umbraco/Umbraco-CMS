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
        private readonly IPropertySet _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySetWrapped"/> class
        /// with an <c>IPropertySet<c> instance to wrap.</c>
        /// </summary>
        /// <param name="content">The content to wrap.</param>
        protected PropertySetWrapped(IPropertySet content)
        {
            _content = content;
        }

        /// <summary>
        /// Gets the wrapped content.
        /// </summary>
        /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
        public IPropertySet Unwrap() => _content;

        /// <inheritdoc />
        public PublishedContentType ContentType => _content.ContentType;

        /// <inheritdoc />
        public Guid Key => _content.Key;

        /// <inheritdoc />
        public IEnumerable<IPublishedProperty> Properties => _content.Properties;

        /// <inheritdoc />
        public IPublishedProperty GetProperty(string alias) => _content.GetProperty(alias);
    }
}
