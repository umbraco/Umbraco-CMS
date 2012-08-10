using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web
{
	/// <summary>
	/// An object resolver to return the IContentStore
	/// </summary>
	internal class ContentStoreResolver : SingleObjectResolverBase<ContentStoreResolver, IContentStore>
	{
		internal ContentStoreResolver(IContentStore contentStore)
		{
			Value = contentStore;
		} 
	
		/// <summary>
		/// Can be used by developers at runtime to set their IContentStore at app startup
		/// </summary>
		/// <param name="contentStore"></param>
		public void SetContentStore(IContentStore contentStore)
		{
			Value = contentStore;
		}

		/// <summary>
		/// Returns the IContentStore
		/// </summary>
		public IContentStore ContentStore
		{
			get { return Value; }
		}
	}
}