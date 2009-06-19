using System;

namespace umbraco.presentation.cache
{

    /// <summary>
    /// pageRefresher is the standard CacheRefresher used by Load-Balancing in Umbraco.
    /// 
    /// If Load balancing is enabled (by default disabled, is set in umbracoSettings.config) pageRefresher will be called
    /// everytime content is added/updated/removed to ensure that the content cache is identical on all load balanced servers
    /// 
    /// pageRefresger inherits from interfaces.ICacheRefresher.
    /// </summary>
	public class pageRefresher : interfaces.ICacheRefresher
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="pageRefresher"/> class.
        /// </summary>
		public pageRefresher()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region ICacheRefresher Members

        /// <summary>
        /// Gets the unique identifier of the CacheRefresher.
        /// </summary>
        /// <value>The unique identifier.</value>
		public Guid UniqueIdentifier
		{
			get
			{
				// TODO:  Add pageRefresher.uniqueIdentifier getter implementation
				return new Guid("27AB3022-3DFA-47b6-9119-5945BC88FD66");
			}
		}

        /// <summary>
        /// Gets the name of the CacheRefresher
        /// </summary>
        /// <value>The name.</value>
		public string Name 
		{
			get {return "Page Refresher (umbraco.library wrapper)";}
		}

        /// <summary>
        /// Refreshes all nodes in umbraco.
        /// </summary>
		public void RefreshAll()
		{
			// library.RePublishNodesDotNet(-1, true);
            content.Instance.RefreshContentFromDatabaseAsync();
		}

        /// <summary>
        /// Not used with content.
        /// </summary>
        /// <param name="Id">The id.</param>
		public void Refresh(Guid Id)
		{
			// Not used when pages
		}

        /// <summary>
        /// Refreshes the cache for the node with specified id
        /// </summary>
        /// <param name="Id">The id.</param>
        public void Refresh(int Id) {
            content.Instance.PublishNode(Id);
        }


        /// <summary>
        /// Removes the node with the specified id from the cache
        /// </summary>
        /// <param name="Id">The id.</param>
        public void Remove(int Id) {
            content.Instance.UnPublishNode(Id);
        }

    		#endregion
	}
}
