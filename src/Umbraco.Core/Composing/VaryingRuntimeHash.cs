namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     A runtime hash this is always different on each app startup
/// </summary>
public sealed class VaryingRuntimeHash : IRuntimeHash
{
    private readonly string _hash;

    public VaryingRuntimeHash() => _hash = DateTime.Now.Ticks.ToString();

    public string GetHashValue() => _hash;
}
