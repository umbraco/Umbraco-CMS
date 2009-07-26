using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using umbraco.presentation.ClientDependency.Providers;
using umbraco.presentation.ClientDependency.Config;

namespace umbraco.presentation.ClientDependency.Controls
{
	[ParseChildren(typeof(ClientDependencyPath), ChildrenAsProperties = true)]
	public class ClientDependencyLoader : Control
	{
		/// <summary>
		/// Constructor sets the defaults.
		/// </summary>
		public ClientDependencyLoader()
		{
			Paths = new ClientDependencyPathCollection();
			EmbedType = ClientDependencyEmbedType.Header;
            IsDebugMode = false;
		}

		private const string ContextKey = "ClientDependencyLoader";

		public static void RegisterDependency(string filePath, ClientDependencyType type)
		{
			RegisterDependency(filePath, "", type);
		}

		public static void RegisterDependency(string filePath, string pathNameAlias, ClientDependencyType type)
		{
			RegisterDependency(ClientDependencyInclude.DefaultPriority, false, filePath, pathNameAlias, type, "");
		}

		/// <summary>
		/// Dynamically registers a dependency into the loader at runtime.
		/// This is similar to ScriptManager.RegisterClientScriptInclude
		/// </summary>
		/// <param name="file"></param>
		public static void RegisterDependency(int priority, bool doNotOptimize, string filePath, string pathNameAlias, ClientDependencyType type, string invokeJavascriptMethodOnLoad)
		{
			if (!HttpContext.Current.Items.Contains(ContextKey))
				throw new NullReferenceException("No ClientDependencyLoader found in the current request");

			ClientDependencyLoader loader = HttpContext.Current.Items[ContextKey] as ClientDependencyLoader;
			if (loader == null)
				throw new Exception("Could not find a ClientDependencyLoader in the current request");

			//loader.RegisteredFiles.Add(file);
			//create/add a new control to this control's collection
			if (type == ClientDependencyType.Css)
			{
				CssInclude cssInc = new CssInclude();
				cssInc.DoNotOptimize = doNotOptimize;
				cssInc.Priority = priority;
				cssInc.FilePath = filePath;
				cssInc.PathNameAlias = pathNameAlias;
				cssInc.InvokeJavascriptMethodOnLoad = invokeJavascriptMethodOnLoad;
				loader.Controls.Add(cssInc);
			}
			else
			{
				JsInclude jsInc = new JsInclude();
				jsInc.DoNotOptimize = doNotOptimize;
				jsInc.Priority = priority;
				jsInc.FilePath = filePath;
				jsInc.PathNameAlias = pathNameAlias;
				jsInc.InvokeJavascriptMethodOnLoad = invokeJavascriptMethodOnLoad;
				loader.Controls.Add(jsInc);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (HttpContext.Current.Items.Contains(ContextKey))
			{
				throw new Exception("Only one ClientDependencyLoader may exist on a page");
			}
			else
			{
				HttpContext.Current.Items.Add(ContextKey, this);
			}
		}

		/// <summary>
		/// Need to set the container for each of the paths to support databinding.
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			foreach (ClientDependencyPath path in Paths)
			{
				path.Parent = this;
			}	
		}

		/// <summary>
		/// Need to bind all children paths.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			foreach (ClientDependencyPath path in Paths)
			{
				path.DataBind();
			}				
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			foreach (ClientDependencyPath path in Paths)
			{
				Page.Trace.Write("ClientDependency", string.Format("Path loaded: {0}", path.Path));
			}
            ClientDependencyProvider provider = null;
			switch (EmbedType)
			{
				case ClientDependencyEmbedType.Header:
					provider = ClientDependencySettings.Instance.ProviderCollection[PageHeaderProvider.DefaultName];
                    provider.IsDebugMode = IsDebugMode;
					ClientDependencyHelper.RegisterClientDependencies(provider, this.Page, Paths);
					break;
				case ClientDependencyEmbedType.ClientSideRegistration:
					provider = ClientDependencySettings.Instance.ProviderCollection[ClientSideRegistrationProvider.DefaultName];
                    provider.IsDebugMode = IsDebugMode;
					ClientDependencyHelper.RegisterClientDependencies(provider, this.Page, Paths);
					break;
			}
			
		}
				

		[PersistenceMode(PersistenceMode.InnerProperty)]
		public ClientDependencyPathCollection Paths { get; private set; }
		public ClientDependencyEmbedType EmbedType { get; set; }
        public bool IsDebugMode { get; set; }


		private List<ClientDependencyAttribute> m_Dependencies = new List<ClientDependencyAttribute>();
	}

	

	
}
