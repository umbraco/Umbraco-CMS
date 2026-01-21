using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IUserDataService" /> providing operations for managing user-specific data entries.
/// </summary>
public class UserDataService : RepositoryService, IUserDataService
{
    private readonly IUserDataRepository _userDataRepository;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserDataService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="userDataRepository">The repository for user data operations.</param>
    /// <param name="userService">The user service for user-related operations.</param>
    public UserDataService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IUserDataRepository userDataRepository,
        IUserService userService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _userDataRepository = userDataRepository;
        _userService = userService;
    }

    /// <inheritdoc />
    public async Task<IUserData?> GetAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUserData? userData = await _userDataRepository.GetAsync(key);
        scope.Complete();
        return userData;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IUserData>> GetAsync(int skip, int take, IUserDataFilter? filter = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        PagedModel<IUserData> pagedUserData = await _userDataRepository.GetAsync(skip, take, filter);
        scope.Complete();
        return pagedUserData;
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserData, UserDataOperationStatus>> CreateAsync(IUserData userData)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUserData? existingUserData = await _userDataRepository.GetAsync(userData.Key);
        if (existingUserData is not null)
        {
            return Attempt<IUserData, UserDataOperationStatus>.Fail(UserDataOperationStatus.AlreadyExists, userData);
        }

        if (await ReferencedUserExits(userData) is false)
        {
            return Attempt<IUserData, UserDataOperationStatus>.Fail(UserDataOperationStatus.UserNotFound, userData);
        }

        await _userDataRepository.Save(userData);

        scope.Complete();
        return Attempt<IUserData, UserDataOperationStatus>.Succeed(UserDataOperationStatus.Success, userData);
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserData, UserDataOperationStatus>> UpdateAsync(IUserData userData)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUserData? existingUserData = await _userDataRepository.GetAsync(userData.Key);
        if (existingUserData is null)
        {
            return Attempt<IUserData, UserDataOperationStatus>.Fail(UserDataOperationStatus.NotFound, userData);
        }

        if (await ReferencedUserExits(userData) is false)
        {
            return Attempt<IUserData, UserDataOperationStatus>.Fail(UserDataOperationStatus.UserNotFound, userData);
        }

        await _userDataRepository.Update(userData);

        scope.Complete();
        return Attempt<IUserData, UserDataOperationStatus>.Succeed(UserDataOperationStatus.Success, userData);
    }

    /// <inheritdoc />
    public async Task<Attempt<UserDataOperationStatus>> DeleteAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUserData? existingUserData = await _userDataRepository.GetAsync(key);
        if (existingUserData is null)
        {
            return Attempt<UserDataOperationStatus>.Fail(UserDataOperationStatus.NotFound);
        }

        await _userDataRepository.Delete(existingUserData);

        scope.Complete();
        return Attempt<UserDataOperationStatus>.Succeed(UserDataOperationStatus.Success);
    }

    /// <summary>
    ///     Checks whether the user referenced by the user data exists.
    /// </summary>
    /// <param name="userData">The user data containing the user reference.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the referenced user exists; otherwise, false.</returns>
    private async Task<bool> ReferencedUserExits(IUserData userData)
        => await _userService.GetAsync(userData.UserKey) is not null;
}
