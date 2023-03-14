using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Member object
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Member : ContentBase, IMember
{
    private IDictionary<string, object?>? _additionalData;
    private string _email;
    private DateTime? _emailConfirmedDate;
    private int _failedPasswordAttempts;
    private bool _isApproved;
    private bool _isLockedOut;
    private DateTime? _lastLockoutDate;
    private DateTime? _lastLoginDate;
    private DateTime? _lastPasswordChangeDate;
    private string? _passwordConfig;
    private string? _rawPasswordValue;
    private string? _securityStamp;
    private string _username;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Member" /> class.
    ///     Constructor for creating an empty Member object
    /// </summary>
    /// <param name="contentType">ContentType for the current Content object</param>
    public Member(IMemberType contentType)
        : base(string.Empty, -1, contentType, new PropertyCollection())
    {
        IsApproved = true;

        // this cannot be null but can be empty
        _rawPasswordValue = string.Empty;
        _email = string.Empty;
        _username = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Member" /> class.
    ///     Constructor for creating a Member object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    public Member(string name, IMemberType contentType)
        : base(name, -1, contentType, new PropertyCollection())
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        IsApproved = true;

        // this cannot be null but can be empty
        _rawPasswordValue = string.Empty;
        _email = string.Empty;
        _username = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Member" /> class.
    ///     Constructor for creating a Member object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <param name="contentType"></param>
    /// <param name="isApproved"></param>
    public Member(string name, string email, string username, IMemberType contentType, bool isApproved = true)
        : base(name, -1, contentType, new PropertyCollection())
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (email == null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(email));
        }

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

        _email = email;
        _username = username;
        IsApproved = isApproved;

        // this cannot be null but can be empty
        _rawPasswordValue = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Member" /> class.
    ///     Constructor for creating a Member object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <param name="contentType"></param>
    /// <param name="userId"></param>
    /// <param name="isApproved"></param>
    public Member(string name, string email, string username, IMemberType contentType, int userId, bool isApproved = true)
        : base(name, -1, contentType, new PropertyCollection())
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (email == null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(email));
        }

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

        _email = email;
        _username = username;
        CreatorId = userId;
        IsApproved = isApproved;

        // this cannot be null but can be empty
        _rawPasswordValue = string.Empty;
    }

    /// <summary>
    ///     Constructor for creating a Member object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <param name="rawPasswordValue">
    ///     The password value passed in to this parameter should be the encoded/encrypted/hashed format of the member's
    ///     password
    /// </param>
    /// <param name="contentType"></param>
    public Member(string? name, string email, string username, string? rawPasswordValue, IMemberType? contentType)
        : base(name, -1, contentType, new PropertyCollection())
    {
        _email = email;
        _username = username;
        _rawPasswordValue = rawPasswordValue;
        IsApproved = true;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Member" /> class.
    ///     Constructor for creating a Member object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <param name="rawPasswordValue">
    ///     The password value passed in to this parameter should be the encoded/encrypted/hashed format of the member's
    ///     password
    /// </param>
    /// <param name="contentType"></param>
    /// <param name="isApproved"></param>
    public Member(string name, string email, string username, string rawPasswordValue, IMemberType contentType, bool isApproved)
        : base(name, -1, contentType, new PropertyCollection())
    {
        _email = email;
        _username = username;
        _rawPasswordValue = rawPasswordValue;
        IsApproved = isApproved;
    }

    /// <summary>
    ///     Constructor for creating a Member object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="username"></param>
    /// <param name="rawPasswordValue">
    ///     The password value passed in to this parameter should be the encoded/encrypted/hashed format of the member's
    ///     password
    /// </param>
    /// <param name="contentType"></param>
    /// <param name="isApproved"></param>
    /// <param name="userId"></param>
    public Member(string name, string email, string username, string rawPasswordValue, IMemberType contentType, bool isApproved, int userId)
        : base(name, -1, contentType, new PropertyCollection())
    {
        _email = email;
        _username = username;
        _rawPasswordValue = rawPasswordValue;
        IsApproved = isApproved;
        CreatorId = userId;
    }

    /// <summary>
    ///     Gets or sets the Groups that Member is part of
    /// </summary>
    [DataMember]
    public IEnumerable<string>? Groups { get; set; }

    /// <summary>
    ///     Gets or sets the Username
    /// </summary>
    [DataMember]
    public string Username
    {
        get => _username;
        set => SetPropertyValueAndDetectChanges(value, ref _username!, nameof(Username));
    }

    /// <summary>
    ///     Gets or sets the Email
    /// </summary>
    [DataMember]
    public string Email
    {
        get => _email;
        set => SetPropertyValueAndDetectChanges(value, ref _email!, nameof(Email));
    }

    [DataMember]
    public DateTime? EmailConfirmedDate
    {
        get => _emailConfirmedDate;
        set => SetPropertyValueAndDetectChanges(value, ref _emailConfirmedDate, nameof(EmailConfirmedDate));
    }

    /// <summary>
    ///     Gets or sets the raw password value
    /// </summary>
    [IgnoreDataMember]
    public string? RawPasswordValue
    {
        get => _rawPasswordValue;
        set
        {
            if (value == null)
            {
                // special case, this is used to ensure that the password is not updated when persisting, in this case
                // we don't want to track changes either
                _rawPasswordValue = null;
            }
            else
            {
                SetPropertyValueAndDetectChanges(value, ref _rawPasswordValue, nameof(RawPasswordValue));
            }
        }
    }

    [IgnoreDataMember]
    public string? PasswordConfiguration
    {
        get => _passwordConfig;
        set => SetPropertyValueAndDetectChanges(value, ref _passwordConfig, nameof(PasswordConfiguration));
    }

    // TODO: When get/setting all of these properties we MUST:
    // * Check if we are using the umbraco membership provider, if so then we need to use the configured fields - not the explicit fields below
    // * If any of the fields don't exist, what should we do? Currently it will throw an exception!

    /// <summary>
    ///     Gets or set the comments for the member
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberComments
    ///     Part of the standard properties collection.
    /// </remarks>
    [DataMember]
    public string? Comments
    {
        get
        {
            Attempt<string?> a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.Comments, nameof(Comments), default(string));
            if (a.Success == false)
            {
                return a.Result;
            }

            return Properties[Constants.Conventions.Member.Comments]?.GetValue() == null
                ? string.Empty
                : Properties[Constants.Conventions.Member.Comments]?.GetValue()?.ToString();
        }

        set
        {
            if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.Comments,
                    nameof(Comments)) == false)
            {
                return;
            }

            Properties[Constants.Conventions.Member.Comments]?.SetValue(value);
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the Member is approved
    /// </summary>
    [DataMember]
    public bool IsApproved
    {
        get => _isApproved;
        set => SetPropertyValueAndDetectChanges(value, ref _isApproved, nameof(IsApproved));
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the Member is locked out
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberLockedOut
    ///     Part of the standard properties collection.
    /// </remarks>
    [DataMember]
    public bool IsLockedOut
    {
        get => _isLockedOut;
        set => SetPropertyValueAndDetectChanges(value, ref _isLockedOut, nameof(IsLockedOut));
    }

    /// <summary>
    ///     Gets or sets the date for last login
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberLastLogin
    ///     Part of the standard properties collection.
    /// </remarks>
    [DataMember]
    public DateTime? LastLoginDate
    {
        get => _lastLoginDate;
        set => SetPropertyValueAndDetectChanges(value, ref _lastLoginDate, nameof(LastLoginDate));
    }

    /// <summary>
    ///     Gest or sets the date for last password change
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberLastPasswordChangeDate
    ///     Part of the standard properties collection.
    /// </remarks>
    [DataMember]
    public DateTime? LastPasswordChangeDate
    {
        get => _lastPasswordChangeDate;
        set => SetPropertyValueAndDetectChanges(value, ref _lastPasswordChangeDate, nameof(LastPasswordChangeDate));
    }

    /// <summary>
    ///     Gets or sets the date for when Member was locked out
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberLastLockoutDate
    ///     Part of the standard properties collection.
    /// </remarks>
    [DataMember]
    public DateTime? LastLockoutDate
    {
        get => _lastLockoutDate;
        set => SetPropertyValueAndDetectChanges(value, ref _lastLockoutDate, nameof(LastLockoutDate));
    }

    /// <summary>
    ///     Gets or sets the number of failed password attempts.
    ///     This is the number of times the password was entered incorrectly upon login.
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberFailedPasswordAttempts
    ///     Part of the standard properties collection.
    /// </remarks>
    [DataMember]
    public int FailedPasswordAttempts
    {
        get => _failedPasswordAttempts;
        set => SetPropertyValueAndDetectChanges(value, ref _failedPasswordAttempts, nameof(FailedPasswordAttempts));
    }

    /// <summary>
    ///     String alias of the default ContentType
    /// </summary>
    [DataMember]
    public virtual string ContentTypeAlias => ContentType.Alias;

    /// <summary>
    ///     The security stamp used by ASP.Net identity
    /// </summary>
    [IgnoreDataMember]
    public string? SecurityStamp
    {
        get => _securityStamp;
        set => SetPropertyValueAndDetectChanges(value, ref _securityStamp, nameof(SecurityStamp));
    }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [IgnoreDataMember]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? LongStringPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [IgnoreDataMember]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? ShortStringPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [IgnoreDataMember]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public int IntegerPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [IgnoreDataMember]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool BoolPropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [IgnoreDataMember]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DateTime DateTimePropertyValue { get; set; }

    /// <summary>
    ///     Internal/Experimental - only used for mapping queries.
    /// </summary>
    /// <remarks>
    ///     Adding these to have first level properties instead of the Properties collection.
    /// </remarks>
    [IgnoreDataMember]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? PropertyTypeAlias { get; set; }

    /// <inheritdoc />
    [DataMember]
    [DoNotClone]
    public IDictionary<string, object?> AdditionalData => _additionalData ??= new Dictionary<string, object?>();

    /// <inheritdoc />
    [IgnoreDataMember]
    public bool HasAdditionalData => _additionalData != null;

    private Attempt<T> WarnIfPropertyTypeNotFoundOnGet<T>(string propertyAlias, string propertyName, T defaultVal)
    {
        static void DoLog(string logPropertyAlias, string logPropertyName)
        {
            StaticApplicationLogging.Logger.LogWarning(
                "Trying to access the '{PropertyName}' property on '{MemberType}' " +
                "but the {PropertyAlias} property does not exist on the member type so a default value is returned. " +
                "Ensure that you have a property type with alias:  {PropertyAlias} configured on your member type in order to use the '{PropertyName}' property on the model correctly.",
                logPropertyName,
                typeof(Member),
                logPropertyAlias);
        }

        // if the property doesn't exist,
        if (Properties.Contains(propertyAlias) == false)
        {
            // put a warn in the log if this entity has been persisted
            // then return a failure
            if (HasIdentity)
            {
                DoLog(propertyAlias, propertyName);
            }

            return Attempt<T>.Fail(defaultVal);
        }

        return Attempt<T>.Succeed();
    }

    private bool WarnIfPropertyTypeNotFoundOnSet(string propertyAlias, string propertyName)
    {
        static void DoLog(string logPropertyAlias, string logPropertyName)
        {
            StaticApplicationLogging.Logger.LogWarning(
                "An attempt was made to set a value on the property '{PropertyName}' on type '{MemberType}' but the " +
                "property type {PropertyAlias} does not exist on the member type, ensure that this property type exists so that setting this property works correctly.",
                logPropertyName,
                typeof(Member),
                logPropertyAlias);
        }

        // if the property doesn't exist,
        if (Properties.Contains(propertyAlias) == false)
        {
            // put a warn in the log if this entity has been persisted
            // then return a failure
            if (HasIdentity)
            {
                DoLog(propertyAlias, propertyName);
            }

            return false;
        }

        return true;
    }
}
