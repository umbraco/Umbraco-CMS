using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using SecurityConstants = Umbraco.Cms.Core.Constants.Security;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Options used to configure auto-linking external OAuth providers
/// </summary>
public class MemberExternalSignInAutoLinkOptions
{
    private readonly string? _defaultCulture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberExternalSignInAutoLinkOptions" /> class.
    /// </summary>
    public MemberExternalSignInAutoLinkOptions(
        bool autoLinkExternalAccount = false,
        bool defaultIsApproved = true,
        string defaultMemberTypeAlias = Core.Constants.Conventions.MemberTypes.DefaultAlias,
        string? defaultCulture = null,
        IEnumerable<string>? defaultMemberGroups = null)
    {
        AutoLinkExternalAccount = autoLinkExternalAccount;
        DefaultIsApproved = defaultIsApproved;
        DefaultMemberTypeAlias = defaultMemberTypeAlias;
        _defaultCulture = defaultCulture;
        DefaultMemberGroups = defaultMemberGroups ?? Array.Empty<string>();
    }

    /// <summary>
    ///     A callback executed during account auto-linking and before the user is persisted
    /// </summary>
    [IgnoreDataMember]
    public Action<MemberIdentityUser, ExternalLoginInfo>? OnAutoLinking { get; set; }

    /// <summary>
    ///     A callback executed during every time a user authenticates using an external login.
    ///     returns a boolean indicating if sign in should continue or not.
    /// </summary>
    [IgnoreDataMember]
    public Func<MemberIdentityUser, ExternalLoginInfo, bool>? OnExternalLogin { get; set; }

    /// <summary>
    ///     Gets a value indicating whether flag indicating if logging in with the external provider should auto-link/create a
    ///     local user
    /// </summary>
    public bool AutoLinkExternalAccount { get; }

    /// <summary>
    ///     Gets the member type alias that auto linked members are created as
    /// </summary>
    public string DefaultMemberTypeAlias { get; }

    /// <summary>
    ///     Gets the IsApproved value for auto linked members.
    /// </summary>
    public bool DefaultIsApproved { get; }

    /// <summary>
    ///     Gets the default member groups to add the user in.
    /// </summary>
    public IEnumerable<string> DefaultMemberGroups { get; }

    /// <summary>
    ///     The default Culture to use for auto-linking users
    /// </summary>
    // TODO: Should we use IDefaultCultureAccessor here instead?
    public string GetUserAutoLinkCulture(GlobalSettings globalSettings) =>
        _defaultCulture ?? globalSettings.DefaultUILanguage;
}
