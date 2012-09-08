using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web
{
	/// <summary>
	/// An object resolver to return the IContentStore
	/// </summary>
	internal class PublishedContentStoreResolver : SingleObjectResolverBase<PublishedContentStoreResolver, IPublishedContentStore>
	{
		internal PublishedContentStoreResolver(IPublishedContentStore publishedContentStore)
			: base(publishedContentStore)
		{
		}

		/// <summary>
		/// Can be used by developers at runtime to set their IContentStore at app startup
		/// </summary>
		/// <param name="publishedContentStore"></param>
		public void SetContentStore(IPublishedContentStore publishedContentStore)
		{
			Value = publishedContentStore;
		}

		/// <summary>
		/// Returns the IContentStore
		/// </summary>
		public IPublishedContentStore PublishedContentStore
		{
			get { return Value; }
		}
	}
}