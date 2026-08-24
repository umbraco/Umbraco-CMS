using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locks document types, and nothing else.
/// </summary>
public class LockDocumentTypesConfigurator : ISchemaLockdownConfigurator
{
    /// <inheritdoc />
    public void Configure(SchemaLockdownMatrix matrix)
        => matrix.BlockMutations(Constants.UdiEntityType.DocumentType);
}
