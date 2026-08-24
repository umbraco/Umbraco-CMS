using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locks data types, and nothing else.
/// </summary>
public class LockDataTypesConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(SchemaLockdownRules rules)
        => rules.BlockMutations(Constants.UdiEntityType.DataType);
}
