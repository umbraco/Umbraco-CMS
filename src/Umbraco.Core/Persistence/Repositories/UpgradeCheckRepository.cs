﻿using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Semver;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    public class UpgradeCheckRepository : IUpgradeCheckRepository
    {
        private readonly IJsonSerializer _jsonSerializer;
        private static HttpClient _httpClient;
        private const string RestApiUpgradeChecklUrl = "https://our.umbraco.com/umbraco/api/UpgradeCheck/CheckUpgrade";

        public UpgradeCheckRepository(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public async Task<UpgradeResult> CheckUpgradeAsync(SemVersion version)
        {
            try
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                var content = new StringContent(_jsonSerializer.Serialize(new CheckUpgradeDto(version)), Encoding.UTF8, "application/json");

                var task = await _httpClient.PostAsync(RestApiUpgradeChecklUrl,content);
                var json = await task.Content.ReadAsStringAsync();
                var result = _jsonSerializer.Deserialize<UpgradeResult>(json);

                return result ?? new UpgradeResult("None", "", "");
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

            public int VersionMajor { get;  }
            public int VersionMinor { get; }
            public int VersionPatch { get; }
            public string VersionComment { get;  }
        }
    }
}
