using System;
using System.Xml;

namespace umbraco.interfaces
{
	/// <summary>
	/// Interface for created application trees in the umbraco backoffice
	/// </summary>
	public interface ITree : IDiscoverable
    {
        /// <summary>
        /// Sets the tree id.
        /// </summary>
        /// <value>The id.</value>
		int id{set;}
        /// <summary>
        /// Sets the applicatin alias.
        /// </summary>
        /// <value>The app.</value>
		String app {set;}
        /// <summary>
        /// Renders the specified tree.
        /// </summary>
        /// <param name="Tree">The tree.</param>
		void Render(ref XmlDocument Tree);
        /// <summary>
        /// Renders the client side script associatied with the tree.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
		void RenderJS(ref System.Text.StringBuilder Javascript);
	}
}
