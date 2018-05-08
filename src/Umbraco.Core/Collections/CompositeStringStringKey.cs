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
            // fixme temp - debugging
            if (key1 == null) throw new Exception("Getting null culture in CompositeStringStringKey constructor, fix!");
            if (key2 == null) throw new Exception("Getting null segment in CompositeStringStringKey constructor, fix!");

            _key1 = key1?.ToLowerInvariant() ?? "NULL";
            _key2 = key2?.ToLowerInvariant() ?? "NULL";
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
