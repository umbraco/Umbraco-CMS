using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown.Configurators;

/// <summary>
/// Locks document types, and nothing else.
/// </summary>
public class LockDocumentTypesConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(ISchemaLockdownRules rules)
        => rules.BlockMutations(Constants.UdiEntityType.DocumentType);
}
