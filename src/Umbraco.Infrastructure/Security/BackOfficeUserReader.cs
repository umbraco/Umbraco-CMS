using System.Data.Common;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Default implementation of <see cref="IBackOfficeUserReader"/>; wraps <see cref="IUserRepository"/>
/// with the scope handling and upgrade-state fallback required by back office user lookups.
/// </summary>
internal sealed class BackOfficeUserReader : IBackOfficeUserReader
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IUserRepository _userRepository;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeUserReader"/> class.
    /// </summary>
    /// <param name="scopeProvider">Provides database transaction scopes for data operations.</param>
    /// <param name="userRepository">Repository for accessing user data.</param>
    /// <param name="runtimeState">Represents the current runtime state of the Umbraco application.</param>
    public BackOfficeUserReader(
        ICoreScopeProvider scopeProvider,
        IUserRepository userRepository,
        IRuntimeState runtimeState)
    {
        _scopeProvider = scopeProvider;
        _userRepository = userRepository;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    public IUser? GetById(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        try
        {
            return _userRepository.Get(id);
        }
        catch (DbException)
        {
            // During upgrades the user table schema may be mid-migration, so fall back to
            // a minimal query that only resolves the fields needed for authorization.
            if (IsUpgrading)
            {
                return _userRepository.GetForUpgrade(id);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public IUser? GetByKey(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _userRepository.Get(key);
    }

    /// <inheritdoc />
    public IEnumerable<IUser> GetManyById(IEnumerable<int> ids)
    {
        // Need to use a List here because the expression tree cannot convert the array when used in Contains.
        // See ExpressionTests.Sql_In().
        List<int> idsAsList = [.. ids];
        if (idsAsList.Count == 0)
        {
            return Enumerable.Empty<IUser>();
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IQuery<IUser> query = _scopeProvider.CreateQuery<IUser>().Where(x => idsAsList.Contains(x.Id));
        return _userRepository.Get(query);
    }

    /// <inheritdoc />
    public IEnumerable<IUser> GetManyByKey(IEnumerable<Guid> keys)
    {
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        if (keysArray.Length == 0)
        {
            return Enumerable.Empty<IUser>();
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _userRepository.GetMany(keysArray);
    }

    /// <inheritdoc />
    public IEnumerable<IUser> GetAllInGroup(int groupId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _userRepository.GetAllInGroup(groupId);
    }

    private bool IsUpgrading =>
        _runtimeState.Level is RuntimeLevel.Install or RuntimeLevel.Upgrade or RuntimeLevel.Upgrading;
}
