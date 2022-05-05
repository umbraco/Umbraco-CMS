using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

public class UmbracoPasswordHasher<TUser> : PasswordHasher<TUser>
    where TUser : UmbracoIdentityUser
{
    private readonly IJsonSerializer _jsonSerializer;

    public UmbracoPasswordHasher(LegacyPasswordSecurity legacyPasswordSecurity, IJsonSerializer jsonSerializer)
    {
        LegacyPasswordSecurity =
            legacyPasswordSecurity ?? throw new ArgumentNullException(nameof(legacyPasswordSecurity));
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
    }

    public LegacyPasswordSecurity LegacyPasswordSecurity { get; }

    public override string HashPassword(TUser user, string password) =>

        // Always use the latest/current hash algorithm when hashing new passwords for storage.
        // NOTE: This is only overridden to show that we can since we may need to adjust this in the future
        // if new/different formats are required.
        base.HashPassword(user, password);

    /// <summary>
    ///     Verifies a user's hashed password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="hashedPassword"></param>
    /// <param name="providedPassword"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This will check the user's current hashed password format stored with their user row and use that to verify the
    ///     hash. This could be any hashes
    ///     from the very old v4, to the older v6-v8, to the older aspnet identity and finally to the most recent
    /// </remarks>
    public override PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            // Best case and most likely scenario, a modern hash supported by ASP.Net identity.
            PasswordVerificationResult upstreamResult =
                base.VerifyHashedPassword(user, hashedPassword, providedPassword);
            if (upstreamResult != PasswordVerificationResult.Failed)
            {
                return upstreamResult;
            }
        }
        catch (FormatException)
        {
            // hash wasn't a valid base64 encoded string, MS concat the salt bytes and hash bytes and base 64 encode both together.
            // We however historically base 64 encoded the salt bytes and hash bytes separately then concat the strings so we got 2 sets of padding.
            // both salt bytes and hash bytes lengths were not evenly divisible by 3 hence 2 sets of padding.

            // We could check upfront with TryFromBase64String, but not whilst we target netstandard 2.0
            // so might as well just deal with the exception.
        }

        // At this point we either have a legacy password or a bad attempt.

        // Check the supported worst case scenario, a "useLegacyEncoding" password - HMACSHA1 but with password used as key so not unique for users sharing same password
        // This was the standard for v4.
        // Do this first because with useLegacyEncoding the algorithm stored in the database is irrelevant.
        if (LegacyPasswordSecurity.VerifyLegacyHashedPassword(providedPassword, hashedPassword))
        {
            return PasswordVerificationResult.SuccessRehashNeeded;
        }

        // For users we expect to know the historic algorithm.
        // NOTE: MemberPasswordHasher subclasses this class to deal with the fact that PasswordConfig wasn't stored.
        if (user.PasswordConfig.IsNullOrWhiteSpace())
        {
            return PasswordVerificationResult.Failed;
        }

        PersistedPasswordSettings? deserialized;
        try
        {
            deserialized = _jsonSerializer.Deserialize<PersistedPasswordSettings>(user.PasswordConfig ?? string.Empty);
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }

        if (deserialized?.HashAlgorithm is null ||
            !LegacyPasswordSecurity.SupportHashAlgorithm(deserialized.HashAlgorithm))
        {
            return PasswordVerificationResult.Failed;
        }

        // Last chance must be HMACSHA256 or SHA1
        return LegacyPasswordSecurity.VerifyPassword(deserialized.HashAlgorithm, providedPassword, hashedPassword)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Failed;
    }
}
