using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using HttpClientConstants = Umbraco.Core.Constants.HttpClientConstants;
namespace Umbraco.Web.Editors
{
    public class HelpController : UmbracoAuthorizedJsonController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HelpController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<List<HelpPage>> GetContextHelpForPage(string section, string tree, string baseUrl = "https://our.umbraco.com")
        {
            var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetContextHelpDocs?sectionAlias={0}&treeAlias={1}", section, tree);

            try
            {

                var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.OurUmbracoHelpPage);

                //fetch dashboard json and parse to JObject
                var json = await httpClient.GetStringAsync(url);
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
