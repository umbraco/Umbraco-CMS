using System;
using System.Xml;
namespace umbraco.interfaces
{

    /// <summary>
    /// Umbraco form handler interface. Has a simple execute statement and a redirect ID.
    /// </summary>
	public interface IFormhandler
	{
        /// <summary>
        /// Executes the specified form handler node.
        /// </summary>
        /// <param name="formHandlerNode">The form handler node.</param>
        /// <returns></returns>
		bool Execute(XmlNode formHandlerNode);
        /// <summary>
        /// Gets the redirect ID.
        /// </summary>
        /// <value>The redirect ID.</value>
		int redirectID {get;}
	}
}
