using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.presentation.ClientDependency;

namespace umbraco.presentation.umbraco
{

	[ClientDependency(1, ClientDependencyType.Javascript, "Application/NamespaceManager.js", "UmbracoClient")]
	[ClientDependency(0, ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
	[ClientDependency(0, ClientDependencyType.Css, "css/umbracoGui.css", "UmbracoRoot", CompositeGroupName = "CSSFiles")]
	[ClientDependency(0, ClientDependencyType.Css, "css/treeIcons.css", "UmbracoRoot", CompositeGroupName = "CSSFiles")]	
	[ClientDependency(0, ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient", CompositeGroupName = "JQueryUI")]
	[ClientDependency(1, ClientDependencyType.Javascript, "ui/accordian.js", "UmbracoClient", CompositeGroupName = "JQueryUI")]
	[ClientDependency(2, ClientDependencyType.Javascript, "ui/jQueryWresize.js", "UmbracoClient", CompositeGroupName = "JQueryUI")]	
	public partial class ClientDependencyTest : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			DataBind();
		}
	}
}
