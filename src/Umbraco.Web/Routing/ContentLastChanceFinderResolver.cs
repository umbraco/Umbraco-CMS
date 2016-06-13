using System;
using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{

    /// <summary>
    /// Resolves the last chance IPublishedContentFinder object.
    /// </summary>
    public sealed class ContentLastChanceFinderResolver : ContainerSingleObjectResolver<ContentLastChanceFinderResolver, IContentFinder>
    {
        /// <summary>
        /// Initializes the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        /// <param name="implementationType"></param>
        internal ContentLastChanceFinderResolver(IServiceContainer container, Type implementationType)
            : base(container, implementationType)
        { }
   
        /// <summary>
        /// Sets the last chance finder.
        /// </summary>
        /// <param name="finder">The finder.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetFinder(IContentFinder finder)
        {
            Value = finder;
        }

        /// <summary>
        /// Gets the last chance finder.
        /// </summary>
        public IContentFinder Finder => Value;
    }
}