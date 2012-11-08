using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web
{
	/// <summary>
	/// An object resolver to return the IPublishedMediaStore
	/// </summary>
	internal class PublishedMediaStoreResolver : SingleObjectResolverBase<PublishedMediaStoreResolver, IPublishedMediaStore>
	{
		internal PublishedMediaStoreResolver(IPublishedMediaStore publishedMediaStore)
			: base(publishedMediaStore)
		{
		}

		/// <summary>
		/// Can be used by developers at runtime to set their IContentStore at app startup
		/// </summary>
		/// <param name="publishedMediaStore"></param>
		public void SetContentStore(IPublishedMediaStore publishedMediaStore)
		{
			Value = publishedMediaStore;
		}

		/// <summary>
		/// Returns the IContentStore
		/// </summary>
		public IPublishedMediaStore PublishedMediaStore
		{
			get { return Value; }
		}
	}
}