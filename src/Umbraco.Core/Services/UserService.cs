using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using Guid = System.Guid;
using UserProfile = Umbraco.Cms.Core.Models.Membership.UserProfile;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the UserService, which is an easy access to operations involving <see cref="IProfile" />,
///     <see cref="IMembershipUser" /> and eventually Backoffice Users.
/// </summary>
internal partial class UserService : RepositoryService, IUserService
{
    private readonly GlobalSettings _globalSettings;
    private readonly SecuritySettings _securitySettings;
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly UserEditorAuthorizationHelper _userEditorAuthorizationHelper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IEntityService _entityService;
    private readonly ILocalLoginSettingProvider _localLoginSettingProvider;
    private readonly IUserInviteSender _inviteSender;
    private readonly IUserForgotPasswordSender _forgotPasswordSender;
    private readonly MediaFileManager _mediaFileManager;
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IIsoCodeValidator _isoCodeValidator;
    private readonly IUserRepository _userRepository;
    private readonly ContentSettings _contentSettings;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public UserService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IUserRepository userRepository,
        IUserGroupRepository userGroupRepository,
        IOptions<GlobalSettings> globalSettings,
        IOptions<SecuritySettings> securitySettings,
        UserEditorAuthorizationHelper userEditorAuthorizationHelper,
        IServiceScopeFactory serviceScopeFactory,
        IEntityService entityService,
        ILocalLoginSettingProvider localLoginSettingProvider,
        IUserInviteSender inviteSender,
        MediaFileManager mediaFileManager,
        ITemporaryFileService temporaryFileService,
        IShortStringHelper shortStringHelper,
        IOptions<ContentSettings> contentSettings,
        IIsoCodeValidator isoCodeValidator,
        IUserForgotPasswordSender forgotPasswordSender,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _userRepository = userRepository;
        _userGroupRepository = userGroupRepository;
        _userEditorAuthorizationHelper = userEditorAuthorizationHelper;
        _serviceScopeFactory = serviceScopeFactory;
        _entityService = entityService;
        _localLoginSettingProvider = localLoginSettingProvider;
        _inviteSender = inviteSender;
        _mediaFileManager = mediaFileManager;
        _temporaryFileService = temporaryFileService;
        _shortStringHelper = shortStringHelper;
        _isoCodeValidator = isoCodeValidator;
        _forgotPasswordSender = forgotPasswordSender;
        _userIdKeyResolver = userIdKeyResolver;
        _globalSettings = globalSettings.Value;
        _securitySettings = securitySettings.Value;
        _contentSettings = contentSettings.Value;
    }

    /// <summary>
    ///     Checks in a set of permissions associated with a user for those related to a given nodeId
    /// </summary>
    /// <param name="permissions">The set of permissions</param>
    /// <param name="nodeId">The node Id</param>
    /// <param name="assignedPermissions">The permissions to return</param>
    /// <returns>True if permissions for the given path are found</returns>
    public static bool TryGetAssignedPermissionsForNode(
        IList<EntityPermission> permissions,
        int nodeId,
        out string assignedPermissions)
    {
        if (permissions.Any(x => x.EntityId == nodeId))
        {
            EntityPermission found = permissions.First(x => x.EntityId == nodeId);
            ISet<string> assignedPermissionsArray = found.AssignedPermissions;

            // Working with permissions assigned directly to a user AND to their groups, so maybe several per node
            // and we need to get the most permissive set
            foreach (EntityPermission permission in permissions.Where(x => x.EntityId == nodeId).Skip(1))
            {
                AddAdditionalPermissions(assignedPermissionsArray, permission.AssignedPermissions);
            }

            assignedPermissions = string.Join(string.Empty, assignedPermissionsArray);
            return true;
        }

        assignedPermissions = string.Empty;
        return false;
    }

    #region Implementation of IMembershipUserService

    /// <summary>
    ///     Checks if a User with the username exists
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns><c>True</c> if the User exists otherwise <c>False</c></returns>
    public bool Exists(string username)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userRepository.ExistsByUserName(username);
        }
    }

    /// <summary>
    ///     Creates a new User
    /// </summary>
    /// <remarks>The user will be saved in the database and returned with an Id</remarks>
    /// <param name="username">Username of the user to create</param>
    /// <param name="email">Email of the user to create</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    public IUser CreateUserWithIdentity(string username, string email) =>
        CreateUserWithIdentity(username, email, string.Empty);

    /// <summary>
    ///     Creates and persists a new <see cref="IUser" />
    /// </summary>
    /// <param name="username">Username of the <see cref="IUser" /> to create</param>
    /// <param name="email">Email of the <see cref="IUser" /> to create</param>
    /// <param name="passwordValue">
    ///     This value should be the encoded/encrypted/hashed value for the password that will be
    ///     stored in the database
    /// </param>
    /// <param name="memberTypeAlias">Not used for users</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    IUser IMembershipMemberService<IUser>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias) => CreateUserWithIdentity(username, email, passwordValue);

    /// <summary>
    ///     Creates and persists a new <see cref="IUser" />
    /// </summary>
    /// <param name="username">Username of the <see cref="IUser" /> to create</param>
    /// <param name="email">Email of the <see cref="IUser" /> to create</param>
    /// <param name="passwordValue">
    ///     This value should be the encoded/encrypted/hashed value for the password that will be
    ///     stored in the database
    /// </param>
    /// <param name="memberTypeAlias">Alias of the Type</param>
    /// <param name="isApproved">Is the member approved</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    IUser IMembershipMemberService<IUser>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias, bool isApproved) => CreateUserWithIdentity(username, email, passwordValue, isApproved);

    /// <summary>
    ///     Gets a User by its integer id
    /// </summary>
    /// <param name="id"><see cref="int" /> Id</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    [Obsolete("Please use GetAsync instead. Scheduled for removal in V15.")]
    public IUser? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            Guid userKey = _userIdKeyResolver.GetAsync(id).GetAwaiter().GetResult();
            return _userRepository.Get(userKey);
        }
    }

    /// <summary>
    ///     Creates and persists a Member
    /// </summary>
    /// <remarks>
    ///     Using this method will persist the Member object before its returned
    ///     meaning that it will have an Id available (unlike the CreateMember method)
    /// </remarks>
    /// <param name="username">Username of the Member to create</param>
    /// <param name="email">Email of the Member to create</param>
    /// <param name="passwordValue">
    ///     This value should be the encoded/encrypted/hashed value for the password that will be
    ///     stored in the database
    /// </param>
    /// <param name="isApproved">Is the user approved</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    private IUser CreateUserWithIdentity(string username, string email, string passwordValue, bool isApproved = true)
    {
        if (username == null)
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(username));
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        // TODO: PUT lock here!!
        User user;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var loginExists = _userRepository.ExistsByLogin(username);
            if (loginExists)
            {
                throw new ArgumentException("Login already exists"); // causes rollback
            }

            user = new User(_globalSettings)
            {
                Email = email,
                Language = _globalSettings.DefaultUILanguage,
                Name = username,
                RawPasswordValue = passwordValue,
                Username = username,
                IsLockedOut = false,
                IsApproved = isApproved,
            };

            var savingNotification = new UserSavingNotification(user, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return user;
            }

            _userRepository.Save(user);

            scope.Notifications.Publish(new UserSavedNotification(user, evtMsgs).WithStateFrom(savingNotification));
            scope.Complete();
        }

        return user;
    }

    /// <summary>
    ///     Gets an <see cref="IUser" /> by its provider key
    /// </summary>
    /// <param name="id">Id to use for retrieval</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    public IUser? GetByProviderKey(object id)
    {
        Attempt<int> asInt = id.TryConvertTo<int>();
        return asInt.Success ? GetById(asInt.Result) : null;
    }

    /// <summary>
    ///     Get an <see cref="IUser" /> by email
    /// </summary>
    /// <param name="email">Email to use for retrieval</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    public IUser? GetByEmail(string email)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();
        return backOfficeUserStore.GetByEmailAsync(email).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Get an <see cref="IUser" /> by username
    /// </summary>
    /// <param name="username">Username to use for retrieval</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    public IUser? GetByUsername(string? username)
    {
        if (username is null)
        {
            return null;
        }
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return backOfficeUserStore.GetByUserNameAsync(username).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Disables an <see cref="IUser" />
    /// </summary>
    /// <param name="membershipUser"><see cref="IUser" /> to disable</param>
    public void Delete(IUser membershipUser)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        backOfficeUserStore.DisableAsync(membershipUser).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Deletes or disables a User
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to delete</param>
    /// <param name="deletePermanently"><c>True</c> to permanently delete the user, <c>False</c> to disable the user</param>
    public void Delete(IUser user, bool deletePermanently)
    {
        if (deletePermanently == false)
        {
            Delete(user);
        }
        else
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                var deletingNotification = new UserDeletingNotification(user, evtMsgs);
                if (scope.Notifications.PublishCancelable(deletingNotification))
                {
                    scope.Complete();
                    return;
                }

                _userRepository.Delete(user);

                scope.Notifications.Publish(
                    new UserDeletedNotification(user, evtMsgs).WithStateFrom(deletingNotification));
                scope.Complete();
            }
        }
    }

    /// <summary>
    /// Saves an <see cref="IUser" />
    /// </summary>
    /// <param name="entity"><see cref="IUser" /> to Save</param>
    public void Save(IUser entity)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        backOfficeUserStore.SaveAsync(entity).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Saves an <see cref="IUser" />
    /// </summary>
    /// <param name="entity"><see cref="IUser" /> to Save</param>
    public async Task<UserOperationStatus> SaveAsync(IUser entity)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return await backOfficeUserStore.SaveAsync(entity);
    }

    /// <summary>
    ///     Saves a list of <see cref="IUser" /> objects
    /// </summary>
    /// <param name="entities"><see cref="IEnumerable{IUser}" /> to save</param>
    public void Save(IEnumerable<IUser> entities)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        IUser[] entitiesA = entities.ToArray();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new UserSavingNotification(entitiesA, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            foreach (IUser user in entitiesA)
            {
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    throw new ArgumentException("Empty username.", nameof(entities));
                }

                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    throw new ArgumentException("Empty name.", nameof(entities));
                }

                _userRepository.Save(user);
            }

            scope.Notifications.Publish(
                new UserSavedNotification(entitiesA, evtMsgs).WithStateFrom(savingNotification));

            // commit the whole lot in one go
            scope.Complete();
        }
    }

    /// <summary>
    ///     Finds a list of <see cref="IUser" /> objects by a partial email string
    /// </summary>
    /// <param name="emailStringToMatch">Partial email string to match</param>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.StartsWith" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{IUser}" />
    /// </returns>
    public IEnumerable<IUser> FindByEmail(string emailStringToMatch, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUser> query = Query<IUser>();

            switch (matchType)
            {
                case StringPropertyMatchType.Exact:
                    query?.Where(member => member.Email.Equals(emailStringToMatch));
                    break;
                case StringPropertyMatchType.Contains:
                    query?.Where(member => member.Email.Contains(emailStringToMatch));
                    break;
                case StringPropertyMatchType.StartsWith:
                    query?.Where(member => member.Email.StartsWith(emailStringToMatch));
                    break;
                case StringPropertyMatchType.EndsWith:
                    query?.Where(member => member.Email.EndsWith(emailStringToMatch));
                    break;
                case StringPropertyMatchType.Wildcard:
                    query?.Where(member => member.Email.SqlWildcard(emailStringToMatch, TextColumnType.NVarchar));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchType));
            }

            return _userRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Email);
        }
    }

    /// <summary>
    ///     Finds a list of <see cref="IUser" /> objects by a partial username
    /// </summary>
    /// <param name="login">Partial username to match</param>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <param name="matchType">
    ///     The type of match to make as <see cref="StringPropertyMatchType" />. Default is
    ///     <see cref="StringPropertyMatchType.StartsWith" />
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{IUser}" />
    /// </returns>
    public IEnumerable<IUser> FindByUsername(string login, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUser> query = Query<IUser>();

            switch (matchType)
            {
                case StringPropertyMatchType.Exact:
                    query?.Where(member => member.Username.Equals(login));
                    break;
                case StringPropertyMatchType.Contains:
                    query?.Where(member => member.Username.Contains(login));
                    break;
                case StringPropertyMatchType.StartsWith:
                    query?.Where(member => member.Username.StartsWith(login));
                    break;
                case StringPropertyMatchType.EndsWith:
                    query?.Where(member => member.Username.EndsWith(login));
                    break;
                case StringPropertyMatchType.Wildcard:
                    query?.Where(member => member.Email.SqlWildcard(login, TextColumnType.NVarchar));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchType));
            }

            return _userRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Username);
        }
    }

    /// <summary>
    ///     Gets the total number of Users based on the count type
    /// </summary>
    /// <remarks>
    ///     The way the Online count is done is the same way that it is done in the MS SqlMembershipProvider - We query for any
    ///     members
    ///     that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact
    ///     science
    ///     but that is how MS have made theirs so we'll follow that principal.
    /// </remarks>
    /// <param name="countType"><see cref="MemberCountType" /> to count by</param>
    /// <returns><see cref="int" /> with number of Users for passed in type</returns>
    public int GetCount(MemberCountType countType)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IUser>? query;

            switch (countType)
            {
                case MemberCountType.All:
                    query = Query<IUser>();
                    break;
                case MemberCountType.LockedOut:
                    query = Query<IUser>()?.Where(x => x.IsLockedOut);
                    break;
                case MemberCountType.Approved:
                    query = Query<IUser>()?.Where(x => x.IsApproved);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(countType));
            }

            return _userRepository.GetCountByQuery(query);
        }
    }

    public Guid CreateLoginSession(int userId, string requestingIpAddress)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            Guid session = _userRepository.CreateLoginSession(userId, requestingIpAddress);
            scope.Complete();
            return session;
        }
    }

    public int ClearLoginSessions(int userId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var count = _userRepository.ClearLoginSessions(userId);
            scope.Complete();
            return count;
        }
    }

    public void ClearLoginSession(Guid sessionId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _userRepository.ClearLoginSession(sessionId);
            scope.Complete();
        }
    }

    public bool ValidateLoginSession(int userId, Guid sessionId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var result = _userRepository.ValidateLoginSession(userId, sessionId);
            scope.Complete();
            return result;
        }
    }

    public IDictionary<UserState, int> GetUserStates()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userRepository.GetUserStates();
        }
    }

    /// <inheritdoc/>
    public async Task<Attempt<UserCreationResult, UserOperationStatus>> CreateAsync(Guid performingUserKey, UserCreateModel model, bool approveUser = false)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

        IUser? performingUser = await GetAsync(performingUserKey);

        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUser, new UserCreationResult());
        }

        IUserGroup[] userGroups = _userGroupRepository.GetMany().Where(x=>model.UserGroupKeys.Contains(x.Key)).ToArray();

        if (userGroups.Length != model.UserGroupKeys.Count)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUserGroup, new UserCreationResult());
        }

        UserOperationStatus result = await ValidateUserCreateModel(model);
        if (result != UserOperationStatus.Success)
        {
            return Attempt.FailWithStatus(result, new UserCreationResult());
        }

        Attempt<string?> authorizationAttempt = _userEditorAuthorizationHelper.IsAuthorized(
            performingUser,
            null,
            null,
            null,
            userGroups.Select(x => x.Alias));

        if (authorizationAttempt.Success is false)
        {
            return Attempt.FailWithStatus(UserOperationStatus.Unauthorized, new UserCreationResult());
        }

        ICoreBackOfficeUserManager backOfficeUserManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();

        IdentityCreationResult identityCreationResult = await backOfficeUserManager.CreateAsync(model);

        if (identityCreationResult.Succeded is false)
        {
            // If we fail from something in Identity we can't know exactly why, so we have to resolve to returning an unknown failure.
            // But there should be more information in the message.
            return Attempt.FailWithStatus(
                UserOperationStatus.UnknownFailure,
                new UserCreationResult { Error = new ValidationResult(identityCreationResult.ErrorMessage) });
        }

        // The user is now created, so we can fetch it to map it to a result model with our generated password.
        // and set it to being approved
        IBackOfficeUserStore backOfficeUserStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();
        IUser? createdUser = await backOfficeUserStore.GetByEmailAsync(model.Email);

        if (createdUser is null)
        {
            // This really shouldn't happen, we literally just created the user
            throw new PanicException("Was unable to get user after creating it");
        }

        createdUser.IsApproved = approveUser;

        foreach (IUserGroup userGroup in userGroups)
        {
            createdUser.AddGroup(userGroup.ToReadOnlyGroup());
        }

        await backOfficeUserStore.SaveAsync(createdUser);

        scope.Complete();

        var creationResult = new UserCreationResult
        {
            CreatedUser = createdUser,
            InitialPassword = identityCreationResult.InitialPassword
        };

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, creationResult);
    }

    public async Task<Attempt<UserOperationStatus>> SendResetPasswordEmailAsync(string userEmail)
    {
        if (_forgotPasswordSender.CanSend() is false)
        {
            return Attempt.Fail(UserOperationStatus.CannotPasswordReset);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

        ICoreBackOfficeUserManager userManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        IUser? user = await userStore.GetByEmailAsync(userEmail);

        if (user is null)
        {
            return Attempt.Fail(UserOperationStatus.UserNotFound);
        }

        IForgotPasswordUriProvider uriProvider = serviceScope.ServiceProvider.GetRequiredService<IForgotPasswordUriProvider>();
        Attempt<Uri, UserOperationStatus> uriAttempt = await uriProvider.CreateForgotPasswordUriAsync(user);
        if (uriAttempt.Success is false)
        {
            return Attempt.Fail(uriAttempt.Status);
        }

        var message = new UserForgotPasswordMessage
        {
            ForgotPasswordUri = uriAttempt.Result,
            Recipient = user,
        };
        await _forgotPasswordSender.SendForgotPassword(message);

        userManager.NotifyForgotPasswordRequested(new ClaimsPrincipal(), user.Id.ToString()); //A bit of a hack, but since this method will be used without a signed in user, there is no real principal anyway.

        scope.Complete();

        return Attempt.Succeed(UserOperationStatus.Success);
    }
    public async Task<Attempt<UserInvitationResult, UserOperationStatus>> InviteAsync(Guid performingUserKey, UserInviteModel model)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

        IUser? performingUser = await GetAsync(performingUserKey);

        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUser, new UserInvitationResult());
        }

        IUserGroup[] userGroups = _userGroupRepository.GetMany().Where(x => model.UserGroupKeys.Contains(x.Key)).ToArray();

        if (userGroups.Length != model.UserGroupKeys.Count)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUserGroup, new UserInvitationResult());
        }

        UserOperationStatus validationResult = await ValidateUserCreateModel(model);

        if (validationResult is not UserOperationStatus.Success)
        {
            return Attempt.FailWithStatus(validationResult, new UserInvitationResult());
        }

        Attempt<string?> authorizationAttempt = _userEditorAuthorizationHelper.IsAuthorized(
            performingUser,
            null,
            null,
            null,
            userGroups.Select(x => x.Alias));

        if (authorizationAttempt.Success is false)
        {
            return Attempt.FailWithStatus(UserOperationStatus.Unauthorized, new UserInvitationResult());
        }

        if (_inviteSender.CanSendInvites() is false)
        {
            return Attempt.FailWithStatus(UserOperationStatus.CannotInvite, new UserInvitationResult());
        }

        ICoreBackOfficeUserManager userManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        IdentityCreationResult creationResult = await userManager.CreateForInvite(model);
        if (creationResult.Succeded is false)
        {
            // If we fail from something in Identity we can't know exactly why, so we have to resolve to returning an unknown failure.
            // But there should be more information in the message.
            return Attempt.FailWithStatus(
                UserOperationStatus.UnknownFailure,
                new UserInvitationResult { Error = new ValidationResult(creationResult.ErrorMessage) });
        }

        IUser? invitedUser = await userStore.GetByEmailAsync(model.Email);

        if (invitedUser is null)
        {
            // This really shouldn't happen, we literally just created the user
            throw new PanicException("Was unable to get user after creating it");
        }

        invitedUser.InvitedDate = DateTime.Now;
        invitedUser.ClearGroups();
        foreach(IUserGroup userGroup in userGroups)
        {
            invitedUser.AddGroup(userGroup.ToReadOnlyGroup());
        }

        await userStore.SaveAsync(invitedUser);

        Attempt<UserInvitationResult, UserOperationStatus> invitationAttempt = await SendInvitationAsync(performingUser, serviceScope, invitedUser, model.Message);

        scope.Complete();

        return invitationAttempt;
    }

    public async Task<Attempt<UserInvitationResult, UserOperationStatus>> ResendInvitationAsync(Guid performingUserKey, UserResendInviteModel model)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

        IUser? performingUser = await GetAsync(performingUserKey);
        if (performingUser == null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUser, new UserInvitationResult());
        }

        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();
        IUser? invitedUser = await userStore.GetAsync(model.InvitedUserKey);
        if (invitedUser == null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, new UserInvitationResult());
        }

        if (invitedUser.UserState != UserState.Invited)
        {
            return Attempt.FailWithStatus(UserOperationStatus.NotInInviteState, new UserInvitationResult());
        }

        // re-inviting so update invite date
        invitedUser.InvitedDate = DateTime.Now;
        await userStore.SaveAsync(invitedUser);

        Attempt<UserInvitationResult, UserOperationStatus> invitationAttempt = await SendInvitationAsync(performingUser, serviceScope, invitedUser, model.Message);
        scope.Complete();

        return invitationAttempt;
    }

    private async Task<Attempt<UserInvitationResult, UserOperationStatus>> SendInvitationAsync(IUser performingUser, IServiceScope serviceScope, IUser invitedUser, string? message)
    {
        IInviteUriProvider inviteUriProvider = serviceScope.ServiceProvider.GetRequiredService<IInviteUriProvider>();
        Attempt<Uri, UserOperationStatus> inviteUriAttempt = await inviteUriProvider.CreateInviteUriAsync(invitedUser);
        if (inviteUriAttempt.Success is false)
        {
            return Attempt.FailWithStatus(inviteUriAttempt.Status, new UserInvitationResult());
        }

        var invitation = new UserInvitationMessage
        {
            InviteUri = inviteUriAttempt.Result,
            Message = message ?? string.Empty,
            Recipient = invitedUser,
            Sender = performingUser,
        };
        await _inviteSender.InviteUser(invitation);

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, new UserInvitationResult { InvitedUser = invitedUser });
    }

    private async Task<UserOperationStatus> ValidateUserCreateModel(UserCreateModel model)
    {
        if (_securitySettings.UsernameIsEmail && model.UserName != model.Email)
        {
            return UserOperationStatus.UserNameIsNotEmail;
        }
        if (model.Email.IsEmail() is false)
        {
            return UserOperationStatus.InvalidEmail;
        }

        if (model.Id is not null && await GetAsync(model.Id.Value) is not null)
        {
            return UserOperationStatus.DuplicateId;
        }

        if (GetByEmail(model.Email) is not null)
        {
            return UserOperationStatus.DuplicateEmail;
        }

        if (GetByUsername(model.UserName) is not null)
        {
            return UserOperationStatus.DuplicateUserName;
        }

        if(model.UserGroupKeys.Count == 0)
        {
            return UserOperationStatus.NoUserGroup;
        }

        return UserOperationStatus.Success;
    }

    public async Task<Attempt<IUser?, UserOperationStatus>> UpdateAsync(Guid performingUserKey, UserUpdateModel model)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        IUser? existingUser = await userStore.GetAsync(model.ExistingUserKey);

        if (existingUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, existingUser);
        }

        IUser? performingUser = await userStore.GetAsync(performingUserKey);

        if (performingUser is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.MissingUser, existingUser);
        }

        // User names can only contain the configured allowed characters. This is validated by ASP.NET Identity on create
        // as the setting is applied to the BackOfficeIdentityOptions, but we need to check ourselves for updates.
        var allowedUserNameCharacters = _securitySettings.AllowedUserNameCharacters;
        if (model.UserName.Any(c => allowedUserNameCharacters.Contains(c) == false))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.InvalidUserName, existingUser);
        }

        IEnumerable<IUserGroup> allUserGroups = _userGroupRepository.GetMany().ToArray();
        var userGroups = allUserGroups.Where(x => model.UserGroupKeys.Contains(x.Key)).ToHashSet();

        if (userGroups.Count != model.UserGroupKeys.Count)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.MissingUserGroup, existingUser);
        }

        // We're de-admining a user, we need to ensure that this would not leave the admin group empty.
        if (existingUser.IsAdmin() && model.UserGroupKeys.Contains(Constants.Security.AdminGroupKey) is false)
        {
            IUserGroup? adminGroup = allUserGroups.FirstOrDefault(x => x.Key == Constants.Security.AdminGroupKey);
            if (adminGroup?.UserCount == 1)
            {
                scope.Complete();
                return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.AdminUserGroupMustNotBeEmpty, existingUser);
            }
        }

        // We have to resolve the keys to ids to be compatible with the repository, this could be done in the factory,
        // but I'd rather keep the ids out of the service API as much as possible.
        List<int>? startContentIds = GetIdsFromKeys(model.ContentStartNodeKeys, UmbracoObjectTypes.Document);

        if (startContentIds is null || startContentIds.Count != model.ContentStartNodeKeys.Count)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.ContentStartNodeNotFound, existingUser);
        }

        List<int>? startMediaIds = GetIdsFromKeys(model.MediaStartNodeKeys, UmbracoObjectTypes.Media);

        if (startMediaIds is null || startMediaIds.Count != model.MediaStartNodeKeys.Count)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.MediaStartNodeNotFound, existingUser);
        }

        if (model.HasContentRootAccess)
        {
            startContentIds.Add(Constants.System.Root);
        }

        if (model.HasMediaRootAccess)
        {
            startMediaIds.Add(Constants.System.Root);
        }

        Attempt<string?> isAuthorized = _userEditorAuthorizationHelper.IsAuthorized(
            performingUser,
            existingUser,
            startContentIds,
            startMediaIds,
            userGroups.Select(x => x.Alias));

        if (isAuthorized.Success is false)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.Unauthorized, existingUser);
        }

        UserOperationStatus validationStatus = ValidateUserUpdateModel(existingUser, model);
        if (validationStatus is not UserOperationStatus.Success)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(validationStatus, existingUser);
        }

        // Now that we're all authorized and validated we can actually map over changes and update the user
        // TODO: This probably shouldn't live here, once we have user content start nodes as keys this can be moved to a mapper
        // Alternatively it should be a map definition, but then we need to use entity service to resolve the IDs
        // TODO: Add auditing
        IUser updated = MapUserUpdate(model, userGroups, existingUser, startContentIds, startMediaIds);
        UserOperationStatus saveStatus = await userStore.SaveAsync(updated);

        if (saveStatus is not UserOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IUser?, UserOperationStatus>(saveStatus, existingUser);
        }

        scope.Complete();
        return Attempt.SucceedWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.Success, updated);

    }

    public async Task<UserOperationStatus> SetAvatarAsync(Guid userKey, Guid temporaryFileKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? user = await GetAsync(userKey);
        if (user is null)
        {
            return UserOperationStatus.UserNotFound;
        }

        TemporaryFileModel? avatarTemporaryFile = await _temporaryFileService.GetAsync(temporaryFileKey);
        _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileKey, ScopeProvider);

        if (avatarTemporaryFile is null)
        {
            return UserOperationStatus.AvatarFileNotFound;
        }

        const string allowedAvatarFileTypes = "jpeg,jpg,gif,bmp,png,tiff,tif,webp";

        // This shouldn't really be necessary since we're just gonna use it to generate a hash, but that's how it was.
        var avatarFileName = avatarTemporaryFile.FileName.ToSafeFileName(_shortStringHelper);
        var extension = Path.GetExtension(avatarFileName)[1..];
        if(allowedAvatarFileTypes.Contains(extension) is false || _contentSettings.DisallowedUploadedFileExtensions.Contains(extension))
        {
            return UserOperationStatus.InvalidAvatar;
        }

        // Generate a path from known data, we don't want this to be guessable
        var avatarHash = $"{user.Key}{avatarFileName}".GenerateHash<SHA1>();
        var avatarPath = $"UserAvatars/{avatarHash}.{extension}";

        await using (Stream fileStream = avatarTemporaryFile.OpenReadStream())
        {
            _mediaFileManager.FileSystem.AddFile(avatarPath, fileStream, true);
        }

        user.Avatar = avatarPath;
        await SaveAsync(user);

        scope.Complete();
        return UserOperationStatus.Success;
    }

    private IUser MapUserUpdate(
        UserUpdateModel source,
        ISet<IUserGroup> sourceUserGroups,
        IUser target,
        List<int> startContentIds,
        List<int> startMediaIds)
    {
        target.Name = source.Name;
        target.Language = source.LanguageIsoCode;
        target.Email = source.Email;
        target.Username = source.UserName;
        target.StartContentIds = startContentIds.ToArray();
        target.StartMediaIds = startMediaIds.ToArray();

        target.ClearGroups();
        foreach (IUserGroup group in sourceUserGroups)
        {
            target.AddGroup(group.ToReadOnlyGroup());
        }

        return target;
    }

    private UserOperationStatus ValidateUserUpdateModel(IUser existingUser, UserUpdateModel model)
    {
        if (_isoCodeValidator.IsValid(model.LanguageIsoCode) is false)
        {
            return UserOperationStatus.InvalidIsoCode;
        }

        // We need to check if there's any Deny Local login providers present, if so we need to ensure that the user's email address cannot be changed.
        if (_localLoginSettingProvider.HasDenyLocalLogin() && model.Email != existingUser.Email)
        {
            return UserOperationStatus.EmailCannotBeChanged;
        }

        if (_securitySettings.UsernameIsEmail && model.UserName != model.Email)
        {
            return UserOperationStatus.UserNameIsNotEmail;
        }

        if (model.Email.IsEmail() is false)
        {
            return UserOperationStatus.InvalidEmail;
        }

        IUser? existing = GetByEmail(model.Email);
        if (existing is not null && existing.Key != existingUser.Key)
        {
            return UserOperationStatus.DuplicateEmail;
        }

        // In case the user has updated their username to be a different email, but not their actually email
        // we have to try and get the user by email using their username, and ensure we don't get any collisions.
        existing = GetByEmail(model.UserName);
        if (existing is not null && existing.Key != existingUser.Key)
        {
            return UserOperationStatus.DuplicateUserName;
        }

        existing = GetByUsername(model.UserName);
        if (existing is not null && existing.Key != existingUser.Key)
        {
            return UserOperationStatus.DuplicateUserName;
        }

        return UserOperationStatus.Success;
    }

    private List<int>? GetIdsFromKeys(IEnumerable<Guid>? guids, UmbracoObjectTypes type)
    {
        var keys = guids?
            .Select(x => _entityService.GetId(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result)
            .ToList();

        return keys;
    }

    public async Task<Attempt<PasswordChangedModel, UserOperationStatus>> ChangePasswordAsync(Guid performingUserKey, ChangeUserPasswordModel model)
    {
        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        IUser? user = await userStore.GetAsync(model.UserKey);
        if (user is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, new PasswordChangedModel());
        }

        if (user.Kind != UserKind.Default)
        {
            return Attempt.FailWithStatus(UserOperationStatus.InvalidUserType, new PasswordChangedModel());
        }

        IUser? performingUser = await userStore.GetAsync(performingUserKey);
        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUser, new PasswordChangedModel());
        }

        // require old password for self change when outside of invite or resetByToken flows
        if (performingUser.UserState != UserState.Invited && performingUser.Username == user.Username && string.IsNullOrEmpty(model.OldPassword) && string.IsNullOrEmpty(model.ResetPasswordToken))
        {
            return Attempt.FailWithStatus(UserOperationStatus.SelfOldPasswordRequired, new PasswordChangedModel());
        }

        if (performingUser.IsAdmin() is false && user.IsAdmin())
        {
            return Attempt.FailWithStatus(UserOperationStatus.Forbidden, new PasswordChangedModel());
        }

        if (string.IsNullOrEmpty(model.ResetPasswordToken) is false)
        {
            Attempt<UserOperationStatus> verifyPasswordResetAsync = await VerifyPasswordResetAsync(model.UserKey, model.ResetPasswordToken);
            if (verifyPasswordResetAsync.Result != UserOperationStatus.Success)
            {
                return Attempt.FailWithStatus(verifyPasswordResetAsync.Result, new PasswordChangedModel());
            }
        }

        IBackOfficePasswordChanger passwordChanger = serviceScope.ServiceProvider.GetRequiredService<IBackOfficePasswordChanger>();
        Attempt<PasswordChangedModel?> result = await passwordChanger.ChangeBackOfficePassword(
            new ChangeBackOfficeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            OldPassword = model.OldPassword,
            User = user,
            ResetPasswordToken = model.ResetPasswordToken,
        }, performingUser);

        if (result.Success is false)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UnknownFailure, result.Result ?? new PasswordChangedModel());
        }

        scope.Complete();
        return Attempt.SucceedWithStatus(UserOperationStatus.Success, result.Result ?? new PasswordChangedModel());
    }

    public async Task<Attempt<PagedModel<IUser>?, UserOperationStatus>> GetAllAsync(Guid performingUserKey, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? requestingUser = await GetAsync(performingUserKey);

        if (requestingUser is null)
        {
            return Attempt.FailWithStatus<PagedModel<IUser>?, UserOperationStatus>(UserOperationStatus.MissingUser, null);
        }

        UserFilter baseFilter = CreateBaseUserFilter(requestingUser, out IQuery<IUser> query);

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out long pageNumber, out int pageSize);

        HashSet<string> excludeUserGroupAliases = new();
        if (baseFilter.ExcludeUserGroups is not null)
        {
            Attempt<IEnumerable<string>, UserOperationStatus> userGroupKeyConversionAttempt =
                GetUserGroupAliasesFromKeys(baseFilter.ExcludeUserGroups);


            if (userGroupKeyConversionAttempt.Success is false)
            {
                return Attempt.FailWithStatus<PagedModel<IUser>?, UserOperationStatus>(UserOperationStatus.MissingUserGroup, null);
            }

            excludeUserGroupAliases = new HashSet<string>(userGroupKeyConversionAttempt.Result);
        }

        IEnumerable<IUser> result = _userRepository.GetPagedResultsByQuery(
            null,
            pageNumber,
            pageSize,
            out long totalRecords,
            x => x.Username,
            excludeUserGroups: excludeUserGroupAliases.ToArray(),
            filter: query,
            userState: baseFilter.IncludeUserStates?.ToArray());

        var pagedResult = new PagedModel<IUser> { Items = result, Total = totalRecords };

        scope.Complete();
        return Attempt.SucceedWithStatus<PagedModel<IUser>?, UserOperationStatus>(UserOperationStatus.Success, pagedResult);
    }

    public async Task<Attempt<PagedModel<IUser>, UserOperationStatus>> FilterAsync(
        Guid userKey,
        UserFilter filter,
        int skip = 0,
        int take = 100,
        UserOrder orderBy = UserOrder.UserName,
        Direction orderDirection = Direction.Ascending)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? requestingUser = await GetAsync(userKey);

        if (requestingUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUser, new PagedModel<IUser>());
        }

        UserFilter baseFilter = CreateBaseUserFilter(requestingUser, out IQuery<IUser> baseQuery);

        UserFilter mergedFilter = filter.Merge(baseFilter);

        // TODO: We should have a repository method that accepts keys so we don't have to do this conversion
        HashSet<string>? excludedUserGroupAliases = null;
        if (mergedFilter.ExcludeUserGroups is not null)
        {
            Attempt<IEnumerable<string>, UserOperationStatus> userGroupKeyConversionAttempt =
                GetUserGroupAliasesFromKeys(mergedFilter.ExcludeUserGroups);


            if (userGroupKeyConversionAttempt.Success is false)
            {
                return Attempt.FailWithStatus(UserOperationStatus.MissingUserGroup, new PagedModel<IUser>());
            }

            excludedUserGroupAliases = new HashSet<string>(userGroupKeyConversionAttempt.Result);
        }

        string[]? includedUserGroupAliases = null;
        if (mergedFilter.IncludedUserGroups is not null)
        {
            Attempt<IEnumerable<string>, UserOperationStatus> userGroupKeyConversionAttempt = GetUserGroupAliasesFromKeys(mergedFilter.IncludedUserGroups);

            if (userGroupKeyConversionAttempt.Success is false)
            {
                return Attempt.FailWithStatus(UserOperationStatus.MissingUserGroup, new PagedModel<IUser>());
            }

            includedUserGroupAliases = userGroupKeyConversionAttempt.Result.ToArray();
        }


        if (mergedFilter.NameFilters is not null)
        {
            foreach (var nameFilter in mergedFilter.NameFilters)
            {
                baseQuery.Where(x => x.Name!.Contains(nameFilter) || x.Username.Contains(nameFilter));
            }
        }

        ISet<UserState>? includeUserStates = null;

        // The issue is that this is a limiting filter we have to ensure that it still follows our rules
        // So I'm not allowed to ask for the disabled users if the setting has been flipped
        if (baseFilter.IncludeUserStates is null || baseFilter.IncludeUserStates.Count == 0)
        {
            includeUserStates = filter.IncludeUserStates;
        }
        else
        {
            includeUserStates = new HashSet<UserState>(filter.IncludeUserStates!);
            includeUserStates.IntersectWith(baseFilter.IncludeUserStates);

            // This means that we've only chosen to include a user state that is not allowed, so we'll return an empty result
            if(includeUserStates.Count == 0)
            {
                return Attempt.SucceedWithStatus(UserOperationStatus.Success, new PagedModel<IUser>());
            }
        }


        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out long pageNumber, out int pageSize);
        Expression<Func<IUser, object?>> orderByExpression = GetOrderByExpression(orderBy);

        // TODO: We should create a Query method on the repo that allows to filter by aliases.
        IEnumerable<IUser> result = _userRepository.GetPagedResultsByQuery(
            null,
            pageNumber,
            pageSize,
            out long totalRecords,
            orderByExpression,
            orderDirection,
            includedUserGroupAliases?.ToArray(),
            excludedUserGroupAliases?.ToArray(),
            includeUserStates?.ToArray(),
            baseQuery);

        scope.Complete();

        var model = new PagedModel<IUser> { Items = result, Total = totalRecords };

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, model);
    }

    /// <summary>
    /// Creates a base user filter which ensures our rules are followed, I.E. Only admins can se other admins.
    /// </summary>
    /// <remarks>
    /// We return the query as an out parameter instead of having it in the intermediate object because a two queries cannot be merged into one.
    /// </remarks>
    /// <returns></returns>
    private UserFilter CreateBaseUserFilter(IUser performingUser, out IQuery<IUser> baseQuery)
    {
        var filter = new UserFilter();
        baseQuery = Query<IUser>();

        // Only super can see super
        if (performingUser.IsSuper() is false)
        {
            baseQuery.Where(x => x.Key != Constants.Security.SuperUserKey);
        }

        // Only admins can see admins
        if (performingUser.IsAdmin() is false)
        {
            filter.ExcludeUserGroups = new HashSet<Guid> { Constants.Security.AdminGroupKey };
        }

        if (_securitySettings.HideDisabledUsersInBackOffice)
        {
            filter.IncludeUserStates = new HashSet<UserState> { UserState.Active, UserState.Invited, UserState.LockedOut, UserState.Inactive };
        }

        return filter;
    }

    private Attempt<IEnumerable<string>, UserOperationStatus> GetUserGroupAliasesFromKeys(IEnumerable<Guid> userGroupKeys)
    {
        var aliases = new List<string>();

        foreach (Guid key in userGroupKeys)
        {
            IUserGroup? group = _userGroupRepository.Get(Query<IUserGroup>().Where(x => x.Key == key)).FirstOrDefault();
            if (group is null)
            {
                return Attempt.FailWithStatus(UserOperationStatus.MissingUserGroup, Enumerable.Empty<string>());
            }

            aliases.Add(group.Alias);
        }

        return Attempt.SucceedWithStatus<IEnumerable<string>, UserOperationStatus>(UserOperationStatus.Success, aliases);
    }

    private Expression<Func<IUser, object?>> GetOrderByExpression(UserOrder orderBy)
    {
        return orderBy switch
        {
            UserOrder.UserName => x => x.Username,
            UserOrder.Language => x => x.Language,
            UserOrder.Name => x => x.Name,
            UserOrder.Email => x => x.Email,
            UserOrder.Id => x => x.Id,
            UserOrder.CreateDate => x => x.CreateDate,
            UserOrder.UpdateDate => x => x.UpdateDate,
            UserOrder.IsApproved => x => x.IsApproved,
            UserOrder.IsLockedOut => x => x.IsLockedOut,
            UserOrder.LastLoginDate => x => x.LastLoginDate,
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };
    }

    public async Task<UserOperationStatus> DeleteAsync(Guid performingUserKey, ISet<Guid> keys)
    {
        if(keys.Any() is false)
        {
            return UserOperationStatus.Success;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUser? performingUser = await GetAsync(performingUserKey);

        if (performingUser is null)
        {
            return UserOperationStatus.MissingUser;
        }

        if (keys.Contains(performingUser.Key))
        {
            return UserOperationStatus.CannotDeleteSelf;
        }

        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();
        IUser[] usersToDisable = (await userStore.GetUsersAsync(keys.ToArray())).ToArray();

        if (usersToDisable.Length != keys.Count)
        {
            return UserOperationStatus.UserNotFound;
        }

        foreach (IUser user in usersToDisable)
        {
            // Check user hasn't logged in. If they have they may have made content changes which will mean
            // the Id is associated with audit trails, versions etc. and can't be removed.
            if (user.LastLoginDate is not null && user.LastLoginDate != default(DateTime))
            {
                return UserOperationStatus.CannotDelete;
            }

            user.IsApproved = false;
            user.InvitedDate = null;

            Delete(user, true);
        }

        scope.Complete();
        return UserOperationStatus.Success;
    }

    public async Task<UserOperationStatus> DisableAsync(Guid performingUserKey, ISet<Guid> keys)
    {
        if(keys.Any() is false)
        {
            return UserOperationStatus.Success;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUser? performingUser = await GetAsync(performingUserKey);

        if (performingUser is null)
        {
            return UserOperationStatus.MissingUser;
        }

        if (keys.Contains(performingUser.Key))
        {
            return UserOperationStatus.CannotDisableSelf;
        }

        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();
        IUser[] usersToDisable = (await userStore.GetUsersAsync(keys.ToArray())).ToArray();

        if (usersToDisable.Length != keys.Count)
        {
            return UserOperationStatus.UserNotFound;
        }

        foreach (IUser user in usersToDisable)
        {
            if (user.UserState is UserState.Invited)
            {
                return UserOperationStatus.CannotDisableInvitedUser;
            }

            user.IsApproved = false;
            user.InvitedDate = null;
        }

        Save(usersToDisable);

        scope.Complete();
        return UserOperationStatus.Success;
    }

    public async Task<UserOperationStatus> EnableAsync(Guid performingUserKey, ISet<Guid> keys)
    {
        if(keys.Any() is false)
        {
            return UserOperationStatus.Success;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUser? performingUser = await GetAsync(performingUserKey);

        if (performingUser is null)
        {
            return UserOperationStatus.MissingUser;
        }

        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();
        IUser[] usersToEnable = (await userStore.GetUsersAsync(keys.ToArray())).ToArray();

        if (usersToEnable.Length != keys.Count)
        {
            return UserOperationStatus.UserNotFound;
        }

        foreach (IUser user in usersToEnable)
        {
            user.IsApproved = true;
        }

        Save(usersToEnable);

        scope.Complete();
        return UserOperationStatus.Success;
    }

    public async Task<UserOperationStatus> ClearAvatarAsync(Guid userKey)
    {
        IUser? user = await GetAsync(userKey);

        if (user is null)
        {
            return UserOperationStatus.UserNotFound;
        }

        if (string.IsNullOrWhiteSpace(user.Avatar))
        {
            // Nothing to do
            return UserOperationStatus.Success;
        }

        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        string filePath = user.Avatar;
        user.Avatar = null;
        UserOperationStatus result = await backOfficeUserStore.SaveAsync(user);

        if (result is not UserOperationStatus.Success)
        {
            return result;
        }

        if (_mediaFileManager.FileSystem.FileExists(filePath))
        {
            _mediaFileManager.FileSystem.DeleteFile(filePath);
        }

        return UserOperationStatus.Success;
    }

    public async Task<Attempt<UserUnlockResult, UserOperationStatus>> UnlockAsync(Guid performingUserKey, params Guid[] keys)
    {
        if (keys.Length == 0)
        {
            return Attempt.SucceedWithStatus(UserOperationStatus.Success, new UserUnlockResult());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUser? performingUser = await GetAsync(performingUserKey);

        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MissingUser, new UserUnlockResult());
        }

        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        ICoreBackOfficeUserManager manager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();
        IBackOfficeUserStore userStore = serviceScope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        IEnumerable<IUser> usersToUnlock = await userStore.GetUsersAsync(keys);

        foreach (IUser user in usersToUnlock)
        {
            Attempt<UserUnlockResult, UserOperationStatus> result = await manager.UnlockUser(user);
            if (result.Success is false)
            {
                return Attempt.FailWithStatus(UserOperationStatus.UnknownFailure, result.Result);
            }
        }

        scope.Complete();
        return Attempt.SucceedWithStatus(UserOperationStatus.Success, new UserUnlockResult());
    }

    public IEnumerable<IUser> GetAll(long pageIndex, int pageSize, out long totalRecords, string orderBy, Direction orderDirection, UserState[]? userState = null, string[]? userGroups = null, string? filter = null)
    {
        IQuery<IUser>? filterQuery = null;
        if (filter.IsNullOrWhiteSpace() == false)
        {
            filterQuery = Query<IUser>()?.Where(x =>
                (x.Name != null && x.Name.Contains(filter!)) || x.Username.Contains(filter!));
        }

        return GetAll(pageIndex, pageSize, out totalRecords, orderBy, orderDirection, userState, userGroups, null, filterQuery);
    }

    public IEnumerable<IUser> GetAll(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string orderBy,
        Direction orderDirection,
        UserState[]? userState = null,
        string[]? includeUserGroups = null,
        string[]? excludeUserGroups = null,
        IQuery<IUser>? filter = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            Expression<Func<IUser, object?>> sort;
            switch (orderBy.ToUpperInvariant())
            {
                case "USERNAME":
                    sort = member => member.Username;
                    break;
                case "LANGUAGE":
                    sort = member => member.Language;
                    break;
                case "NAME":
                    sort = member => member.Name;
                    break;
                case "EMAIL":
                    sort = member => member.Email;
                    break;
                case "ID":
                    sort = member => member.Id;
                    break;
                case "CREATEDATE":
                    sort = member => member.CreateDate;
                    break;
                case "UPDATEDATE":
                    sort = member => member.UpdateDate;
                    break;
                case "ISAPPROVED":
                    sort = member => member.IsApproved;
                    break;
                case "ISLOCKEDOUT":
                    sort = member => member.IsLockedOut;
                    break;
                case "LASTLOGINDATE":
                    sort = member => member.LastLoginDate;
                    break;
                default:
                    throw new IndexOutOfRangeException("The orderBy parameter " + orderBy + " is not valid");
            }

            return _userRepository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, sort, orderDirection, includeUserGroups, excludeUserGroups, userState, filter);
        }
    }

    /// <summary>
    ///     Gets a list of paged <see cref="IUser" /> objects
    /// </summary>
    /// <param name="pageIndex">Current page index</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalRecords">Total number of records found (out)</param>
    /// <returns>
    ///     <see cref="IEnumerable{IMember}" />
    /// </returns>
    public IEnumerable<IUser> GetAll(long pageIndex, int pageSize, out long totalRecords)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userRepository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Name);
        }
    }

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <returns>
    ///     <see cref="IEnumerable{IUser}" />
    /// </returns>
    public IEnumerable<IUser> GetAllInGroup(int? groupId)
    {
        if (groupId is null)
        {
            return Array.Empty<IUser>();
        }

        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return backOfficeUserStore.GetAllInGroupAsync(groupId.Value).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects not associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <returns>
    ///     <see cref="IEnumerable{IUser}" />
    /// </returns>
    public IEnumerable<IUser> GetAllNotInGroup(int groupId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            return _userRepository.GetAllNotInGroup(groupId);
        }
    }

    #endregion

    #region Implementation of IUserService

    /// <summary>
    ///     Gets an IProfile by User Id.
    /// </summary>
    /// <param name="id">Id of the User to retrieve</param>
    /// <returns>
    ///     <see cref="IProfile" />
    /// </returns>
    public IProfile? GetProfileById(int id)
    {
        // This is called a TON. Go get the full user from cache which should already be IProfile
        IUser? fullUser = GetUserById(id);
        if (fullUser == null)
        {
            return null;
        }

        var asProfile = fullUser as IProfile;
        return asProfile ?? new UserProfile(fullUser.Id, fullUser.Name);
    }

    /// <summary>
    ///     Gets a profile by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>
    ///     <see cref="IProfile" />
    /// </returns>
    public IProfile? GetProfileByUserName(string username)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userRepository.GetProfile(username);
        }
    }

    /// <summary>
    ///     Gets a user by Id
    /// </summary>
    /// <param name="id">Id of the user to retrieve.</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    public IUser? GetUserById(int id)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return backOfficeUserStore.GetAsync(id).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Gets a user by it's key.
    /// </summary>
    /// <param name="key">Key of the user to retrieve.</param>
    /// <returns>Task resolving into an <see cref="IUser"/>.</returns>
    public Task<IUser?> GetAsync(Guid key)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return backOfficeUserStore.GetAsync(key);
    }

    public Task<IEnumerable<IUser>> GetAsync(IEnumerable<Guid> keys)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return backOfficeUserStore.GetUsersAsync(keys.ToArray());
    }

    public async Task<Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus>> GetLinkedLoginsAsync(Guid userKey)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        IUser? user = await backOfficeUserStore.GetAsync(userKey);
        if (user is null)
        {
            return Attempt.FailWithStatus<ICollection<IIdentityUserLogin>, UserOperationStatus>(UserOperationStatus.UserNotFound, Array.Empty<IIdentityUserLogin>());
        }

        ICoreBackOfficeUserManager manager = scope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();

        Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus> loginsAttempt = await manager.GetLoginsAsync(user);

        return loginsAttempt.Success is false
            ? Attempt.FailWithStatus<ICollection<IIdentityUserLogin>, UserOperationStatus>(loginsAttempt.Status, Array.Empty<IIdentityUserLogin>())
            : Attempt.SucceedWithStatus(UserOperationStatus.Success, loginsAttempt.Result);
    }

    public IEnumerable<IUser> GetUsersById(params int[]? ids)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserStore backOfficeUserStore = scope.ServiceProvider.GetRequiredService<IBackOfficeUserStore>();

        return backOfficeUserStore.GetUsersAsync(ids).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Replaces the same permission set for a single group to any number of entities
    /// </summary>
    /// <remarks>If no 'entityIds' are specified all permissions will be removed for the specified group.</remarks>
    /// <param name="groupId">Id of the group</param>
    /// <param name="permissions">
    ///     Permissions as enumerable list of <see cref="char" /> If nothing is specified all permissions
    ///     are removed.
    /// </param>
    /// <param name="entityIds">Specify the nodes to replace permissions for. </param>
    public void ReplaceUserGroupPermissions(int groupId, ISet<string> permissions, params int[] entityIds)
    {
        if (entityIds.Length == 0)
        {
            return;
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _userGroupRepository.ReplaceGroupPermissions(groupId, permissions, entityIds);
            scope.Complete();

            if (permissions is not null)
            {
                EntityPermission[] entityPermissions =
                    entityIds.Select(x => new EntityPermission(groupId, x, permissions)).ToArray();
                scope.Notifications.Publish(new AssignedUserGroupPermissionsNotification(entityPermissions, evtMsgs));
            }
        }
    }

    /// <summary>
    ///     Assigns the same permission set for a single user group to any number of entities
    /// </summary>
    /// <param name="groupId">Id of the user group</param>
    /// <param name="permission"></param>
    /// <param name="entityIds">Specify the nodes to replace permissions for</param>
    public void AssignUserGroupPermission(int groupId, string permission, params int[] entityIds)
    {
        if (entityIds.Length == 0)
        {
            return;
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _userGroupRepository.AssignGroupPermission(groupId, permission, entityIds);
            scope.Complete();

            var assigned = new HashSet<string>() { permission };
            EntityPermission[] entityPermissions =
                entityIds.Select(x => new EntityPermission(groupId, x, assigned)).ToArray();
            scope.Notifications.Publish(new AssignedUserGroupPermissionsNotification(entityPermissions, evtMsgs));
        }
    }

    public async Task<Attempt<UserOperationStatus>> VerifyPasswordResetAsync(Guid userKey, string token)
    {
        var decoded = token.FromUrlBase64();

        if (decoded is null)
        {
            return Attempt.Fail(UserOperationStatus.InvalidPasswordResetToken);
        }

        IUser? user = await GetAsync(userKey);

        if (user is null)
        {
            return Attempt.Fail(UserOperationStatus.UserNotFound);
        }

        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ICoreBackOfficeUserManager backOfficeUserManager = scope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();

        var isValid = await backOfficeUserManager.IsResetPasswordTokenValidAsync(user, decoded);

        return isValid
            ? Attempt.Succeed(UserOperationStatus.Success)
            : Attempt.Fail(UserOperationStatus.InvalidPasswordResetToken);
    }

    public async Task<Attempt<UserOperationStatus>> VerifyInviteAsync(Guid userKey, string token)
    {
        var decoded = token.FromUrlBase64();

        if (decoded is null)
        {
            return Attempt.Fail(UserOperationStatus.InvalidInviteToken);
        }

        IUser? user = await GetAsync(userKey);

        if (user is null)
        {
            return Attempt.Fail(UserOperationStatus.UserNotFound);
        }

        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ICoreBackOfficeUserManager backOfficeUserManager = scope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();

        var isValid = await backOfficeUserManager.IsEmailConfirmationTokenValidAsync(user, decoded);

        return isValid
            ? Attempt.Succeed(UserOperationStatus.Success)
            : Attempt.Fail(UserOperationStatus.InvalidInviteToken);
    }

    public async Task<Attempt<PasswordChangedModel, UserOperationStatus>> CreateInitialPasswordAsync(Guid userKey, string token, string password)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        Attempt<UserOperationStatus> verifyInviteAttempt = await VerifyInviteAsync(userKey, token);
        if (verifyInviteAttempt.Result != UserOperationStatus.Success)
        {
            return Attempt.FailWithStatus(verifyInviteAttempt.Result, new PasswordChangedModel());
        }

        Attempt<PasswordChangedModel, UserOperationStatus> changePasswordAttempt = await ChangePasswordAsync(userKey, new ChangeUserPasswordModel() { NewPassword = password, UserKey = userKey });

        Task<UserOperationStatus> enableAttempt = EnableAsync(userKey, new HashSet<Guid>() { userKey });

        if (enableAttempt.Result != UserOperationStatus.Success)
        {
            return Attempt.FailWithStatus(enableAttempt.Result, new PasswordChangedModel());
        }

        scope.Complete();
        return changePasswordAttempt;
    }

    public async Task<Attempt<PasswordChangedModel, UserOperationStatus>> ResetPasswordAsync(Guid userKey, string token, string password)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        Attempt<PasswordChangedModel, UserOperationStatus> changePasswordAttempt =
            await ChangePasswordAsync(userKey, new ChangeUserPasswordModel
            {
                NewPassword = password,
                UserKey = userKey,
                ResetPasswordToken = token
            });

        scope.Complete();
        return changePasswordAttempt;
    }

    public async Task<Attempt<PasswordChangedModel, UserOperationStatus>> ResetPasswordAsync(Guid performingUserKey, Guid userKey)
    {
        if (performingUserKey.Equals(userKey))
        {
            return Attempt.FailWithStatus(UserOperationStatus.SelfPasswordResetNotAllowed, new PasswordChangedModel());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

        ICoreBackOfficeUserManager backOfficeUserManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();

        var generatedPassword = backOfficeUserManager.GeneratePassword();

        Attempt<PasswordChangedModel, UserOperationStatus> changePasswordAttempt =
            await ChangePasswordAsync(performingUserKey, new ChangeUserPasswordModel
            {
                NewPassword = generatedPassword,
                UserKey = userKey,
            });

        scope.Complete();

        // todo tidy this up
        // this should be part of the result of the ChangePasswordAsync() method
        // but the model requires NewPassword
        // and the passwordChanger does not have a codePath that deals with generating
        if (changePasswordAttempt.Success)
        {
            changePasswordAttempt.Result.ResetPassword = generatedPassword;
        }

        return changePasswordAttempt;
    }


    /// <summary>
    ///     Removes a specific section from all users
    /// </summary>
    /// <remarks>This is useful when an entire section is removed from config</remarks>
    /// <param name="sectionAlias">Alias of the section to remove</param>
    public void DeleteSectionFromAllUserGroups(string sectionAlias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IEnumerable<IUserGroup> assignedGroups = _userGroupRepository.GetGroupsAssignedToSection(sectionAlias);
            foreach (IUserGroup group in assignedGroups)
            {
                // now remove the section for each user and commit
                // now remove the section for each user and commit
                group.RemoveAllowedSection(sectionAlias);
                _userGroupRepository.Save(group);
            }

            scope.Complete();
        }
    }

    /// <inheritdoc/>
    public async Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetMediaPermissionsAsync(Guid userKey, IEnumerable<Guid> mediaKeys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        Attempt<Dictionary<Guid, int>?> idAttempt = CreateIdKeyMap(mediaKeys, UmbracoObjectTypes.Media);

        if (idAttempt.Success is false || idAttempt.Result is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.MediaNodeNotFound, Enumerable.Empty<NodePermissions>());
        }

        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissions = await GetPermissionsAsync(userKey, idAttempt.Result);
        scope.Complete();

        return permissions;
    }

    /// <inheritdoc/>
    public async Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetDocumentPermissionsAsync(Guid userKey, IEnumerable<Guid> contentKeys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        Attempt<Dictionary<Guid, int>?> idAttempt = CreateIdKeyMap(contentKeys, UmbracoObjectTypes.Document);

        if (idAttempt.Success is false || idAttempt.Result is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.ContentNodeNotFound, Enumerable.Empty<NodePermissions>());
        }

        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissions = await GetPermissionsAsync(userKey, idAttempt.Result);
        scope.Complete();

        return permissions;
    }


    private async Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetPermissionsAsync(Guid userKey, Dictionary<Guid, int> nodes)
    {
        IUser? user = await GetAsync(userKey);

        if (user is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, Enumerable.Empty<NodePermissions>());
        }

        EntityPermissionCollection permissionsCollection = _userGroupRepository.GetPermissions(
            user.Groups.ToArray(),
            true,
            nodes.Select(x => x.Value).ToArray());

        var results = new List<NodePermissions>();
        foreach (KeyValuePair<Guid, int> node in nodes)
        {
            ISet<string> permissions = permissionsCollection.GetAllPermissions(node.Value);
            results.Add(new NodePermissions { NodeKey = node.Key, Permissions = permissions });
        }

        return Attempt.SucceedWithStatus<IEnumerable<NodePermissions>, UserOperationStatus>(UserOperationStatus.Success, results);
    }

    private Attempt<Dictionary<Guid, int>?> CreateIdKeyMap(IEnumerable<Guid> nodeKeys, UmbracoObjectTypes objectType)
    {
        // We'll return this as a dictionary we can link the id and key again later.
        Dictionary<Guid, int> idKeys = new();

        foreach (Guid key in nodeKeys)
        {
            Attempt<int> idAttempt = _entityService.GetId(key, objectType);
            if (idAttempt.Success is false)
            {
                return Attempt.Fail<Dictionary<Guid, int>?>(null);
            }

            idKeys[key] = idAttempt.Result;
        }

        return Attempt.Succeed<Dictionary<Guid, int>?>(idKeys);
    }

    /// <inheritdoc />
    public async Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetPermissionsAsync(Guid userKey, params Guid[] nodeKeys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? user = await GetAsync(userKey);

        if (user is null)
        {
            return Attempt.FailWithStatus(UserOperationStatus.UserNotFound, Enumerable.Empty<NodePermissions>());
        }

        Guid[] keys = nodeKeys.ToArray();
        if (keys.Length == 0)
        {
            return Attempt.SucceedWithStatus(UserOperationStatus.Success, Enumerable.Empty<NodePermissions>());
        }

        // We don't know what the entity type may be, so we have to get the entire entity :(
        Dictionary<int, Guid> idKeyMap = new();
        foreach (Guid key in keys)
        {
            IEntitySlim? entity = _entityService.Get(key);

            if (entity is null)
            {
                return Attempt.FailWithStatus(UserOperationStatus.NodeNotFound, Enumerable.Empty<NodePermissions>());
            }

            idKeyMap[entity.Id] = key;
        }

        EntityPermissionCollection permissionCollection = _userGroupRepository.GetPermissions(user.Groups.ToArray(), true, idKeyMap.Keys.ToArray());

        var results = new List<NodePermissions>();
        foreach (int nodeId in idKeyMap.Keys)
        {
            ISet<string> permissions = permissionCollection.GetAllPermissions(nodeId);
            results.Add(new NodePermissions { NodeKey = idKeyMap[nodeId], Permissions = permissions });
        }

        return Attempt.SucceedWithStatus<IEnumerable<NodePermissions>, UserOperationStatus>(UserOperationStatus.Success, results);
    }

    /// <summary>
    ///     Get explicitly assigned permissions for a user and optional node ids
    /// </summary>
    /// <param name="user">User to retrieve permissions for</param>
    /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
    /// <returns>An enumerable list of <see cref="EntityPermission" /></returns>
    public EntityPermissionCollection GetPermissions(IUser? user, params int[] nodeIds)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userGroupRepository.GetPermissions(user?.Groups.ToArray(), true, nodeIds);
        }
    }

    /// <summary>
    ///     Get explicitly assigned permissions for a group and optional node Ids
    /// </summary>
    /// <param name="groups"></param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
    /// <returns>An enumerable list of <see cref="EntityPermission" /></returns>
    public EntityPermissionCollection GetPermissions(IUserGroup?[] groups, bool fallbackToDefaultPermissions, params int[] nodeIds)
    {
        if (groups == null)
        {
            throw new ArgumentNullException(nameof(groups));
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userGroupRepository.GetPermissions(
                groups.WhereNotNull().Select(x => x.ToReadOnlyGroup()).ToArray(),
                fallbackToDefaultPermissions,
                nodeIds);
        }
    }

    /// <summary>
    ///     Get explicitly assigned permissions for a group and optional node Ids
    /// </summary>
    /// <param name="groups">Groups to retrieve permissions for</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
    /// <returns>An enumerable list of <see cref="EntityPermission" /></returns>
    private IEnumerable<EntityPermission> GetPermissions(IReadOnlyUserGroup[] groups, bool fallbackToDefaultPermissions, params int[] nodeIds)
    {
        if (groups == null)
        {
            throw new ArgumentNullException(nameof(groups));
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _userGroupRepository.GetPermissions(groups, fallbackToDefaultPermissions, nodeIds);
        }
    }

    /// <summary>
    ///     Gets the implicit/inherited permissions for the user for the given path
    /// </summary>
    /// <param name="user">User to check permissions for</param>
    /// <param name="path">Path to check permissions for</param>
    public EntityPermissionSet GetPermissionsForPath(IUser? user, string? path)
    {
        var nodeIds = path?.GetIdsFromPathReversed();

        if (nodeIds is null || nodeIds.Length == 0 || user is null)
        {
            return EntityPermissionSet.Empty();
        }

        // collect all permissions structures for all nodes for all groups belonging to the user
        EntityPermission[] groupPermissions = GetPermissionsForPath(user.Groups.ToArray(), nodeIds, true).ToArray();

        return CalculatePermissionsForPathForUser(groupPermissions, nodeIds);

    }

    /// <summary>
    ///     Gets the permissions for the provided group and path
    /// </summary>
    /// <param name="groups"></param>
    /// <param name="path">Path to check permissions for</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <returns>String indicating permissions for provided user and path</returns>
    public EntityPermissionSet GetPermissionsForPath(IUserGroup[] groups, string path, bool fallbackToDefaultPermissions = false)
    {
        var nodeIds = path.GetIdsFromPathReversed();

        if (nodeIds.Length == 0)
        {
            return EntityPermissionSet.Empty();
        }

        // collect all permissions structures for all nodes for all groups
        EntityPermission[] groupPermissions =
            GetPermissionsForPath(groups.Select(x => x.ToReadOnlyGroup()).ToArray(), nodeIds, true).ToArray();

        return CalculatePermissionsForPathForUser(groupPermissions, nodeIds);
    }

    public async Task<UserClientCredentialsOperationStatus> AddClientIdAsync(Guid userKey, string clientId)
    {
        if (ValidClientId().IsMatch(clientId) is false)
        {
            return UserClientCredentialsOperationStatus.InvalidClientId;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<string> currentClientIds = _userRepository.GetAllClientIds();
        if (currentClientIds.InvariantContains(clientId))
        {
            return UserClientCredentialsOperationStatus.DuplicateClientId;
        }

        IUser? user = await GetAsync(userKey);
        if (user is null || user.Kind != UserKind.Api)
        {
            return UserClientCredentialsOperationStatus.InvalidUser;
        }

        _userRepository.AddClientId(user.Id, clientId);

        return UserClientCredentialsOperationStatus.Success;
    }

    public async Task<bool> RemoveClientIdAsync(Guid userKey, string clientId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        return _userRepository.RemoveClientId(userId, clientId);
    }

    public Task<IUser?> FindByClientIdAsync(string clientId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IUser? user = _userRepository.GetByClientId(clientId);
        return Task.FromResult(user?.Kind == UserKind.Api ? user : null);
    }

    public async Task<IEnumerable<string>> GetClientIdsAsync(Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        var userId = await _userIdKeyResolver.GetAsync(userKey);
        return _userRepository.GetClientIds(userId);
    }

    /// <summary>
    ///     This performs the calculations for inherited nodes based on this
    ///     http://issues.umbraco.org/issue/U4-10075#comment=67-40085
    /// </summary>
    /// <param name="groupPermissions"></param>
    /// <param name="pathIds"></param>
    /// <returns></returns>
    internal static EntityPermissionSet CalculatePermissionsForPathForUser(
        EntityPermission[] groupPermissions,
        int[] pathIds)
    {
        // not sure this will ever happen, it shouldn't since this should return defaults, but maybe those are empty?
        if (groupPermissions.Length == 0 || pathIds.Length == 0)
        {
            return EntityPermissionSet.Empty();
        }

        // The actual entity id being looked at (deepest part of the path)
        var entityId = pathIds[0];

        var resultPermissions = new EntityPermissionCollection();

        // create a grouped by dictionary of another grouped by dictionary
        var permissionsByGroup = groupPermissions
            .GroupBy(x => x.UserGroupId)
            .ToDictionary(
                x => x.Key,
                x => x.GroupBy(a => a.EntityId).ToDictionary(a => a.Key, a => a.ToArray()));

        // iterate through each group
        foreach (KeyValuePair<int, Dictionary<int, EntityPermission[]>> byGroup in permissionsByGroup)
        {
            var added = false;

            // iterate deepest to shallowest
            foreach (var pathId in pathIds)
            {
                if (byGroup.Value.TryGetValue(pathId, out EntityPermission[]? permissionsForNodeAndGroup) == false)
                {
                    continue;
                }

                // In theory there will only be one EntityPermission in this group
                // but there's nothing stopping the logic of this method
                // from having more so we deal with it here
                foreach (EntityPermission entityPermission in permissionsForNodeAndGroup)
                {
                    if (entityPermission.IsDefaultPermissions == false)
                    {
                        // explicit permission found so we'll append it and move on, the collection is a hashset anyways
                        // so only supports adding one element per groupid/contentid
                        resultPermissions.Add(entityPermission);
                        added = true;
                        break;
                    }
                }

                // if the permission has been added for this group and this branch then we can exit this loop
                if (added)
                {
                    break;
                }
            }

            if (added == false && byGroup.Value.Count > 0)
            {
                // if there was no explicit permissions assigned in this branch for this group, then we will
                // add the group's default permissions
                resultPermissions.Add(byGroup.Value[entityId][0]);
            }
        }

        var permissionSet = new EntityPermissionSet(entityId, resultPermissions);
        return permissionSet;
    }

    private EntityPermissionCollection GetPermissionsForPath(IReadOnlyUserGroup[] groups, int[] pathIds, bool fallbackToDefaultPermissions = false)
    {
        if (pathIds.Length == 0)
        {
            return new EntityPermissionCollection(Enumerable.Empty<EntityPermission>());
        }

        // get permissions for all nodes in the path by group
        IEnumerable<IGrouping<int, EntityPermission>> permissions =
            GetPermissions(groups, fallbackToDefaultPermissions, pathIds)
                .GroupBy(x => x.UserGroupId);

        return new EntityPermissionCollection(
            permissions.Select(x => GetPermissionsForPathForGroup(x, pathIds, fallbackToDefaultPermissions))
                .Where(x => x is not null)!);
    }

    /// <summary>
    ///     Returns the resulting permission set for a group for the path based on all permissions provided for the branch
    /// </summary>
    /// <param name="pathPermissions">
    ///     The collective set of permissions provided to calculate the resulting permissions set for the path
    ///     based on a single group
    /// </param>
    /// <param name="pathIds">Must be ordered deepest to shallowest (right to left)</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <returns></returns>
    internal static EntityPermission? GetPermissionsForPathForGroup(
        IEnumerable<EntityPermission> pathPermissions,
        int[] pathIds,
        bool fallbackToDefaultPermissions = false)
    {
        // get permissions for all nodes in the path
        var permissionsByEntityId = pathPermissions.ToDictionary(x => x.EntityId, x => x);

        // then the permissions assigned to the path will be the 'deepest' node found that has permissions
        foreach (var id in pathIds)
        {
            if (permissionsByEntityId.TryGetValue(id, out EntityPermission? permission))
            {
                // don't return the default permissions if that is the one assigned here (we'll do that below if nothing was found)
                if (permission.IsDefaultPermissions == false)
                {
                    return permission;
                }
            }
        }

        // if we've made it here it means that no implicit/inherited permissions were found so we return the defaults if that is specified
        if (fallbackToDefaultPermissions == false)
        {
            return null;
        }

        return permissionsByEntityId[pathIds[0]];
    }

    private static void AddAdditionalPermissions(ISet<string> assignedPermissions, ISet<string> additionalPermissions)
    {
        foreach (var additionalPermission in additionalPermissions)
        {
            assignedPermissions.Add(additionalPermission);
        }
    }

    [GeneratedRegex(@"^[\w\d\-\._~]{1,100}$")]
    private static partial Regex ValidClientId();

    #endregion
}
