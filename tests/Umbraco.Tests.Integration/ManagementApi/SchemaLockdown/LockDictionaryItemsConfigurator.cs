using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locks dictionary items, and nothing else.
/// </summary>
public class LockDictionaryItemsConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(SchemaLockdownMatrix matrix)
        => matrix.BlockMutations(Constants.UdiEntityType.DictionaryItem);
}
