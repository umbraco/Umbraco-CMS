using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.presentation.ClientDependency.Providers
{
	public class PageHeaderProvider : ClientDependencyProvider
	{		

		public const string DefaultName = "PageHeaderProvider";

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{			
			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name))
				name = DefaultName;

			base.Initialize(name, config);
		}

		protected override void RegisterJsFiles(List<IClientDependencyFile> jsDependencies)
		{			
			string js = "<script type=\"text/javascript\" src=\"{0}\"></script>";
			foreach (IClientDependencyFile dependency in jsDependencies)
			{
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", dependency.FilePath));
				DependantControl.Page.Header.Controls.Add(new LiteralControl(string.Format(js, dependency.FilePath)));
			}
		}

		protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
		{
			string css = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
			foreach (IClientDependencyFile dependency in cssDependencies)
			{
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", dependency.FilePath));
				DependantControl.Page.Header.Controls.Add(new LiteralControl(string.Format(css, dependency.FilePath)));
			}
		}
	}
}
