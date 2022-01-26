using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core.Help;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Editors
{
    public class HelpController : UmbracoAuthorizedJsonController
    {
        private readonly IHelpPageSettings _helpPageSettings;

        public HelpController(IHelpPageSettings helpPageSettings)
        {
            _helpPageSettings = helpPageSettings;
        }

        private static HttpClient _httpClient;
        public async Task<List<HelpPage>> GetContextHelpForPage(string section, string tree, string baseUrl = "https://our.umbraco.com")
        {
            if (IsAllowedUrl(baseUrl) is false)
            {
                Logger.Error<HelpController>($"The following URL is not listed in the allowlist for HelpPage in web.config: {baseUrl}");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "HelpPage source not permitted"));
            }

            var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetContextHelpDocs?sectionAlias={0}&treeAlias={1}", section, tree);

            try
            {

                if (_httpClient == null)
                    _httpClient = new HttpClient();

                //fetch dashboard json and parse to JObject
                var json = await _httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<List<HelpPage>>(json);
                if (result != null)
                    return result;

            }
            catch (HttpRequestException rex)
            {
                Logger.Info(GetType(), $"Check your network connection, exception: {rex.Message}");
            }

            return new List<HelpPage>();
        }

        private bool IsAllowedUrl(string url)
        {
            if (string.IsNullOrEmpty(_helpPageSettings.HelpPageUrlAllowList) ||
                _helpPageSettings.HelpPageUrlAllowList.Contains(url))
            {
                return true;
            }

            return false;
        }
    }

    [DataContract(Name = "HelpPage")]
    public class HelpPage
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

    }
}
