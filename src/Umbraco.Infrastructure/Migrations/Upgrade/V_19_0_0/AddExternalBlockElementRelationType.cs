using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Adds the "External Block Element" relation type used to track elements that are
/// embedded as external (reusable) block content, so that only documents whose index
/// includes the element's content are reindexed when the element changes.
/// </summary>
public class AddExternalBlockElementRelationType : AsyncMigrationBase
{
    private readonly IRelationService _relationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddExternalBlockElementRelationType"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="relationService">The relation service used to create the relation type.</param>
    public AddExternalBlockElementRelationType(IMigrationContext context, IRelationService relationService)
        : base(context)
        => _relationService = relationService;

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        IRelationType? relationType = _relationService.GetRelationTypeByAlias(
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias);
        if (relationType != null)
        {
            return Task.CompletedTask;
        }

        // Generate the same unique key a fresh install would produce.
        Guid key = DatabaseDataCreator.CreateUniqueRelationTypeId(
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias,
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementName);

        // Save via the service so the repository cache is updated as well.
        relationType = new RelationType(
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementName,
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias,
            false,
            parentObjectType: null,
            childObjectType: null,
            isDependency: true)
        {
            Key = key
        };
        _relationService.Save(relationType);

        return Task.CompletedTask;
    }
}
