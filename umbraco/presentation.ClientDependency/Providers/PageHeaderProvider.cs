using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.presentation.ClientDependency.Providers
{
	public class PageHeaderProvider : ClientDependencyProvider
	{		

		public const string DefaultName = "PageHeaderProvider";
		public const string ScriptEmbed = "<script type=\"text/javascript\" src=\"{0}\"></script>";
		public const string CssEmbed = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{			
			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name))
				name = DefaultName;

			base.Initialize(name, config);
		}

		protected override void RegisterJsFiles(List<IClientDependencyFile> jsDependencies)
		{			
			
			if (IsDebugMode)
			{
				foreach (IClientDependencyFile dependency in jsDependencies)
				{
					ProcessSingleJsFile(dependency.FilePath);
				}
			}
			else
			{
				List<string> jsList = ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript);
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Processed composite list: {0}", jsList[0]));
				foreach (string js in jsList)
				{
					ProcessSingleJsFile(js);
				}				
			}			
		}

		protected override void ProcessSingleJsFile(string js)
		{
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", js));
			DependantControl.Page.Header.Controls.Add(new LiteralControl(string.Format(ScriptEmbed, js)));
		}

		protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
		{
			if (IsDebugMode)
			{
				foreach (IClientDependencyFile dependency in cssDependencies)
				{
					ProcessSingleCssFile(dependency.FilePath);
				}
			}
			else
			{
				List<string> cssList = ProcessCompositeList(cssDependencies, ClientDependencyType.Css);
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Processed composite list: {0}", cssList[0]));
				foreach (string css in cssList)
				{
					ProcessSingleCssFile(css);
				}				
			}
		}

		protected override void ProcessSingleCssFile(string css)
		{
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", css));
			DependantControl.Page.Header.Controls.Add(new LiteralControl(string.Format(CssEmbed, css)));
		}
	}
}
