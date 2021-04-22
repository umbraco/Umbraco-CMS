using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class InstallationRepository : IInstallationRepository
    {
        private const string RestApiInstallUrl = "https://our.umbraco.com/umbraco/api/Installation/Install";
        private readonly IHttpClientFactory _httpClientFactory;

        public InstallationRepository(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task SaveInstallLogAsync(InstallLog installLog)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientConstants.RestApiInstallUrl);
                await httpClient.PostAsync(RestApiInstallUrl, installLog, new JsonMediaTypeFormatter());
            }
            // this occurs if the server for Our is down or cannot be reached
            catch (HttpRequestException)
            { }
        }
    }
}
