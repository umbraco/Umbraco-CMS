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

namespace umbraco.presentation.plugins.tinymce3 {
	/// <summary>
	/// Description of IModule.
	/// </summary>
	public interface IModule {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		void ProcessRequest(HttpContext context);
	}
}
