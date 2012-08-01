using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace umbraco.uicontrols
{
	[Obsolete("To register scripts and css files for controls, use the ClientDependency library")]
    public class helper
    {
		[Obsolete("To register scripts and css files for controls, use the ClientDependency library")]
        public static void AddLinkToHeader(string key, string src, Page page)
        {
			if (page.Header != null)
			{
			    if (!page.ClientScript.IsClientScriptBlockRegistered(page.GetType(), key))
			    {
			        page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, String.Empty);
			        ((HtmlHead)page.Header).Controls.Add(new LiteralControl(String.Format("<link rel=\"stylesheet\" href=\"{0}\" />", src)));
			    }
			}
			else
			{
                // can't add to header, will failback to a simple register script
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, String.Format("<link rel=\"stylesheet\" href=\"{0}\" />", src));
            }
        }

		[Obsolete("To register scripts and css files for controls, use the ClientDependency library")]
        public static void AddScriptToHeader(string key, string src, Page page)
        {
			if (page.Header != null)
			{
			    if (!page.ClientScript.IsClientScriptBlockRegistered(page.GetType(), key))
			    {
			        page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, String.Empty);
			        ((HtmlHead)page.Header).Controls.Add(new LiteralControl(String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", src)));
			    }
			}
			else
			{
                // can't add to header, will failback to a simple register script
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", src));
            }
        }
    }
}
