using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.IO;

namespace Umbraco.Web.UI.Dashboard
{
    public partial class UserControlProxy : Pages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var path = Request.QueryString["ctrl"];
            if (!string.IsNullOrEmpty(path))
            {
                path = IOHelper.FindFile(path);

                try
                {
                    Control c = LoadControl(path);
                    container.Controls.Add(c);
                }
                catch (Exception ee)
                {
                    container.Controls.Add(
                        new LiteralControl(
                            "<p class=\"umbracoErrorMessage\">Could not load control: '" + path +
                            "'. <br/><span class=\"guiDialogTiny\"><strong>Error message:</strong> " +
                            ee.ToString() + "</span></p>"));
                }
            }
        }
    }
}