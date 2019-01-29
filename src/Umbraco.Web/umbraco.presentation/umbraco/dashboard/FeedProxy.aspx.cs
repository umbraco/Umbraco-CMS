using System.Net.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using System;
using System.Linq;
using System.Net.Mime;
using Umbraco.Core.IO;
using Umbraco.Core.Xml;
using Umbraco.Web.Composing;

namespace dashboardUtilities
{


    public partial class FeedProxy : UmbracoEnsuredPage
    {
        private static HttpClient _httpClient;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString.AllKeys.Contains("url") == false || Request.QueryString["url"] == null)
                    return;

                var url = Request.QueryString["url"];
                if (string.IsNullOrWhiteSpace(url) || url.StartsWith("/"))
                    return;

                if (Uri.TryCreate(url, UriKind.Absolute, out var requestUri) == false)
                    return;

                var feedProxyXml = XmlHelper.OpenAsXmlDocument(IOHelper.MapPath(SystemFiles.FeedProxyConfig));
                if (feedProxyXml?.SelectSingleNode($"//allow[@host = '{requestUri.Host}']") != null && (requestUri.Port == 80 || requestUri.Port == 443))
                {
                    if (_httpClient == null)
                        _httpClient = new HttpClient();

                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        var response = _httpClient.SendAsync(request).Result;
                        var result = response.Content.ReadAsStringAsync().Result;

                        if (string.IsNullOrEmpty(result))
                            return;

                        Response.Clear();
                        Response.ContentType = Request.CleanForXss("type") ?? MediaTypeNames.Text.Xml;
                        Response.Write(result);
                    }
                }
                else
                {
                                Current.Logger.Debug<FeedProxy>("Access to unallowed feed proxy attempted: {RequestUrl}", requestUri);
                }
            }
            catch (Exception ex)
            {
                Current.Logger.Error<FeedProxy>(ex, "Exception occurred");
            }
        }
    }
}
