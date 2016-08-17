using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Umbraco.Core.IO;
using Umbraco.Core.Xml;

namespace dashboardUtilities
{
    

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
                            var feedProxyXml = XmlHelper.OpenAsXmlDocument(IOHelper.MapPath(SystemFiles.FeedProxyConfig));
                            if (feedProxyXml != null 
                                && feedProxyXml.SelectSingleNode(string.Concat("//allow[@host = '", requestUri.Host, "']")) != null
                                && requestUri.Port == 80)
                            {
                                using (var client = new WebClient())
                                {
                                    var response = client.DownloadString(requestUri);

                                    if (string.IsNullOrEmpty(response) == false)
                                    {
                                        Response.Clear();
                                        Response.ContentType = Request.CleanForXss("type") ?? MediaTypeNames.Text.Xml;
                                        Response.Write(response);
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.Debug<FeedProxy>(string.Format("Access to unallowed feedproxy attempted: {0}", requestUri));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<FeedProxy>("Exception occurred", ex);
            }
        }
    }
}