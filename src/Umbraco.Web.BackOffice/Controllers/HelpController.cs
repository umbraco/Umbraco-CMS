using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Web.Common.Attributes;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class HelpController : UmbracoAuthorizedJsonController
    {
        private readonly ILogger<HelpController> _logger;

        public HelpController(ILogger<HelpController> logger)
        {
            _logger = logger;
        }

        private static HttpClient _httpClient;
        public async Task<List<HelpPage>> GetContextHelpForPage(string section, string tree, string baseUrl = "https://our.umbraco.com")
        {
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
                _logger.LogInformation($"Check your network connection, exception: {rex.Message}");
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
