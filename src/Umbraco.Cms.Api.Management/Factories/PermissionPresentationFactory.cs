using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Api.Management.Factories;

public class PermissionPresentationFactory : IPermissionPresentationFactory
{
    private readonly ILogger<PermissionPresentationFactory> _logger;
    private readonly IDictionary<string, IPermissionPresentationMapper> _permissionPresentationMappersByContext;
    private readonly Dictionary<Type, IPermissionPresentationMapper> _permissionPresentationMappersByType;

    public PermissionPresentationFactory(IEnumerable<IPermissionPresentationMapper> permissionPresentationMappers, ILogger<PermissionPresentationFactory> logger)
    {
        _logger = logger;
        _permissionPresentationMappersByContext = permissionPresentationMappers.ToDictionary(x => x.Context);
        _permissionPresentationMappersByType = permissionPresentationMappers.ToDictionary(x => x.PresentationModelToHandle);
    }

    public Task<ISet<IPermissionPresentationModel>> CreateAsync(ISet<IGranularPermission> granularPermissions)
    {
        var result = new HashSet<IPermissionPresentationModel>();

        IEnumerable<IGrouping<string, IGranularPermission>> contexts = granularPermissions.GroupBy(x => x.Context);

        foreach (IGrouping<string, IGranularPermission> contextGroup in contexts)
        {
            if (_permissionPresentationMappersByContext.TryGetValue(contextGroup.Key, out IPermissionPresentationMapper? mapper))
            {
                IEnumerable<IPermissionPresentationModel> mapped = mapper.MapManyAsync(contextGroup);
                foreach (IPermissionPresentationModel permissionPresentationModel in mapped)
                {
                    result.Add(permissionPresentationModel);
                }
            }
            else
            {
                IEnumerable<IGrouping<Guid?, IGranularPermission>> keyGroups = contextGroup.GroupBy(x => x.Key);
                foreach (IGrouping<Guid?, IGranularPermission> keyGroup in keyGroups)
                {
                    var verbs = keyGroup.Select(x => x.Permission).ToHashSet();
                    result.Add(new UnknownTypePermissionPresentationModel()
                    {
                        Context = contextGroup.Key, Verbs = verbs
                    });
                }
            }
        }

        return Task.FromResult<ISet<IPermissionPresentationModel>>(result);
    }

    public Task<ISet<IGranularPermission>> CreatePermissionSetsAsync(ISet<IPermissionPresentationModel> permissions)
    {
        ISet<IGranularPermission> granularPermissions = new HashSet<IGranularPermission>();
        foreach (IPermissionPresentationModel permissionViewModel in permissions)
        {
            if (_permissionPresentationMappersByType.TryGetValue(permissionViewModel.GetType(), out IPermissionPresentationMapper? mapper))
            {
                IEnumerable<IGranularPermission> mapped = mapper.MapToGranularPermissions(permissionViewModel);
                foreach (IGranularPermission granularPermission in mapped)
                {
                    granularPermissions.Add(granularPermission);
                }
            }
            else if (permissionViewModel is UnknownTypePermissionPresentationModel unknownTypePermissionViewModel)
            {
                foreach (var verb in unknownTypePermissionViewModel.Verbs)
                {
                    granularPermissions.Add(new UnknownTypeGranularPermission
                    {
                        Context = unknownTypePermissionViewModel.Context,
                        Permission = verb
                    });
                }
            }
            else
            {
                _logger.LogWarning("Unknown mapper for type {Type} to IGranularPermission", permissionViewModel.GetType());
            }
        }

        return Task.FromResult(granularPermissions);
    }
}
