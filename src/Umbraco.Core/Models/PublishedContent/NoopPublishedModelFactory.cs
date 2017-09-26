using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <inheritdoc />
    /// <summary>Represents a no-operation factory.</summary>
    public class NoopPublishedModelFactory : IPublishedModelFactory
    {
        /// <inheritdoc />
        public IPublishedElement CreateModel(IPublishedElement element) => element;

        /// <inheritdoc />
        public Dictionary<string, Type> ModelTypeMap { get; } = new Dictionary<string, Type>();
    }
}
