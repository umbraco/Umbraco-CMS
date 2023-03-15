using System.Globalization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Sync;

internal sealed class LastSyncedDatabaseRepository : ILastSyncedDatabaseRepository
{
    private readonly IKeyValueService _keyValueService;

    public LastSyncedDatabaseRepository(IKeyValueService keyValueService)
    {
        _keyValueService = keyValueService;
    }

    public void SetValue(string serverKey, int id) => _keyValueService.SetValue(serverKey, id.ToString(CultureInfo.InvariantCulture));

    public int GetValue(string serverKey)
    {
        var dbValue = _keyValueService.GetValue(serverKey);

        // TODO make individual repository and save timestamp when writing.
        if (int.TryParse(dbValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        return -1;
    }
}
