using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Umbraco.Web.Editors
{
    public class HelpController : UmbracoAuthorizedJsonController
    {
        private static HttpClient _httpClient;
        public async Task<List<HelpPage>> GetContextHelpForPage(string section, string tree, string baseUrl = "https://our.umbraco.com")
        {
            var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetContextHelpDocs?sectionAlias={0}&treeAlias={1}", section, tree);

            if (_httpClient == null)
                _httpClient = new HttpClient();

            //fetch dashboard json and parse to JObject
            var json = await _httpClient.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<List<HelpPage>>(json);
            if (result != null)
                return result;

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
