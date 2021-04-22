using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class UpgradeCheckRepository : IUpgradeCheckRepository
    {
        private const string RestApiUpgradeChecklUrl = "https://our.umbraco.com/umbraco/api/UpgradeCheck/CheckUpgrade";
        private readonly IHttpClientFactory _httpClientFactory;

        public UpgradeCheckRepository(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<UpgradeResult> CheckUpgradeAsync(SemVersion version)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientConstants.RestApiUpgradeChecklUrl);

                var task = await httpClient.PostAsync(RestApiUpgradeChecklUrl, new CheckUpgradeDto(version), new JsonMediaTypeFormatter());
                var result = await task.Content.ReadAsAsync<UpgradeResult>();

                return result ?? new UpgradeResult("None", "", "");
            }
            catch (UnsupportedMediaTypeException)
            {
                // this occurs if the server for Our is up but doesn't return a valid result (ex. content type)
                return new UpgradeResult("None", "", "");
            }
            catch (HttpRequestException)
            {
                // this occurs if the server for Our is down or cannot be reached
                return new UpgradeResult("None", "", "");
            }
        }
        private class CheckUpgradeDto
        {
            public CheckUpgradeDto(SemVersion version)
            {
                VersionMajor = version.Major;
                VersionMinor = version.Minor;
                VersionPatch = version.Patch;
                VersionComment = version.Prerelease;
            }

            public int VersionMajor { get; }
            public int VersionMinor { get; }
            public int VersionPatch { get; }
            public string VersionComment { get;  }
        }
    }
}
