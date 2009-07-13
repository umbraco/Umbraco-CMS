using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.presentation.ClientDependency
{
	public interface IClientDependencyFile
	{
		string FilePath { get; set; }
		ClientDependencyType DependencyType { get; set; }
		string InvokeJavascriptMethodOnLoad { get; set; }
		int Priority { get; set; }
        //string CompositeGroupName { get; set; }
        string PathNameAlias { get; set; }
		bool DoNotOptimize { get; set; }
	}
}
