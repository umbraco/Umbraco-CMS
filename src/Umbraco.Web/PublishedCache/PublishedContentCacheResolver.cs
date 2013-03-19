using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.PublishedCache
{
	/// <summary>
	/// Resolves the IPublishedContentCache object.
	/// </summary>
	internal sealed class PublishedContentCacheResolver : SingleObjectResolverBase<PublishedContentCacheResolver, IPublishedContentCache>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentCacheResolver"/> class with a content cache.
        /// </summary>
        /// <param name="publishedContentCache">The content cache.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PublishedContentCacheResolver(IPublishedContentCache publishedContentCache)
			: base(publishedContentCache)
		{ }

        /// <summary>
        /// Sets the content cache.
        /// </summary>
        /// <param name="contentCache">The content cache.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetContentCache(IPublishedContentCache contentCache)
		{
			Value = contentCache;
		}

		/// <summary>
		/// Gets the content cache.
		/// </summary>
		public IPublishedContentCache ContentCache
		{
			get { return Value; }
		}
	}
}