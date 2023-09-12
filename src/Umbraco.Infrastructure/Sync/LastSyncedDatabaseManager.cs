using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Sync;

internal sealed class LastSyncedDatabaseManager : ILastSyncedManager
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILastSyncedDatabaseRepository _lastSyncedDatabaseRepository;
    private HostingSettings _hostingSettings;

    private int? _lastSyncedId;

    public LastSyncedDatabaseManager(
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<HostingSettings> hostingOptionsMonitor,
        ILastSyncedDatabaseRepository lastSyncedDatabaseRepository)
    {
        _hostingEnvironment = hostingEnvironment;
        _lastSyncedDatabaseRepository = lastSyncedDatabaseRepository;
        _hostingSettings = hostingOptionsMonitor.CurrentValue;
        hostingOptionsMonitor.OnChange(hostingSettings =>
        {
            // We sitename changes doing runtime. we save the value to the new name.
            var lastSyncedId = _lastSyncedId ?? _lastSyncedDatabaseRepository.GetValue(ServerKey);
            _hostingSettings = hostingSettings;

            _lastSyncedDatabaseRepository.SetValue(ServerKey, lastSyncedId);
        });
    }

    private string ServerKey
    {
        get
        {
            var unique = _hostingSettings.SiteName;

            if (string.IsNullOrEmpty(unique))
            {
                unique = (Environment.MachineName + _hostingEnvironment.ApplicationId).GenerateHash();
            }

            return $"{nameof(LastSyncedDatabaseManager)}_{unique}";
        }
    }

    public int LastSyncedId
    {
        get
        {
            if (_lastSyncedId.HasValue)
            {
                return _lastSyncedId.Value;
            }

            _lastSyncedId = _lastSyncedDatabaseRepository.GetValue(ServerKey);


            return _lastSyncedId ?? -1;
        }
    }

    public void SaveLastSyncedId(int id)
    {
        _lastSyncedDatabaseRepository.SetValue(ServerKey, id);
        _lastSyncedId = id;
    }

}
