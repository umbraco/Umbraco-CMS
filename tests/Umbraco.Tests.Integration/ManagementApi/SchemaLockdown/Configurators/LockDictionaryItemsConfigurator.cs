using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown.Configurators;

/// <summary>
/// Locks dictionary items, and nothing else.
/// </summary>
public class LockDictionaryItemsConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(ISchemaLockdownRules rules)
        => rules.BlockMutations(Constants.UdiEntityType.DictionaryItem);
}
