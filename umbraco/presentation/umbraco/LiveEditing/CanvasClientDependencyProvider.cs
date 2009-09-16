using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;
using umbraco.presentation;

namespace umbraco.presentation.LiveEditing
{
	public class CanvasClientDependencyProvider : LazyLoadProvider
	{
		protected override void ProcessSingleCssFile(string css)
		{
			if (UmbracoContext.Current.LiveEditingContext.Enabled)
				base.ProcessSingleCssFile(css);
		}

		protected override void ProcessSingleJsFile(string js)
		{
			if (UmbracoContext.Current.LiveEditingContext.Enabled)
				base.ProcessSingleJsFile(js);
		}

		//protected override void RegisterCssFiles(List<ClientDependency.Core.IClientDependencyFile> cssDependencies)
		//{
		//    throw new NotImplementedException();
		//}

		//protected override void RegisterJsFiles(List<ClientDependency.Core.IClientDependencyFile> jsDependencies)
		//{
		//    throw new NotImplementedException();
		//}
	}
}
