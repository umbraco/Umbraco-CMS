/*
 * Created by SharpDevelop.
 * User: spocke
 * Date: 2007-11-22
 * Time: 14:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Web;

namespace umbraco.editorControls.tinyMCE3.webcontrol.plugin {
	/// <summary>
	/// Description of IAction.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public interface IModule
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		void ProcessRequest(HttpContext context);
	}
}
