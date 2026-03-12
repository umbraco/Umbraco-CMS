using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The identity user used for the back office
/// </summary>
public class BackOfficeIdentityUser : UmbracoIdentityUser
{
    private static readonly DelegateEqualityComparer<int[]> _startIdsComparer = new(
        (groups, enumerable) => groups.UnsortedSequenceEqual(enumerable),
        groups => groups.GetHashCode());

    private string[]? _allowedSections;
    private string _culture;
    private IReadOnlyCollection<IReadOnlyUserGroup> _groups = null!;
    private DateTime? _inviteDate;
    private int[] _startContentIds;
    private int[] _startMediaIds;
    private UserKind _kind;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeIdentityUser" /> class.
    /// </summary>
    public BackOfficeIdentityUser(GlobalSettings globalSettings, int userId, IEnumerable<IReadOnlyUserGroup> groups)
        : this(globalSettings, groups.ToArray()) =>

        // use the property setters - they do more than just setting a field
        Id = UserIdToString(userId);

    private BackOfficeIdentityUser(GlobalSettings globalSettings, IReadOnlyCollection<IReadOnlyUserGroup> groups)
    {
        _startMediaIds = Array.Empty<int>();
        _startContentIds = Array.Empty<int>();
        _allowedSections = Array.Empty<string>();
        _culture = globalSettings.DefaultUILanguage;

        SetGroups(groups);
    }

    /// <summary>
    /// Gets or sets the calculated media start node IDs for the back office identity user.
    /// These IDs represent the root media nodes that the user has access to, typically determined by user group permissions or configuration.
    /// </summary>
    public int[]? CalculatedMediaStartNodeIds { get; set; }

    /// <summary>
    /// Gets or sets the set of content start node IDs that have been calculated for this back office identity user, typically based on their group memberships and permissions.
    /// These IDs determine the root content nodes the user can access in the back office.
    /// </summary>
    public int[]? CalculatedContentStartNodeIds { get; set; }

    /// <summary>
    ///     Gets or sets invite date
    /// </summary>
    public DateTime? InviteDate
    {
        get => _inviteDate;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _inviteDate, nameof(InviteDate));
    }

    /// <summary>
    ///     Gets or sets content start nodes assigned to the User (not ones assigned to the user's groups)
    /// </summary>
    public int[] StartContentIds
    {
        get => _startContentIds;
        set
        {
            value ??= [];

            BeingDirty.SetPropertyValueAndDetectChanges(value, ref _startContentIds!, nameof(StartContentIds), _startIdsComparer);
        }
    }

    /// <summary>
    ///     Gets or sets media start nodes assigned to the User (not ones assigned to the user's groups)
    /// </summary>
    public int[] StartMediaIds
    {
        get => _startMediaIds;
        set
        {
            value ??= Array.Empty<int>();

            BeingDirty.SetPropertyValueAndDetectChanges(value, ref _startMediaIds!, nameof(StartMediaIds), _startIdsComparer);
        }
    }

    /// <summary>
    ///     Gets a readonly list of the user's allowed sections which are based on it's user groups
    /// </summary>
    public string[] AllowedSections =>
        _allowedSections ??= _groups.SelectMany(x => x.AllowedSections).Distinct().ToArray();

    /// <summary>
    ///     Gets or sets the culture
    /// </summary>
    public string Culture
    {
        get => _culture;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _culture!, nameof(Culture));
    }

    private Guid _key;

    /// <summary>
    /// Gets or sets the unique key that identifies the back office identity user.
    /// </summary>
    public Guid Key
    {
        get => _key;
        set
        {
            _key = value;
            HasIdentity = true;
        }
    }

    /// <summary>
    /// Gets or sets the classification of the back office user, indicating the user's type or role within the system.
    /// </summary>
    public UserKind Kind
    {
        get => _kind;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _kind, nameof(Kind));
    }

    /// <summary>
    ///     Creates a new <see cref="BackOfficeIdentityUser"/> instance without an identity, for use before the user is persisted.
    /// </summary>
    /// <param name="globalSettings">The global settings used to initialize the user instance.</param>
    /// <param name="username">The username for the new user. Cannot be null or whitespace.</param>
    /// <param name="email">The email address for the user. This can be null, but must be set before persisting the user.</param>
    /// <param name="culture">The culture assigned to the user. Cannot be null or whitespace.</param>
    /// <param name="name">The display name of the user. Optional.</param>
    /// <param name="id">The unique identifier for the user. Optional; if not provided, a new one will be generated.</param>
    /// <param name="kind">The kind of user. Optional; defaults to <see cref="UserKind.Default"/>.</param>
    /// <returns>A new instance of <see cref="BackOfficeIdentityUser"/> without an identity.</returns>
    public static BackOfficeIdentityUser CreateNew(GlobalSettings globalSettings, string? username, string email, string culture, string? name = null, Guid? id = null, UserKind kind = UserKind.Default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));
        }

        var user = new BackOfficeIdentityUser(globalSettings, Array.Empty<IReadOnlyUserGroup>());
        user.DisableChangeTracking();
        user.UserName = username;
        user.Email = email;
        user.Id = string.Empty;

        if (id is not null)
        {
            user.Key = id.Value;
        }

        user.HasIdentity = false;
        user._culture = culture;
        user.Name = name;
        user.Kind = kind;
        user.EnableChangeTracking();
        return user;
    }

    /// <summary>
    ///     Sets the user groups for the backoffice identity user and updates related roles.
    /// </summary>
    /// <param name="value">The collection of user groups to assign to the user.</param>
    public void SetGroups(IReadOnlyCollection<IReadOnlyUserGroup> value)
    {
        // so they recalculate
        _allowedSections = null;

        _groups = value.ToArray();

        var roles = new List<IdentityUserRole<string>>();
        foreach (IdentityUserRole<string> identityUserRole in _groups.Select(x => new IdentityUserRole<string>
        {
            RoleId = x.Alias,
            UserId = Id,
        }))
        {
            roles.Add(identityUserRole);
        }

        // now reset the collection
        Roles = roles;
    }

    private static string UserIdToString(int userId) => string.Intern(userId.ToString(CultureInfo.InvariantCulture));

    private static int UserIdToInt(string? userId)
    {
        if (int.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        if (Guid.TryParse(userId, out Guid key))
        {
            // Reverse the IntExtensions.ToGuid
            return BitConverter.ToInt32(key.ToByteArray(), 0);
        }

        throw new InvalidOperationException($"Unable to convert user ID ({userId})to int using InvariantCulture");
    }
}
