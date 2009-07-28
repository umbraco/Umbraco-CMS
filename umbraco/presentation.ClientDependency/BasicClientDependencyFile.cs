using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.presentation.ClientDependency
{
	public class BasicClientDependencyFile : IClientDependencyFile
	{
		public BasicClientDependencyFile(ClientDependencyType type)
		{
			DependencyType = type;
		}

		#region IClientDependencyFile Members

		public string FilePath { get; set; }
		public ClientDependencyType DependencyType { get; private set; }
		public string InvokeJavascriptMethodOnLoad { get; set; }
		public int Priority { get; set; }
		public string PathNameAlias { get; set; }
		public bool DoNotOptimize { get; set; }

		#endregion
	}
}
