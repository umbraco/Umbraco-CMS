using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <inheritdoc />
internal sealed class ItemAncestorService : IItemAncestorService
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemAncestorService"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="umbracoMapper">The mapper to handle entity mapping.</param>
    public ItemAncestorService(IEntityService entityService, IUmbracoMapper umbracoMapper)
    {
        _entityService = entityService;
        _umbracoMapper = umbracoMapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ItemAncestorsResponseModel<NamedItemResponseModel>>> GetAncestorsAsync(
        UmbracoObjectTypes itemObjectType,
        UmbracoObjectTypes? folderObjectType,
        ISet<Guid> entityKeys)
    {
        return await GetAncestorsAsync(itemObjectType, folderObjectType, entityKeys, AncestorMapper);

        Task<IEnumerable<NamedItemResponseModel>> AncestorMapper(IEnumerable<IEntitySlim> entities)
            => Task.FromResult(_umbracoMapper.MapEnumerable<IEntitySlim, NamedItemResponseModel>(entities).AsEnumerable());
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ItemAncestorsResponseModel<TAncestorItem>>> GetAncestorsAsync<TAncestorItem>(
        UmbracoObjectTypes itemObjectType,
        UmbracoObjectTypes? folderObjectType,
        ISet<Guid> entityKeys,
        Func<IEnumerable<IEntitySlim>, Task<IEnumerable<TAncestorItem>>> ancestorMapper)
        where TAncestorItem : ItemResponseModelBase
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
            return entities.Select(e => new ItemAncestorsResponseModel<TAncestorItem> { Id = e.Key });
        }

        // Batch fetch ancestor entities by int IDs, storing the full IEntitySlim objects.
        var ancestorById = _entityService
            .GetAll(itemObjectType, allAncestorIds)
            .ToDictionary(a => a.Id);

        // For folder types: also fetch container ancestors (one-by-one, as EntityService.GetAll
        // doesn't work with container types).
        if (folderObjectType.HasValue)
        {
            foreach (var ancestorId in allAncestorIds.Where(id => !ancestorById.ContainsKey(id)))
            {
                IEntitySlim? container = _entityService.Get(ancestorId, folderObjectType.Value);
                if (container is not null)
                {
                    ancestorById[container.Id] = container;
                }
            }
        }

        // Call the mapping delegate with all ancestor entities to produce rich models.
        var mappedAncestors = (await ancestorMapper(ancestorById.Values)).ToDictionary(ancestor => ancestor.Id);

        // Map per-entity: entity key -> ordered ancestor chain (root-first).
        return entities.Select(entity =>
        {
            var ancestorIds = entity.AncestorIds();
            var ancestors = new List<TAncestorItem>(ancestorIds.Length);

            foreach (var ancestorId in ancestorIds)
            {
                if (ancestorById.TryGetValue(ancestorId, out IEntitySlim? ancestorEntity)
                    && mappedAncestors.TryGetValue(ancestorEntity.Key, out TAncestorItem? mapped))
                {
                    ancestors.Add(mapped);
                }
            }

            return new ItemAncestorsResponseModel<TAncestorItem>
            {
                Id = entity.Key,
                Ancestors = ancestors,
            };
        });
    }
}
