using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.umbraco.controls
{
	public partial class ProgressBar : System.Web.UI.UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			DataBind();
		}
	}
}