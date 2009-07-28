using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace umbraco.presentation.ClientDependency
{
	public class BasicClientDependencyPath : IClientDependencyPath
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string ResolvedPath
		{
			get
			{
				return (HttpContext.Current.CurrentHandler as Page).ResolveUrl(Path);
			}
		}
	}
}
