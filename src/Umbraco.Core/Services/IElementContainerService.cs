using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IElementContainerService : IEntityTypeContainerService<IElement>
{
    Task<Attempt<EntityContainerOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey);

    Task<Attempt<EntityContainerOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey);

    Task<Attempt<EntityContainerOperationStatus>> EmptyRecycleBinAsync(Guid userKey);

    Task<Attempt<EntityContainerOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey);
}
