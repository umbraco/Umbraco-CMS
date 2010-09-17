namespace dashboardUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Net;
    using System.IO;

    public partial class FeedProxy : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Request["url"]);
            request.Method = WebRequestMethods.Http.Get;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string tmp = reader.ReadToEnd();
            response.Close();

            Response.Clear();
            Response.ContentType = "text/xml";

            Response.Write(tmp);
        }
    }
}