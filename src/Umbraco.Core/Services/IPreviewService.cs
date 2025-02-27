using System.Security.Claims;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services;

public interface IPreviewService
{
    /// <summary>
    /// Enters preview mode for a given user that calls this
    /// </summary>
    Task<bool> TryEnterPreviewAsync(IUser user);

    /// <summary>
    /// Exits preview mode for a given user that calls this
    /// </summary>
    Task EndPreviewAsync();

    bool IsInPreview();

    Task<Attempt<ClaimsIdentity>> TryGetPreviewClaimsIdentityAsync();
}
