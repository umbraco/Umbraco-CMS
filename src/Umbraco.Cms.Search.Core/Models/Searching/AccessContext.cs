using System.Text.Json.Serialization;

namespace Umbraco.Cms.Search.Core.Models.Searching;

/// <summary>
/// Identifies the member (and optionally their groups) a search is being performed on behalf of, so protected content they have access to is included in results.
/// </summary>
/// <param name="PrincipalId">The key of the member performing the search.</param>
/// <param name="GroupIds">The keys of the member groups the member belongs to, or null if none apply.</param>
public record AccessContext(Guid PrincipalId, Guid[]? GroupIds)
{
    /// <summary>
    /// Gets a value indicating whether protected-content access restrictions should be bypassed entirely, including all protected content regardless of member.
    /// </summary>
    [JsonIgnore]
    public bool Bypass { get; private init; }

    /// <summary>
    /// Creates an <see cref="AccessContext"/> that bypasses protected-content access restrictions.
    /// </summary>
    /// <returns>The created <see cref="AccessContext"/>.</returns>
    public static AccessContext BypassProtection() => new(Guid.Empty, null) { Bypass = true };
}
