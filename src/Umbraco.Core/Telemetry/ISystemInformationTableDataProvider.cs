using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Telemetry
{
    /// <summary>
    /// Provides the data for the system information panel shown
    /// </summary>
    public interface ISystemInformationTableDataProvider
    {
        IEnumerable<UserData> GetSystemInformationTableData();
    }
}
