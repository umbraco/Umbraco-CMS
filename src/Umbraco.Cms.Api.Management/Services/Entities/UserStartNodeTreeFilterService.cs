using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// Abstract base class for user start node tree filter services.
/// </summary>
/// <remarks>
/// Contains the shared filtering logic for tree controllers that support user start node access.
/// Concrete implementations provide the start node resolution for their specific domain
/// (documents, media, or elements).
/// </remarks>
internal abstract class UserStartNodeTreeFilterService : IUserStartNodeTreeFilterService
{
    private readonly IUserStartNodeEntitiesService _userStartNodeEntitiesService;
    private readonly IDataTypeService _dataTypeService;

    private Dictionary<Guid, bool> _accessMap = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStartNodeTreeFilterService"/> class.
    /// </summary>
    /// <param name="userStartNodeEntitiesService">The service for retrieving user access entities.</param>
    /// <param name="dataTypeService">The data type service.</param>
    protected UserStartNodeTreeFilterService(
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService)
    {
        _userStartNodeEntitiesService = userStartNodeEntitiesService;
        _dataTypeService = dataTypeService;
    }

    /// <summary>
    /// Gets the object types to include in tree queries.
    /// </summary>
    protected abstract UmbracoObjectTypes[] TreeObjectTypes { get; }

    private int[] UserStartNodeIds => field ??= CalculateUserStartNodeIds();

    private string[] UserStartNodePaths => field ??= CalculateUserStartNodePaths();

    /// <inheritdoc />
    public bool ShouldBypassStartNodeFiltering(Guid? dataTypeKey = null)
        => UserHasRootAccess() || IgnoreUserStartNodes(dataTypeKey);

    /// <inheritdoc />
    public IEntitySlim[] GetFilteredRootEntities(out long totalItems)
        => CalculateAccessMap(
            () => _userStartNodeEntitiesService.RootUserAccessEntities(TreeObjectTypes, UserStartNodeIds),
            out totalItems);

    /// <inheritdoc />
    public IEntitySlim[] GetFilteredChildEntities(
        Guid parentKey,
        int skip,
        int take,
        Ordering ordering,
        out long totalItems)
    {
        IEnumerable<UserAccessEntity> userAccessEntities = _userStartNodeEntitiesService.ChildUserAccessEntities(
            TreeObjectTypes,
            UserStartNodePaths,
            parentKey,
            skip,
            take,
            ordering,
            out totalItems);

        return CalculateAccessMap(() => userAccessEntities, out _);
    }

    /// <inheritdoc />
    public IEntitySlim[] GetFilteredSiblingEntities(
        Guid target,
        int before,
        int after,
        Ordering ordering,
        out long totalBefore,
        out long totalAfter)
    {
        IEnumerable<UserAccessEntity> userAccessEntities = _userStartNodeEntitiesService.SiblingUserAccessEntities(
            TreeObjectTypes,
            UserStartNodePaths,
            target,
            before,
            after,
            ordering,
            out totalBefore,
            out totalAfter);

        return CalculateAccessMap(() => userAccessEntities, out _);
    }

    /// <inheritdoc />
    public TItem[] MapWithAccessFiltering<TItem>(
        IEntitySlim[] entities,
        Func<IEntitySlim, TItem> mapEntity,
        Func<IEntitySlim, TItem> mapEntityAsNoAccess)
        where TItem : class =>
        entities.Select(entity =>
            {
                if (_accessMap.TryGetValue(entity.Key, out var hasAccess) is false)
                {
                    return null;
                }

                return hasAccess ? mapEntity(entity) : mapEntityAsNoAccess(entity);
            })
            .WhereNotNull()
            .ToArray();

    /// <summary>
    /// Calculates the start node IDs for the current user.
    /// </summary>
    /// <returns>An array of start node IDs.</returns>
    protected abstract int[] CalculateUserStartNodeIds();

    /// <summary>
    /// Calculates the start node paths for the current user.
    /// </summary>
    /// <returns>An array of start node paths.</returns>
    protected abstract string[] CalculateUserStartNodePaths();

    private bool UserHasRootAccess() => UserStartNodeIds.Contains(Constants.System.Root);

    private bool IgnoreUserStartNodes(Guid? dataTypeKey)
        => dataTypeKey.HasValue
           && _dataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeKey.Value);

    private IEntitySlim[] CalculateAccessMap(Func<IEnumerable<UserAccessEntity>> getUserAccessEntities, out long totalItems)
    {
        UserAccessEntity[] userAccessEntities = getUserAccessEntities().ToArray();

        _accessMap = userAccessEntities.ToDictionary(uae => uae.Entity.Key, uae => uae.HasAccess);

        IEntitySlim[] entities = userAccessEntities.Select(uae => uae.Entity).ToArray();
        totalItems = entities.Length;

        return entities;
    }
}
