using System;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// Represents a composite key of (string, string) for fast dictionaries.
    /// </summary>
    /// <remarks>
    /// <para>The string parts of the key are case-insensitive.</para>
    /// <para>Null is a valid value for both parts.</para>
    /// </remarks>
    public struct CompositeStringStringKey : IEquatable<CompositeStringStringKey>
    {
        private readonly string _key1;
        private readonly string _key2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeStringStringKey"/> struct.
        /// </summary>
        public CompositeStringStringKey(string key1, string key2)
        {
            _key1 = key1?.ToLowerInvariant() ?? "NULL";

            //fixme - we are changing this to null if it is an empty string, this is because if we don't do this than this key will not match
            // anything see comments http://issues.umbraco.org/issue/U4-11227#comment=67-46399
            // since we're not dealing with segments right now and I just need to get something working, this is the 'fix'
            _key2 = !key2.IsNullOrWhiteSpace() ? key2.ToLowerInvariant() : "NULL";
        }

        public bool Equals(CompositeStringStringKey other)
            => _key2 == other._key2 && _key1 == other._key1;

        public override bool Equals(object obj)
            => obj is CompositeStringStringKey other && _key2 == other._key2 && _key1 == other._key1;

        public override int GetHashCode()
            => _key2.GetHashCode() * 31 + _key1.GetHashCode();

        public static bool operator ==(CompositeStringStringKey key1, CompositeStringStringKey key2)
            => key1._key2 == key2._key2 && key1._key1 == key2._key1;

        public static bool operator !=(CompositeStringStringKey key1, CompositeStringStringKey key2)
            => key1._key2 != key2._key2 || key1._key1 != key2._key1;
    }
}
