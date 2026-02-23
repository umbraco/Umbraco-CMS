using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <inheritdoc />
internal sealed class ItemAncestorService : IItemAncestorService
{
    private readonly IEntityService _entityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAncestorService"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    public ItemAncestorService(IEntityService entityService)
        => _entityService = entityService;

    /// <inheritdoc />
    public IEnumerable<ItemAncestorsResponseModel> GetAncestors(
        UmbracoObjectTypes itemObjectType,
        UmbracoObjectTypes? folderObjectType,
        ISet<Guid> entityKeys)
    {
        // Batch fetch all requested entities by Guid key (trying both item + folder types).
        IEntitySlim[] entities = _entityService
            .GetAll(itemObjectType, entityKeys.ToArray())
            .ToArray();

        // For folder types: fetch container entities one-by-one, as EntityService.GetAll
        // doesn't work with container object types.
        if (folderObjectType.HasValue)
        {
            var remainingKeys = entityKeys.Except(entities.Select(e => e.Key));
            var folderEntities = remainingKeys
                .Select(key => _entityService.Get(key, folderObjectType.Value))
                .WhereNotNull()
                .ToArray();

            entities = entities.Union(folderEntities).ToArray();
        }

        if (entities.Length == 0)
        {
            return [];
        }

        // Collect all unique ancestor int IDs from entity paths.
        var allAncestorIds = entities
            .SelectMany(e => e.AncestorIds())
            .Distinct()
            .ToArray();

        if (allAncestorIds.Length == 0)
        {
            // All entities are root-level - return empty ancestor chains.
            return entities.Select(e => new ItemAncestorsResponseModel { Id = e.Key });
        }

        // Batch fetch ancestor entities by int IDs.
        var ancestorIdToKey = _entityService
            .GetAll(itemObjectType, allAncestorIds)
            .ToDictionary(a => a.Id, a => a.Key);

        // For folder types: also fetch container ancestors (one-by-one, as EntityService.GetAll
        // doesn't work with container types).
        if (folderObjectType.HasValue)
        {
            foreach (var ancestorId in allAncestorIds.Where(id => !ancestorIdToKey.ContainsKey(id)))
            {
                IEntitySlim? container = _entityService.Get(ancestorId, folderObjectType.Value);
                if (container is not null)
                {
                    ancestorIdToKey[container.Id] = container.Key;
                }
            }
        }

        // Map per-entity: entity key -> ordered ancestor Guid chain (root-first).
        return entities.Select(entity =>
        {
            var ancestorIds = entity.AncestorIds();
            var ancestorKeys = new List<ReferenceByIdModel>(ancestorIds.Length);

            foreach (var ancestorId in ancestorIds)
            {
                if (ancestorIdToKey.TryGetValue(ancestorId, out Guid ancestorKey))
                {
                    ancestorKeys.Add(new ReferenceByIdModel(ancestorKey));
                }
            }

            return new ItemAncestorsResponseModel
            {
                Id = entity.Key,
                Ancestors = ancestorKeys,
            };
        });
    }
}
