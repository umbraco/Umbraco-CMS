using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.presentation.ClientDependency.Controls
{
	public class JsInclude : ClientDependencyInclude
	{

		public JsInclude()
		{
			DependencyType = ClientDependencyType.Javascript;
		}
		public JsInclude(IClientDependencyFile file)
			: base(file)
		{
			DependencyType = ClientDependencyType.Javascript;
		}

	}
}
