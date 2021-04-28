using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class InstallationRepository : IInstallationRepository
    {
        private static HttpClient _httpClient;
        private const string RestApiInstallUrl = "https://our.umbraco.com/umbraco/api/Installation/Install";


        public async Task SaveInstallLogAsync(InstallLog installLog)
        {
            try
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                await _httpClient.PostAsync(RestApiInstallUrl, installLog, new JsonMediaTypeFormatter());
            }
            // this occurs if the server for Our is down or cannot be reached
            catch (HttpRequestException)
            { }
        }
    }
}
