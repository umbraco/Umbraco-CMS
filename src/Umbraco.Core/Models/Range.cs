using System;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a range with a minimum and maximum value.
    /// </summary>
    /// <typeparam name="T">The type of the minimum and maximum values.</typeparam>
    public class Range<T>
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
        public override string ToString() => string.Format("{0},{1}", this.Minimum, this.Maximum);

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
    }
}
