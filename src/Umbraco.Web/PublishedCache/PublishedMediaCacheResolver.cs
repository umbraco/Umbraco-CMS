using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.PublishedCache
{
	/// <summary>
	/// Resolves the IPublicMediaCache object.
	/// </summary>
	internal sealed class PublishedMediaCacheResolver : SingleObjectResolverBase<PublishedMediaCacheResolver, IPublishedMediaCache>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedMediaCacheResolver"/> class with a media cache.
        /// </summary>
        /// <param name="publishedMediaCache">The media cache.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PublishedMediaCacheResolver(IPublishedMediaCache publishedMediaCache)
			: base(publishedMediaCache)
		{ }

        /// <summary>
        /// Sets the media cache.
        /// </summary>
        /// <param name="publishedMediaCache">The media cache.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetContentCache(IPublishedMediaCache publishedMediaCache)
		{
			Value = publishedMediaCache;
		}

		/// <summary>
		/// Gets the media cache.
		/// </summary>
		public IPublishedMediaCache PublishedMediaCache
		{
			get { return Value; }
		}
	}
}