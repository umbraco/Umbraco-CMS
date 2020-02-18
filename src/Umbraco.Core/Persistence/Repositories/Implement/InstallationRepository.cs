using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class InstallationRepository : IInstallationRepository
    {
        private static HttpClient _httpClient;
        private const string RestApiInstallUrl = "https://our.umbraco.com/umbraco/api/Installation/Install";


        public async Task SaveInstall(InstallLog installLog)
        {
            try
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                var jsonObj = JsonConvert.SerializeObject(installLog);
                var content = new StringContent(jsonObj, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(RestApiInstallUrl, content);
            }
            // this occurs if the server for Our is down or cannot be reached
            catch (HttpRequestException)
            { }
        }
    }
}
