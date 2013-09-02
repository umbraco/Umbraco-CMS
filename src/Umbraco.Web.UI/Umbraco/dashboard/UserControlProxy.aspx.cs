using System;
using System.Web.UI;
using Umbraco.Core.IO;


namespace Umbraco.Web.UI.Umbraco.Dashboard
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
            if (string.IsNullOrEmpty(path) == false)
            {
                path = IOHelper.FindFile(path);

                try
                {
                    var c = LoadControl(path);
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