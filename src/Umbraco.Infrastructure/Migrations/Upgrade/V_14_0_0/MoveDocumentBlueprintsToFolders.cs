using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Represents a migration that moves document blueprints into folders to improve their organization within the system.
/// </summary>
public class MoveDocumentBlueprintsToFolders : MigrationBase
{
    private readonly IEntityService _entityService;
    private readonly IContentService _contentService;
    private readonly IDocumentBlueprintContainerRepository _containerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0.MoveDocumentBlueprintsToFolders"/> class,
    /// which is responsible for migrating document blueprints into their respective folders during the upgrade to version 14.0.0.
    /// </summary>
    /// <param name="context">The migration context providing information and services for the migration process.</param>
    /// <param name="entityService">Service used to manage and query entities within Umbraco.</param>
    /// <param name="contentService">Service used to manage content items in Umbraco.</param>
    /// <param name="containerRepository">Repository for accessing and managing document blueprint containers.</param>
    public MoveDocumentBlueprintsToFolders(
        IMigrationContext context,
        IEntityService entityService,
        IContentService contentService,
        IDocumentBlueprintContainerRepository containerRepository)
        : base(context)
    {
        _entityService = entityService;
        _contentService = contentService;
        _containerRepository = containerRepository;
    }

    protected override void Migrate()
    {
        Guid[] allDocumentBlueprintKeysAtRoot = _entityService
            .GetAll(UmbracoObjectTypes.DocumentBlueprint)
            .Where(e => e.ParentId == Constants.System.Root)
            .Select(e => e.Key)
            .ToArray();

        if (allDocumentBlueprintKeysAtRoot.Any() is false)
        {
            return;
        }

        var allContainersAtRoot = _containerRepository.GetMany(
                _entityService
                    .GetRootEntities(UmbracoObjectTypes.DocumentBlueprint)
                    .Select(e => e.Id)
                    .ToArray())
            .ToList();

        foreach (Guid key in allDocumentBlueprintKeysAtRoot)
        {
            IContent? blueprint = _contentService.GetBlueprintById(key);
            if (blueprint is null)
            {
                continue;
            }

            EntityContainer? container = allContainersAtRoot.FirstOrDefault(c => c.Name == blueprint.ContentType.Name);
            if (container is null)
            {
                container = new EntityContainer(Constants.ObjectTypes.DocumentBlueprint)
                {
                    Name = blueprint.ContentType.Name
                };
                _containerRepository.Save(container);
                allContainersAtRoot.Add(container);
            }

            blueprint.ParentId = container.Id;
            _contentService.SaveBlueprint(blueprint, null);
        }
    }
}
