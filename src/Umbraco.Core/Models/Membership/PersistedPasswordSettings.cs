using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     The data stored against the user for their password configuration
/// </summary>
[DataContract(Name = "userPasswordSettings", Namespace = "")]
public class PersistedPasswordSettings
{
    /// <summary>
    ///     The algorithm name
    /// </summary>
    /// <remarks>
    ///     This doesn't explicitly need to map to a 'true' algorithm name, this may match an algorithm name alias that
    ///     uses many different options such as PBKDF2.ASPNETCORE.V3 which would map to the aspnetcore's v3 implementation of
    ///     PBKDF2
    ///     PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.
    /// </remarks>
    [DataMember(Name = "hashAlgorithm")]
    public string? HashAlgorithm { get; set; }
}
