using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A password hasher for back office users
/// </summary>
/// <remarks>
///     This allows us to verify passwords in old formats and roll forward to the latest format
/// </remarks>
public class BackOfficePasswordHasher : UmbracoPasswordHasher<BackOfficeIdentityUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficePasswordHasher"/> class.
    /// </summary>
    /// <param name="passwordSecurity">An instance of <see cref="LegacyPasswordSecurity"/> used for legacy password operations.</param>
    /// <param name="jsonSerializer">An instance of <see cref="IJsonSerializer"/> used for serializing and deserializing password data.</param>
    public BackOfficePasswordHasher(LegacyPasswordSecurity passwordSecurity, IJsonSerializer jsonSerializer)
        : base(passwordSecurity, jsonSerializer)
    {
    }
}
