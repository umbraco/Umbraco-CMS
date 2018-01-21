using System;

namespace Umbraco.Core
{
    /// <summary>
    /// A lightweight struct for storing a pair of types for caching.
    /// </summary>
    internal struct TypePair : IEquatable<TypePair>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypePair"/> struct.
        /// </summary>
        /// <param name="type1">The first type</param>
        /// <param name="type2">The second type</param>
        public TypePair(Type type1, Type type2)
        {
            this.Type1 = type1;
            this.Type2 = type2;
        }

        /// <summary>
        /// Gets the first type
        /// </summary>
        public Type Type1 { get; }

        /// <summary>
        /// Gets the second type
        /// </summary>
        public Type Type2 { get; }

        /// <inheritdoc/>
        public bool Equals(TypePair other)
        {
            return this.Type1 == other.Type1 && this.Type2 == other.Type2;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return (obj is TypePair) && this.Equals((TypePair)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Type1.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Type2.GetHashCode();
                return hashCode;
            }
        }
    }
}
