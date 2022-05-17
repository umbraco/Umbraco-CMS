using System.Text;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public class InstallationRepository : IInstallationRepository
{
    private const string RestApiInstallUrl = "https://our.umbraco.com/umbraco/api/Installation/Install";
    private static HttpClient? _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    public InstallationRepository(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    public async Task SaveInstallLogAsync(InstallLog installLog)
    {
        try
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }

            var content = new StringContent(_jsonSerializer.Serialize(installLog), Encoding.UTF8, "application/json");

            await _httpClient.PostAsync(RestApiInstallUrl, content);
        }

        // this occurs if the server for Our is down or cannot be reached
        catch (HttpRequestException)
        {
        }
    }
}
