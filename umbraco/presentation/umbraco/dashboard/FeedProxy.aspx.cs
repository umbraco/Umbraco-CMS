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
    using umbraco.BasePages;

    public partial class FeedProxy : UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["url"] != null)
            {
                var requestUri = new Uri(Request["url"]);
                if (requestUri != null)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
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
    }
}