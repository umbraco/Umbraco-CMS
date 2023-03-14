namespace Umbraco.Cms.Core.WebAssets;

public struct BundlingOptions : IEquatable<BundlingOptions>
{
    public BundlingOptions(bool optimizeOutput = true, bool enabledCompositeFiles = true)
    {
        OptimizeOutput = optimizeOutput;
        EnabledCompositeFiles = enabledCompositeFiles;
    }

    public static BundlingOptions OptimizedAndComposite => new(true);

    public static BundlingOptions OptimizedNotComposite => new(true, false);

    public static BundlingOptions NotOptimizedNotComposite => new(false, false);

    public static BundlingOptions NotOptimizedAndComposite => new(false);

    /// <summary>
    ///     If true, the files in the bundle will be minified
    /// </summary>
    public bool OptimizeOutput { get; }

    /// <summary>
    ///     If true, the files in the bundle will be combined, if false the files
    ///     will be served as individual files.
    /// </summary>
    public bool EnabledCompositeFiles { get; }

    public static bool operator ==(BundlingOptions left, BundlingOptions right) => left.Equals(right);

    public override bool Equals(object? obj) => obj is BundlingOptions options && Equals(options);

    public bool Equals(BundlingOptions other) => OptimizeOutput == other.OptimizeOutput &&
                                                 EnabledCompositeFiles == other.EnabledCompositeFiles;

    public override int GetHashCode()
    {
        var hashCode = 2130304063;
        hashCode = (hashCode * -1521134295) + OptimizeOutput.GetHashCode();
        hashCode = (hashCode * -1521134295) + EnabledCompositeFiles.GetHashCode();
        return hashCode;
    }

    public static bool operator !=(BundlingOptions left, BundlingOptions right) => !(left == right);
}
