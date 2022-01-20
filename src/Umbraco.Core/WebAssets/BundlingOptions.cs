using System;

namespace Umbraco.Cms.Core.WebAssets
{
    public struct BundlingOptions : IEquatable<BundlingOptions>
    {
        public static BundlingOptions OptimizedAndComposite => new BundlingOptions(true, true);
        public static BundlingOptions OptimizedNotComposite => new BundlingOptions(true, false);
        public static BundlingOptions NotOptimizedNotComposite => new BundlingOptions(false, false);
        public static BundlingOptions NotOptimizedAndComposite => new BundlingOptions(false, true);

        public BundlingOptions(bool optimizeOutput = true, bool enabledCompositeFiles = true)
        {
            OptimizeOutput = optimizeOutput;
            EnabledCompositeFiles = enabledCompositeFiles;
        }

        /// <summary>
        /// If true, the files in the bundle will be minified
        /// </summary>
        public bool OptimizeOutput { get; }

        /// <summary>
        /// If true, the files in the bundle will be combined, if false the files
        /// will be served as individual files.
        /// </summary>
        public bool EnabledCompositeFiles { get; }

        public override bool Equals(object obj) => obj is BundlingOptions options && Equals(options);
        public bool Equals(BundlingOptions other) => OptimizeOutput == other.OptimizeOutput && EnabledCompositeFiles == other.EnabledCompositeFiles;

        public override int GetHashCode()
        {
            int hashCode = 2130304063;
            hashCode = hashCode * -1521134295 + OptimizeOutput.GetHashCode();
            hashCode = hashCode * -1521134295 + EnabledCompositeFiles.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(BundlingOptions left, BundlingOptions right) => left.Equals(right);

        public static bool operator !=(BundlingOptions left, BundlingOptions right) => !(left == right);
    }
}
