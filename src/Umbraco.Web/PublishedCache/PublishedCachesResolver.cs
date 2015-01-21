using System;
using System.Linq.Expressions;
using Umbraco.Core.LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.PublishedCache
{
    //TODO: REmove this requirement, just use normal IoC and publicize IPublishedCaches

	/// <summary>
	/// Resolves the IPublishedCaches object.
	/// </summary>
	internal sealed class PublishedCachesResolver : ContainerSingleObjectResolver<PublishedCachesResolver, IPublishedCaches>
	{
	    /// <summary>
	    /// Initializes the resolver to use IoC
	    /// </summary>
	    /// <param name="container"></param>
	    /// <param name="implementationType"></param>
	    public PublishedCachesResolver(IServiceContainer container, Type implementationType) : base(container, implementationType)
	    {
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCachesResolver"/> class with caches.
        /// </summary>
        /// <param name="caches">The caches.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PublishedCachesResolver(IPublishedCaches caches)
			: base(caches)
		{ }

	    /// <summary>
	    /// Initializes the resolver to use IoC
	    /// </summary>
	    /// <param name="container"></param>
	    /// <param name="implementationType"></param>
	    public PublishedCachesResolver(IServiceContainer container, Expression<Func<IServiceFactory, IPublishedCaches>> implementationType) : base(container, implementationType)
	    {
	    }

	    /// <summary>
        /// Sets the caches.
        /// </summary>
        /// <param name="caches">The caches.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetCaches(IPublishedCaches caches)
		{
			Value = caches;
		}

		/// <summary>
		/// Gets the caches.
		/// </summary>
		public IPublishedCaches Caches
		{
			get { return Value; }
		}
	}
}