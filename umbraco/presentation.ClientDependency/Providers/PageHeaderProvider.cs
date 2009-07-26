using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Linq;

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
			if (jsDependencies.Count == 0)
				return;
			
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
			if (DependantControl.Page.Header == null)
				throw new NullReferenceException("PageHeaderProvider requires a runat='server' tag in the page's header tag");

			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", js));
			AddToHead(string.Format(ScriptEmbed, js));
		}

		protected override void RegisterCssFiles(List<IClientDependencyFile> cssDependencies)
		{
			if (cssDependencies.Count == 0)
				return;

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
			if (DependantControl.Page.Header == null)
				throw new NullReferenceException("PageHeaderProvider requires a runat='server' tag in the page's header tag");
			DependantControl.Page.Trace.Write("ClientDependency", string.Format("Registering: {0}", css));
			AddToHead(string.Format(CssEmbed, css));
		}

		/// <summary>
		/// inserts the dependencies at the top of the head so needs to search the head
		/// controls to find out where to insert.
		/// </summary>
		/// <param name="literal"></param>
		private void AddToHead(string literal)
		{
			List<int> indexes = new List<int>();
			Type iDependency = typeof(IClientDependencyFile);
			foreach (Control ctl in DependantControl.Page.Header.Controls)
			{
				if (ctl.ID != null && ctl.ID.StartsWith("CD_"))
					indexes.Add(DependantControl.Page.Header.Controls.IndexOf(ctl));
			}
			//now that we have all of the indexes of the client dependencies, we need to insert
			//the next one after the largest index
			int newIndex = indexes.Count == 0 ? 0 : indexes.Max() + 1;
			LiteralControl dCtl = new LiteralControl(literal);
			dCtl.ID = "CD_" + newIndex.ToString();
			if (newIndex >= DependantControl.Page.Header.Controls.Count)
				DependantControl.Page.Header.Controls.Add(dCtl);
			else
				DependantControl.Page.Header.Controls.AddAt(newIndex, dCtl);


		}
	}
}
