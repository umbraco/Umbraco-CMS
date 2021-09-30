using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services
{
    public class UserDataService : IUserDataService
    {
        private IUmbracoVersion _version;
        private readonly ILocalizationService _localizationService;
        public UserDataService(IUmbracoVersion version, ILocalizationService localizationService)
        {
            _version = version;
            _localizationService = localizationService;
        }

        public IEnumerable<UserData> GetUserData()
        {
            var userDataList = new List<UserData>
            {
                new UserData("Server OS", RuntimeInformation.OSDescription),
                new UserData("Server Framework", RuntimeInformation.FrameworkDescription),
                new UserData("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
                new UserData("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
                new UserData("Current Culture", Thread.CurrentThread.CurrentCulture.ToString()),
                new UserData("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
                new UserData("Current Webserver", GetCurrentWebServer())
            };
            return userDataList;
        }

        public string GetCurrentWebServer()
        {
            if (IsRunningInProcessIIS())
            {
                return "IIS";
            }

            return "Kestrel";
        }
        public bool IsRunningInProcessIIS()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            string processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
            return (processName.Contains("w3wp") || processName.Contains("iisexpress"));
        }
    }
}
