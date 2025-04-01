using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_4_0;

/// <summary>
/// Migration to add an automatic relation type for members if it doesn't already exist.
/// </summary>
public class AddRelationTypeForMembers : MigrationBase
{
    private readonly IRelationService _relationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRelationTypeForMembers"/> class.
    /// </summary>
    public AddRelationTypeForMembers(IMigrationContext context, IRelationService relationService)
        : base(context) => _relationService = relationService;

    /// <inheritdoc/>
    protected override void Migrate()
    {
        Logger.LogDebug("Adding automatic relation type for members if it doesn't already exist");

        IRelationType? relationType = _relationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMemberAlias);
        if (relationType != null)
        {
            Logger.LogDebug("Automatic relation type for members already exists.");
            return;
        }

        // Generate a new unique relation type key that is the same as would have come from a new install.
        Guid key = DatabaseDataCreator.CreateUniqueRelationTypeId(
            Constants.Conventions.RelationTypes.RelatedMemberAlias,
            Constants.Conventions.RelationTypes.RelatedMemberName);

        // Create new relation type using service, so the repository cache gets updated as well.
        relationType = new RelationType(Constants.Conventions.RelationTypes.RelatedMemberName, Constants.Conventions.RelationTypes.RelatedMemberAlias, false, null, null, true)
        {
            Key = key
        };
        _relationService.Save(relationType);
    }
}
