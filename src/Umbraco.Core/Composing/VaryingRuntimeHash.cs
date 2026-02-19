namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     A runtime hash this is always different on each app startup
/// </summary>
public sealed class VaryingRuntimeHash : IRuntimeHash
{
    private readonly string _hash;

    /// <summary>
    /// Initializes a new instance of the <see cref="VaryingRuntimeHash" /> class.
    /// </summary>
    /// <remarks>
    /// The hash value is generated based on the current timestamp, ensuring
    /// a unique hash on each application startup.
    /// </remarks>
    public VaryingRuntimeHash() => _hash = DateTime.Now.Ticks.ToString();

    /// <inheritdoc />
    public string GetHashValue() => _hash;
}
