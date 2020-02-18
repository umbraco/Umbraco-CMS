using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class UpgradeCheckRepository : IUpgradeCheckRepository
    {
        private static HttpClient _httpClient;
        private const string RestApiUpgradeChecklUrl = "https://our.umbraco.com/umbraco/api/UpgradeCheck/CheckUpgrade";


        public async Task<UpgradeResult> CheckUpgrade(SemVersion version)
        {
            try
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                var jsonObj = new { VersionMajor = version.Major, VersionMinor = version.Minor, VersionPatch = version.Patch, VersionComment = version.Prerelease };
                var json = JsonConvert.SerializeObject(jsonObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var task = await _httpClient.PostAsync(RestApiUpgradeChecklUrl, content);
                var jsonResponse = await task.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UpgradeResult>(jsonResponse);

                return result ?? new UpgradeResult("None", "", "");
            }
            catch (HttpRequestException)
            {
                // this occurs if the server for Our is down or cannot be reached
                return new UpgradeResult("None", "", "");
            }
        }
    }
}
