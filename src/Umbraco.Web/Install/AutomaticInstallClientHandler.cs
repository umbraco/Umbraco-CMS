namespace Umbraco.Web.Install
{
    using Newtonsoft.Json;
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
        private readonly InstallInstructions instructions = null;

        public AutomaticInstallClientHandler(InstallInstructions instructions)
        {
            this.instructions = instructions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public async Task<InstallProgressResultModel> Execute()
        {
            InstallProgressResultModel result = null;

            Uri currentUrl = HttpContext.Current.Request.Url;
            Uri targetUrl = new Uri(string.Format("{0}://{1}:{2}/install/api/PostPerformInstall", currentUrl.Scheme, IPAddress.Loopback, currentUrl.Port));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(HttpRequestHeader.Host.ToString(), HttpContext.Current.Request.Url.Host);

                do
                {
                    var data = new StringContent(JsonConvert.SerializeObject(instructions));

                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync(targetUrl, data))
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        result = JsonConvert.DeserializeObject<InstallProgressResultModel>(content.Substring(AngularJsonMediaTypeFormatter.XsrfPrefix.Length));
                    }
                } while (result.ProcessComplete == false);
            }

            return new InstallProgressResultModel(true, string.Empty, string.Empty);
        }
    }
}
