using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.presentation.ClientDependency.Controls
{
	public class CssInclude : ClientDependencyInclude
	{

		public CssInclude()
		{
			DependencyType = ClientDependencyType.Css;
		}
		public CssInclude(IClientDependencyFile file)
			: base(file)
		{
			DependencyType = ClientDependencyType.Css;
		}
	}
}
