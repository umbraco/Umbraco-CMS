using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Services
{
    public class UserDataService : IUserDataService
    {
        private readonly ISystemInformationTableDataProvider _tableDataProvider;

        public UserDataService(ISystemInformationTableDataProvider tableDataProvider)
        {
            _tableDataProvider = tableDataProvider;
        }

        [Obsolete("Use constructor that takes ISystemInformationTableDataProvider")]
        public UserDataService(IUmbracoVersion version, ILocalizationService localizationService)
            : this(StaticServiceProvider.Instance.GetRequiredService<ISystemInformationTableDataProvider>())
        {
        }

        public IEnumerable<UserData> GetUserData() => _tableDataProvider.GetSystemInformationTableData();

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
