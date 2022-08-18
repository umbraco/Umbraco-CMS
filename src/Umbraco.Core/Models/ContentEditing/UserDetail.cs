using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents information for the current user
/// </summary>
[DataContract(Name = "user", Namespace = "")]
public class UserDetail : UserProfile
{
    [DataMember(Name = "email", IsRequired = true)]
    [Required]
    public string? Email { get; set; }

    [DataMember(Name = "locale", IsRequired = true)]
    [Required]
    public string? Culture { get; set; }

    /// <summary>
    ///     The MD5 lowercase hash of the email which can be used by gravatar
    /// </summary>
    [DataMember(Name = "emailHash")]
    public string? EmailHash { get; set; }

    [ReadOnly(true)]
    [DataMember(Name = "userGroups")]
    public string?[]? UserGroups { get; set; }

    /// <summary>
    ///     Gets/sets the number of seconds for the user's auth ticket to expire
    /// </summary>
    [DataMember(Name = "remainingAuthSeconds")]
    public double SecondsUntilTimeout { get; set; }

    /// <summary>
    ///     The user's calculated start nodes based on the start nodes they have assigned directly to them and via the groups
    ///     they're assigned to
    /// </summary>
    [DataMember(Name = "startContentIds")]
    public int[]? StartContentIds { get; set; }

    /// <summary>
    ///     The user's calculated start nodes based on the start nodes they have assigned directly to them and via the groups
    ///     they're assigned to
    /// </summary>
    [DataMember(Name = "startMediaIds")]
    public int[]? StartMediaIds { get; set; }

    /// <summary>
    ///     Returns a list of different size avatars
    /// </summary>
    [DataMember(Name = "avatars")]
    public string[]? Avatars { get; set; }

    /// <summary>
    ///     A list of sections the user is allowed to view.
    /// </summary>
    [DataMember(Name = "allowedSections")]
    public IEnumerable<string>? AllowedSections { get; set; }

    /// <summary>
    ///     A list of language culcure codes the user is allowed to view.
    /// </summary>
    [DataMember(Name = "allowedLanguageIds")]
    public IEnumerable<int>? AllowedLanguageIds { get; set; }
}
