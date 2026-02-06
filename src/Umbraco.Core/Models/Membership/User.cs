using System.Runtime.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents a backoffice user
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class User : EntityBase, IUser, IProfile
{
    // Custom comparer for enumerable
    private static readonly DelegateEqualityComparer<IEnumerable<int>> IntegerEnumerableComparer =
        new(
            (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
            enum1 => enum1.GetHashCode());

    private IEnumerable<string>? _allowedSections;
    private string? _avatar;
    private string _email;
    private DateTime? _emailConfirmedDate;
    private int _failedLoginAttempts;
    private DateTime? _invitedDate;
    private bool _isApproved;
    private bool _isLockedOut;
    private string? _language;
    private DateTime? _lastLockoutDate;
    private DateTime? _lastLoginDate;
    private DateTime? _lastPasswordChangedDate;

    private string _name;
    private string? _passwordConfig;
    private string? _rawPasswordValue;
    private string? _securityStamp;
    private int _sessionTimeout;
    private int[]? _startContentIds;
    private int[]? _startMediaIds;
    private int[]? _startElementIds;
    private HashSet<IReadOnlyUserGroup> _userGroups;

    private string _username;
    private UserKind _kind;

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class for a new/empty user.
    /// </summary>
    /// <param name="globalSettings">The global settings.</param>
    public User(GlobalSettings globalSettings)
    {
        SessionTimeout = 60;
        _userGroups = new HashSet<IReadOnlyUserGroup>();
        _language = globalSettings.DefaultUILanguage;
        _isApproved = true;
        _isLockedOut = false;
        _startContentIds = [];
        _startMediaIds = [];
        _startElementIds = [];

        // cannot be null
        _rawPasswordValue = string.Empty;
        _username = string.Empty;
        _email = string.Empty;
        _name = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class for a new/empty user.
    /// </summary>
    /// <param name="globalSettings">The global settings for default values.</param>
    /// <param name="name">The display name of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="username">The username for the user.</param>
    /// <param name="rawPasswordValue">The raw password value for the user.</param>
    public User(GlobalSettings globalSettings, string? name, string email, string username, string rawPasswordValue)
        : this(globalSettings)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
        }

        if (string.IsNullOrEmpty(rawPasswordValue))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(rawPasswordValue));
        }

        _name = name;
        _email = email;
        _username = username;
        _rawPasswordValue = rawPasswordValue;
        _userGroups = new HashSet<IReadOnlyUserGroup>();
        _isApproved = true;
        _isLockedOut = false;
        _startContentIds = [];
        _startMediaIds = [];
        _startElementIds = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class for an existing user.
    /// </summary>
    /// <param name="globalSettings">The global settings for default values.</param>
    /// <param name="id">The unique identifier for the user.</param>
    /// <param name="name">The display name of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="username">The username for the user.</param>
    /// <param name="rawPasswordValue">The raw password value for the user.</param>
    /// <param name="passwordConfig">The password configuration for the user.</param>
    /// <param name="userGroups">The user groups the user belongs to.</param>
    /// <param name="startContentIds">The starting content node identifiers.</param>
    /// <param name="startMediaIds">The starting media node identifiers.</param>
    [Obsolete("Use the constructor that includes startElementIds. Scheduled for removal in Umbraco 19.")]
    public User(
        GlobalSettings globalSettings,
        int id,
        string? name,
        string email,
        string? username,
        string? rawPasswordValue,
        string? passwordConfig,
        IEnumerable<IReadOnlyUserGroup> userGroups,
        int[] startContentIds,
        int[] startMediaIds)
        : this(globalSettings, id, name, email, username, rawPasswordValue, passwordConfig, userGroups, startContentIds, startMediaIds, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class for an existing user.
    /// </summary>
    /// <param name="globalSettings">The global settings.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="email">The email.</param>
    /// <param name="username">The username.</param>
    /// <param name="rawPasswordValue">The raw password value.</param>
    /// <param name="passwordConfig">The password configuration.</param>
    /// <param name="userGroups">The user groups.</param>
    /// <param name="startContentIds">The start content identifiers.</param>
    /// <param name="startMediaIds">The start media identifiers.</param>
    /// <param name="startElementIds">The start element identifiers.</param>
    public User(
        GlobalSettings globalSettings,
        int id,
        string? name,
        string email,
        string? username,
        string? rawPasswordValue,
        string? passwordConfig,
        IEnumerable<IReadOnlyUserGroup> userGroups,
        int[] startContentIds,
        int[] startMediaIds,
        int[] startElementIds)
        : this(globalSettings)
    {
        // we allow whitespace for this value so just check null
        if (rawPasswordValue == null)
        {
            throw new ArgumentNullException(nameof(rawPasswordValue));
        }

        if (userGroups == null)
        {
            throw new ArgumentNullException(nameof(userGroups));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
        }

        Id = id;
        _name = name;
        _email = email;
        _username = username;
        _rawPasswordValue = rawPasswordValue;
        _passwordConfig = passwordConfig;
        _userGroups = new HashSet<IReadOnlyUserGroup>(userGroups);
        _isApproved = true;
        _isLockedOut = false;
        _startContentIds = startContentIds ?? throw new ArgumentNullException(nameof(startContentIds));
        _startMediaIds = startMediaIds ?? throw new ArgumentNullException(nameof(startMediaIds));
        _startElementIds = startElementIds ?? throw new ArgumentNullException(nameof(startElementIds));
    }

    /// <inheritdoc />
    [DataMember]
    public DateTime? EmailConfirmedDate
    {
        get => _emailConfirmedDate;
        set => SetPropertyValueAndDetectChanges(value, ref _emailConfirmedDate, nameof(EmailConfirmedDate));
    }

    /// <inheritdoc />
    [DataMember]
    public DateTime? InvitedDate
    {
        get => _invitedDate;
        set => SetPropertyValueAndDetectChanges(value, ref _invitedDate, nameof(InvitedDate));
    }

    /// <inheritdoc />
    [DataMember]
    public string Username
    {
        get => _username;
        set => SetPropertyValueAndDetectChanges(value, ref _username!, nameof(Username));
    }

    /// <inheritdoc />
    [DataMember]
    public string Email
    {
        get => _email;
        set => SetPropertyValueAndDetectChanges(value, ref _email!, nameof(Email));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public string? RawPasswordValue
    {
        get => _rawPasswordValue;
        set => SetPropertyValueAndDetectChanges(value, ref _rawPasswordValue, nameof(RawPasswordValue));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public string? PasswordConfiguration
    {
        get => _passwordConfig;
        set => SetPropertyValueAndDetectChanges(value, ref _passwordConfig, nameof(PasswordConfiguration));
    }

    /// <inheritdoc />
    [DataMember]
    public bool IsApproved
    {
        get => _isApproved;
        set => SetPropertyValueAndDetectChanges(value, ref _isApproved, nameof(IsApproved));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public bool IsLockedOut
    {
        get => _isLockedOut;
        set => SetPropertyValueAndDetectChanges(value, ref _isLockedOut, nameof(IsLockedOut));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public DateTime? LastLoginDate
    {
        get => _lastLoginDate;
        set => SetPropertyValueAndDetectChanges(value, ref _lastLoginDate, nameof(LastLoginDate));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public DateTime? LastPasswordChangeDate
    {
        get => _lastPasswordChangedDate;
        set => SetPropertyValueAndDetectChanges(value, ref _lastPasswordChangedDate, nameof(LastPasswordChangeDate));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public DateTime? LastLockoutDate
    {
        get => _lastLockoutDate;
        set => SetPropertyValueAndDetectChanges(value, ref _lastLockoutDate, nameof(LastLockoutDate));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public int FailedPasswordAttempts
    {
        get => _failedLoginAttempts;
        set => SetPropertyValueAndDetectChanges(value, ref _failedLoginAttempts, nameof(FailedPasswordAttempts));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public string? Comments { get; set; }

    /// <inheritdoc />
    public UserState UserState
    {
        get
        {
            if (LastLoginDate == default && IsApproved == false && InvitedDate != null)
            {
                return UserState.Invited;
            }

            if (IsLockedOut)
            {
                return UserState.LockedOut;
            }

            if (IsApproved == false)
            {
                return UserState.Disabled;
            }

            // User is not disabled or locked and has never logged in before
            if (LastLoginDate == default && IsApproved && IsLockedOut == false)
            {
                return UserState.Inactive;
            }

            return UserState.Active;
        }
    }

    /// <inheritdoc />
    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <inheritdoc />
    public IEnumerable<string> AllowedSections => _allowedSections ??= new List<string>(_userGroups
                                                      .SelectMany(x => x.AllowedSections).Distinct());

    /// <inheritdoc />
    public IProfile ProfileData => new WrappedUserProfile(this);

    /// <summary>
    ///     The security stamp used by ASP.Net identity
    /// </summary>
    [IgnoreDataMember]
    public string? SecurityStamp
    {
        get => _securityStamp;
        set => SetPropertyValueAndDetectChanges(value, ref _securityStamp, nameof(SecurityStamp));
    }

    /// <inheritdoc />
    [DataMember]
    public string? Avatar
    {
        get => _avatar;
        set => SetPropertyValueAndDetectChanges(value, ref _avatar, nameof(Avatar));
    }

    /// <summary>
    ///     Gets or sets the session timeout.
    /// </summary>
    /// <value>
    ///     The session timeout.
    /// </value>
    [DataMember]
    public int SessionTimeout
    {
        get => _sessionTimeout;
        set => SetPropertyValueAndDetectChanges(value, ref _sessionTimeout, nameof(SessionTimeout));
    }

    /// <summary>
    ///     Gets or sets the start content id.
    /// </summary>
    /// <value>
    ///     The start content id.
    /// </value>
    [DataMember]
    [DoNotClone]
    public int[]? StartContentIds
    {
        get => _startContentIds;
        set => SetPropertyValueAndDetectChanges(value, ref _startContentIds, nameof(StartContentIds), IntegerEnumerableComparer);
    }

    /// <summary>
    ///     Gets or sets the start media id.
    /// </summary>
    /// <value>
    ///     The start media id.
    /// </value>
    [DataMember]
    [DoNotClone]
    public int[]? StartMediaIds
    {
        get => _startMediaIds;
        set => SetPropertyValueAndDetectChanges(value, ref _startMediaIds, nameof(StartMediaIds), IntegerEnumerableComparer);
    }

    /// <summary>
    ///     Gets or sets the start element ids.
    /// </summary>
    /// <value>
    ///     The start element ids.
    /// </value>
    [DataMember]
    [DoNotClone]
    public int[]? StartElementIds
    {
        get => _startElementIds;
        set => SetPropertyValueAndDetectChanges(value, ref _startElementIds, nameof(StartElementIds), IntegerEnumerableComparer);
    }

    /// <inheritdoc />
    [DataMember]
    public string? Language
    {
        get => _language;
        set => SetPropertyValueAndDetectChanges(value, ref _language, nameof(Language));
    }

    /// <inheritdoc />
    [DataMember]
    public UserKind Kind
    {
        get => _kind;
        set => SetPropertyValueAndDetectChanges(value, ref _kind, nameof(Kind));
    }

    /// <summary>
    ///     Gets the groups that user is part of
    /// </summary>
    [DataMember]
    public IEnumerable<IReadOnlyUserGroup> Groups => _userGroups;

    /// <inheritdoc />
    public void RemoveGroup(string group)
    {
        foreach (IReadOnlyUserGroup userGroup in _userGroups.ToArray())
        {
            if (userGroup.Alias == group)
            {
                _userGroups.Remove(userGroup);

                // reset this flag so it's rebuilt with the assigned groups
                _allowedSections = null;
                OnPropertyChanged(nameof(Groups));
            }
        }
    }

    /// <inheritdoc />
    public void ClearGroups()
    {
        if (_userGroups.Count > 0)
        {
            _userGroups.Clear();

            // reset this flag so it's rebuilt with the assigned groups
            _allowedSections = null;
            OnPropertyChanged(nameof(Groups));
        }
    }

    /// <inheritdoc />
    public void AddGroup(IReadOnlyUserGroup group)
    {
        if (_userGroups.Add(group))
        {
            // reset this flag so it's rebuilt with the assigned groups
            _allowedSections = null;
            OnPropertyChanged(nameof(Groups));
        }
    }

    /// <inheritdoc />
    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (User)clone;

        // manually clone the start node props
        clonedEntity._startContentIds = _startContentIds?.ToArray();
        clonedEntity._startMediaIds = _startMediaIds?.ToArray();
        clonedEntity._startElementIds = _startElementIds?.ToArray();

        // need to create new collections otherwise they'll get copied by ref
        clonedEntity._userGroups = new HashSet<IReadOnlyUserGroup>(_userGroups);
        clonedEntity._allowedSections = _allowedSections != null ? new List<string>(_allowedSections) : null;
    }

    /// <summary>
    ///     Internal class used to wrap the user in a profile
    /// </summary>
    private sealed class WrappedUserProfile : IProfile
    {
        private readonly IUser _user;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WrappedUserProfile" /> class.
        /// </summary>
        /// <param name="user">The user to wrap.</param>
        public WrappedUserProfile(IUser user) => _user = user;

        /// <inheritdoc />
        public int Id => _user.Id;

        /// <inheritdoc />
        public string? Name => _user.Name;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((WrappedUserProfile)obj);
        }

        private bool Equals(WrappedUserProfile other) => _user.Equals(other._user);

        /// <inheritdoc />
        public override int GetHashCode() => _user.GetHashCode();
    }
}
