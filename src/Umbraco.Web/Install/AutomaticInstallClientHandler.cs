namespace Umbraco.Web.Install
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using Umbraco.Web.Install.Models;
    using Umbraco.Web.WebApi;

    /// <summary>
    /// 
    /// </summary>
    public class AutomaticInstallClientHandler
    {
        private readonly InstallInstructions _instructions = null;

        private readonly string _loginRoute = "umbraco/backoffice/UmbracoApi/Authentication/PostLogin";
        private readonly string _postPerformInstallRoute = "install/api/PostPerformInstall";

        public AutomaticInstallClientHandler(InstallInstructions instructions)
        {
            _instructions = instructions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requireAuthentication"></param>
        /// <returns></returns>
        public async Task<InstallProgressResultModel> Execute(CredentialModel credential = null)
        {
            InstallProgressResultModel result = null;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(HttpRequestHeader.Host.ToString(), HttpContext.Current.Request.Url.Host);

                if (credential != null)
                {
                    var data = GetPostData(credential);

                    using (var response = await client.PostAsync(GetUri(_loginRoute), data))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new UnauthorizedAccessException();
                        }
                    }
                }
                
                do
                {
                    var data = GetPostData(_instructions);

                    using (var response = await client.PostAsync(GetUri(_postPerformInstallRoute), data))
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        result = JsonConvert.DeserializeObject<InstallProgressResultModel>(content.Substring(AngularJsonMediaTypeFormatter.XsrfPrefix.Length));

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new UnauthorizedAccessException();
                        }
                    }
                } while (result.ProcessComplete == false);
            }

            return new InstallProgressResultModel(true, string.Empty, string.Empty);
        }

        /// <summary>
        /// Serialize data as StringContent to pass to PostAsync
        /// </summary>
        /// <param name="data">Object to serialize</param>
        /// <returns>Serialized object as StringContent</returns>
        private StringContent GetPostData(object data)
        {
            var sc = new StringContent(JsonConvert.SerializeObject(data));

            sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return sc;
        }

        /// <summary>
        /// Get a full URL to Umbraco from a relative one
        /// </summary>
        /// <param name="route">Relative URL</param>
        /// <returns>Full URL</returns>
        private Uri GetUri(string route)
        {
            Uri currentUrl = HttpContext.Current.Request.Url;

            return new Uri(string.Format("{0}://{1}:{2}/{3}", currentUrl.Scheme, IPAddress.Loopback, currentUrl.Port, route.TrimStart('/')));
        }
    }
}
