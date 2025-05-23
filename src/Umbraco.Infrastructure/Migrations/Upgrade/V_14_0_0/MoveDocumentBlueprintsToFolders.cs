using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class MoveDocumentBlueprintsToFolders : MigrationBase
{
    private readonly IEntityService _entityService;
    private readonly IContentService _contentService;
    private readonly IDocumentBlueprintContainerRepository _containerRepository;

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
            _contentService.SaveBlueprint(blueprint);
        }
    }
}
