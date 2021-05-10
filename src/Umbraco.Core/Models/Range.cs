using System;
using System.Globalization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a range with a minimum and maximum value.
    /// </summary>
    /// <typeparam name="T">The type of the minimum and maximum values.</typeparam>
    /// <seealso cref="System.IEquatable{Umbraco.Core.Models.Range{T}}" />
    public class Range<T> : IEquatable<Range<T>>
        where T : IComparable<T>
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public T Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public T Maximum { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => this.ToString("{0},{1}", CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">A composite format string for a single value (minimum and maximum are equal). Use {0} for the minimum and {1} for the maximum value.</param>
        /// <param name="formatRange">A composite format string for the range values. Use {0} for the minimum and {1} for the maximum value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, string formatRange, IFormatProvider provider = null) => this.ToString(this.Minimum.CompareTo(this.Maximum) == 0 ? format : formatRange, provider);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">A composite format string for the range values. Use {0} for the minimum and {1} for the maximum value.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider provider = null) => string.Format(provider, format, this.Minimum, this.Maximum);

        /// <summary>
        /// Determines whether this range is valid (the minimum value is lower than or equal to the maximum value).
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this range is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid() => this.Minimum.CompareTo(this.Maximum) <= 0;

        /// <summary>
        /// Determines whether this range contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if this range contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(T value) => this.Minimum.CompareTo(value) <= 0 && value.CompareTo(this.Maximum) <= 0;

        /// <summary>
        /// Determines whether this range is inside the specified range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>
        ///   <c>true</c> if this range is inside the specified range; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInsideRange(Range<T> range) => this.IsValid() && range.IsValid() && range.ContainsValue(this.Minimum) && range.ContainsValue(this.Maximum);

        /// <summary>
        /// Determines whether this range contains the specified range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>
        ///   <c>true</c> if this range contains the specified range; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsRange(Range<T> range) => this.IsValid() && range.IsValid() && this.ContainsValue(range.Minimum) && this.ContainsValue(range.Maximum);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is Range<T> other && this.Equals(other);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Range<T> other) => other != null && this.Equals(other.Minimum, other.Maximum);

        /// <summary>
        /// Determines whether the specified <paramref name="minimum" /> and <paramref name="maximum" /> values are equal to this instance values.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <returns>
        ///   <c>true</c> if the specified <paramref name="minimum" /> and <paramref name="maximum" /> values are equal to this instance values; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(T minimum, T maximum) => this.Minimum.CompareTo(minimum) == 0 && this.Maximum.CompareTo(maximum) == 0;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => (this.Minimum, this.Maximum).GetHashCode();
    }
}
