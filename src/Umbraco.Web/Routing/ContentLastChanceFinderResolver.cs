using System;
using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{

    /// <summary>
    /// Resolves the last chance IPublishedContentFinder object.
    /// </summary>
    public sealed class ContentLastChanceFinderResolver : ContainerSingleObjectResolver<ContentLastChanceFinderResolver, IContentLastChanceFinder>
    {
        /// <summary>
        /// Initializes the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        internal ContentLastChanceFinderResolver(IServiceContainer container)
            : base(container)
        { }
   
        /// <summary>
        /// Sets the last chance finder.
        /// </summary>
        /// <param name="finder">The finder.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetFinder(IContentLastChanceFinder finder)
        {
            Value = finder;
        }

        /// <summary>
        /// Gets the last chance finder.
        /// </summary>
        public IContentFinder Finder => Value;
    }
}