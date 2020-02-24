using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides an abstract base class for <c>IPublishedElement</c> implementations that
    /// wrap and extend another <c>IPublishedElement</c>.
    /// </summary>
    public abstract class PublishedElementWrapped : IPublishedElement
    {
        private readonly IPublishedElement _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedElementWrapped"/> class
        /// with an <c>IPublishedElement</c> instance to wrap.
        /// </summary>
        /// <param name="content">The content to wrap.</param>
        protected PublishedElementWrapped(IPublishedElement content)
        {
            _content = content;
        }

        /// <summary>
        /// Gets the wrapped content.
        /// </summary>
        /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
        public IPublishedElement Unwrap() => _content;

        /// <inheritdoc />
        public IPublishedContentType ContentType => _content.ContentType;

        /// <inheritdoc />
        public Guid Key => _content.Key;

        /// <inheritdoc />
        public IEnumerable<IPublishedProperty> Properties => _content.Properties;

        /// <inheritdoc />
        public IPublishedProperty GetProperty(string alias) => _content.GetProperty(alias);
    }
}
