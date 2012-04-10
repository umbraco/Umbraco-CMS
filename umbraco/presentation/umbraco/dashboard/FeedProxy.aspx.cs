namespace dashboardUtilities
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Xml;
    using umbraco.BasePages;
    using umbraco.BusinessLogic;
    using umbraco.IO;
    using umbraco;

    public partial class FeedProxy : UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString.AllKeys.Contains("url") && Request.QueryString["url"] != null)
                {
                    var url = Request.QueryString["url"];
                    if (!string.IsNullOrWhiteSpace(url) && !url.StartsWith("/"))
                    {
                        Uri requestUri;
                        if (Uri.TryCreate(url, UriKind.Absolute, out requestUri))
                        {
                            var feedProxyXml = xmlHelper.OpenAsXmlDocument(IOHelper.MapPath(SystemFiles.FeedProxyConfig));
                            if (feedProxyXml != null && feedProxyXml.SelectSingleNode(string.Concat("//allow[@host = '", requestUri.Host, "']")) != null)
                            {
                                using (var client = new WebClient())
                                {
                                    var response = client.DownloadString(requestUri);

                                    if (!string.IsNullOrEmpty(response))
                                    {
                                        Response.Clear();
                                        Response.ContentType = Request.QueryString["type"] ?? MediaTypeNames.Text.Xml;
                                        Response.Write(response);
                                        Response.End();
                                    }
                                }
                            } else
                            {
                                Log.Add(LogTypes.Error, -1, string.Format("Access to unallowed feedproxy attempted: {0}", requestUri));                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, -1, ex.ToString());
            }
        }
    }
}