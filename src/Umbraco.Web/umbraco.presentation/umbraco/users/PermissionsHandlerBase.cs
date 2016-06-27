using System;
using System.IO;
using System.Web;
using System.Web.UI;
using umbraco.BasePages;

namespace umbraco.cms.presentation.user
{
    public class PermissionsHandlerBase : System.Web.Services.WebService
    {
        protected void Authorize()
        {
            if (!BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                throw new Exception("Client authorization failed. User is not logged in");
            }
        }

        protected static string GetPageResult(Page page)
        {
            var sw = new StringWriter();
            HttpContext.Current.Server.Execute(page, sw, false);
            return sw.ToString();
        }

        protected int[] ToIntArray(string nodeIds)
        {
            var s_nodes = nodeIds.Split(',');
            var i_nodes = new int[s_nodes.Length];

            for (int i = 0; i < s_nodes.Length; i++)
            {
                i_nodes[i] = int.Parse(s_nodes[i]);
            }

            return i_nodes;
        }
    }
}
