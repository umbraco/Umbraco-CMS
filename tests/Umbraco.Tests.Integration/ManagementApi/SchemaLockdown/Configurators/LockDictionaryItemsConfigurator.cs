using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown.Configurators;

/// <summary>
/// Locks dictionary items, and nothing else.
/// </summary>
public class LockDictionaryItemsConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(ISchemaRestrictionsBuilder builder)
        => builder.BlockMutations(Constants.UdiEntityType.DictionaryItem);
}
