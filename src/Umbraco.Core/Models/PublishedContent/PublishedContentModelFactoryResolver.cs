using System;
using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Resolves the IPublishedContentModelFactory object.
    /// </summary>
    public class PublishedContentModelFactoryResolver : ContainerSingleObjectResolver<PublishedContentModelFactoryResolver, IPublishedContentModelFactory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentModelFactoryResolver"/>.
        /// </summary>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PublishedContentModelFactoryResolver()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentModelFactoryResolver"/> with a factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PublishedContentModelFactoryResolver(IPublishedContentModelFactory factory)
            : base(factory)
        { }

        /// <summary>
        /// Initialize the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        internal PublishedContentModelFactoryResolver(IServiceContainer container)
            : base(container)
        { }

        /// <summary>
        /// Sets the factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetFactory(IPublishedContentModelFactory factory)
        {
            Value = factory;
        }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        public IPublishedContentModelFactory Factory
        {
            get { return Value; }
        }
    }
}