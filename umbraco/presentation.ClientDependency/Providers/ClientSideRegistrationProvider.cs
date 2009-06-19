using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace umbraco.presentation.ClientDependency.Providers
{
	public class ClientSideRegistrationProvider : ClientDependencyProvider
	{

		public const string DefaultName = "ClientSideRegistrationProvider";

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name))
				name = DefaultName;

			base.Initialize(name, config);
		}

		/// <summary>Path to the dependency loader we need for adding control dependencies.</summary>
		protected const string DependencyLoaderResourceName = "umbraco.presentation.ClientDependency.UmbracoDependencyLoader.js";		

		protected override void RegisterJsFiles(List<IClientDependencyFile> jsDependencies)
		{
			if (jsDependencies.Count == 0)
				return;

			DetectScriptManager();
			RegisterDependencyLoader();
						
			StringBuilder dependencyCalls = new StringBuilder("UmbDependencyLoader");
			foreach (IClientDependencyFile dependency in jsDependencies)
			{
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", dependency.FilePath));
				dependencyCalls.AppendFormat(".AddJs('{0}','{1}')",
											 dependency.FilePath, dependency.InvokeJavascriptMethodOnLoad);
			}
			dependencyCalls.Append(';');
			ScriptManager.RegisterClientScriptBlock(DependantControl, DependantControl.GetType(), jsDependencies.GetHashCode().ToString(),
													dependencyCalls.ToString(), true);
		}

		protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
		{
			if (cssDependencies.Count == 0)
				return;

			DetectScriptManager();
			RegisterDependencyLoader();

			StringBuilder dependencyCalls = new StringBuilder("UmbDependencyLoader");
			foreach (IClientDependencyFile dependency in cssDependencies)
			{
				DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", dependency.FilePath));
				dependencyCalls.AppendFormat(".AddCss('{0}')", dependency.FilePath);
			}
			dependencyCalls.Append(';');
			ScriptManager.RegisterClientScriptBlock(DependantControl, DependantControl.GetType(), cssDependencies.GetHashCode().ToString(),
													dependencyCalls.ToString(), true);
		}

		private void RegisterDependencyLoader()
		{
			// register loader script
			if (!HttpContext.Current.Items.Contains(DependencyLoaderResourceName))
			{
				DependantControl.Page.ClientScript.RegisterClientScriptResource(typeof(ClientDependencyHelper), DependencyLoaderResourceName);
				HttpContext.Current.Items[DependencyLoaderResourceName] = true;
			}
		}

		private void DetectScriptManager()
		{
			try
			{
				Page pg = (Page)HttpContext.Current.CurrentHandler;
				if (ScriptManager.GetCurrent(pg) == null)
					throw new NullReferenceException("A ScriptManager needs to be declared on the page for ClientDependencyLoader to work");
			}
			catch
			{
				throw new NullReferenceException("ClientDependencyLoader only works with an active Page");
			}
		}
	}
}
