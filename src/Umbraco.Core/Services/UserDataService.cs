using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Telemetry.DataCollectors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services
{
    public class UserDataService : IUserDataService
    {
        private readonly IUmbracoVersion _version;
        private readonly ILocalizationService _localizationService;

        public UserDataService(IUmbracoVersion version, ILocalizationService localizationService)
        {
            _version = version;
            _localizationService = localizationService;
        }

        public IEnumerable<UserData> GetUserData() =>
            new List<UserData>
            {
                new ("Server OS", RuntimeInformation.OSDescription),
                new ("Server Framework", RuntimeInformation.FrameworkDescription),
                new ("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
                new ("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
                new ("Current Culture", Thread.CurrentThread.CurrentCulture.ToString()),
                new ("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
                new ("Current Webserver", SystemInformationTelemetryDataCollector.GetServer())
            };
    }
}
