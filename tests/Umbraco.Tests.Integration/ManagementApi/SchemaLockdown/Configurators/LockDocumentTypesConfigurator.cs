using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown.Configurators;

/// <summary>
/// Locks document types, and nothing else.
/// </summary>
public class LockDocumentTypesConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(ISchemaRestrictionsBuilder builder)
        => builder.BlockMutations(Constants.UdiEntityType.DocumentType);
}
