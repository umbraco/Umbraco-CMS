using System.Text.Json.Serialization;

namespace Umbraco.Cms.Search.Core.Models.Searching;

public record AccessContext(Guid PrincipalId, Guid[]? GroupIds)
{
    [JsonIgnore]
    public bool Bypass { get; private init; }

    public static AccessContext BypassProtection() => new(Guid.Empty, null) { Bypass = true };
}
