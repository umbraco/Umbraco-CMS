using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using umbraco.presentation.ClientDependency.Providers;

namespace umbraco.presentation.ClientDependency
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
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (HttpContext.Current.Items.Contains("ClientDependencyLoader"))
			{
				throw new Exception("Only one ClientDependencyLoader may exist on a page");
			}
			else
			{
				HttpContext.Current.Items.Add("ClientDependencyLoader", true);
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
			switch (EmbedType)
			{
				case ClientDependencyEmbedType.Header:
					ClientDependencyHelper.RegisterClientDependencies<PageHeaderProvider>(this.Page, Paths);
					break;
				case ClientDependencyEmbedType.ClientSideRegistration:
					ClientDependencyHelper.RegisterClientDependencies<ClientSideRegistrationProvider>(this.Page, Paths);
					break;
			}
			
		}
				

		[PersistenceMode(PersistenceMode.InnerProperty)]
		public ClientDependencyPathCollection Paths { get; private set; }
		public ClientDependencyEmbedType EmbedType { get; set; }


		private List<ClientDependencyAttribute> m_Dependencies = new List<ClientDependencyAttribute>();
	}

	

	
}
