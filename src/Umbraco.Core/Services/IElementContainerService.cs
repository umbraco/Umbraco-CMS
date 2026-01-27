using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IElementContainerService : IEntityTypeContainerService<IElement>
{
    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<EntityContainerOperationStatus>> EmptyRecycleBinAsync(Guid userKey);

    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);
}
