using System.Text;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public class UpgradeCheckRepository : IUpgradeCheckRepository
{
    private const string RestApiUpgradeChecklUrl = "https://our.umbraco.com/umbraco/api/UpgradeCheck/CheckUpgrade";
    private static HttpClient? _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    public UpgradeCheckRepository(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    public async Task<UpgradeResult> CheckUpgradeAsync(SemVersion version)
    {
        try
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }

            var content = new StringContent(_jsonSerializer.Serialize(new CheckUpgradeDto(version)), Encoding.UTF8, "application/json");

            _httpClient.Timeout = TimeSpan.FromSeconds(1);
            HttpResponseMessage task = await _httpClient.PostAsync(RestApiUpgradeChecklUrl, content);
            var json = await task.Content.ReadAsStringAsync();
            UpgradeResult? result = _jsonSerializer.Deserialize<UpgradeResult>(json);

            return result ?? new UpgradeResult("None", string.Empty, string.Empty);
        }
        catch (HttpRequestException)
        {
            // this occurs if the server for Our is down or cannot be reached
            return new UpgradeResult("None", string.Empty, string.Empty);
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

        public string VersionComment { get; }
    }
}
