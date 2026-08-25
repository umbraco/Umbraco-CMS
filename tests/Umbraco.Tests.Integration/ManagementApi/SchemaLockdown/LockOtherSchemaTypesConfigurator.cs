using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locks a broad set of schema entity types, none of which are webhooks.
/// </summary>
public class LockOtherSchemaTypesConfigurator : ISchemaLockdownConfigurator
{
    private static readonly string[] EntityTypes =
    [
        Constants.UdiEntityType.DocumentType,
        Constants.UdiEntityType.MediaType,
        Constants.UdiEntityType.MemberType,
        Constants.UdiEntityType.DataType,
        Constants.UdiEntityType.DictionaryItem,
        Constants.UdiEntityType.Language,
    ];

    /// <inheritdoc />
    public void Configure(ISchemaLockdownRules rules)
    {
        foreach (var entityType in EntityTypes)
        {
            rules.BlockMutations(entityType);
        }
    }
}
