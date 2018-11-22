using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.PublishedCache
{
	/// <summary>
	/// Resolves the IPublishedCaches object.
	/// </summary>
	internal sealed class PublishedCachesResolver : SingleObjectResolverBase<PublishedCachesResolver, IPublishedCaches>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCachesResolver"/> class with caches.
        /// </summary>
        /// <param name="caches">The caches.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PublishedCachesResolver(IPublishedCaches caches)
			: base(caches)
		{ }

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