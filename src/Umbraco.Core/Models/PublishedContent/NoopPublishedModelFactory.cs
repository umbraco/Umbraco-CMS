using System;
using System.Collections;
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
        public IList CreateModelList(string alias) => new List<IPublishedElement>();

        /// <inheritdoc />
        public Type MapModelType(Type type) => typeof(IPublishedElement);
    }
}
