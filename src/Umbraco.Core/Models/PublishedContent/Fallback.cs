using System;
using System.Collections;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Manages the built-in fallback policies.
    /// </summary>
    public struct Fallback : IEnumerable<int>
    {
        private readonly int[] _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fallback"/> struct with values.
        /// </summary>
        private Fallback(int[] values)
        {
            _values = values;
        }

        /// <summary>
        /// Gets an ordered set of fallback policies.
        /// </summary>
        /// <param name="values"></param>
        public static Fallback To(params int[] values) => new Fallback(values);

        /// <summary>
        /// Do not fallback.
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// Fallback to default value.
        /// </summary>
        public const int DefaultValue = 1;

        /// <summary>
        /// Gets the fallback to default value policy.
        /// </summary>
        public static Fallback ToDefaultValue => new Fallback(new[] { DefaultValue });

        /// <summary>
        /// Fallback to other languages.
        /// </summary>
        public const int Language = 2;

        /// <summary>
        /// Gets the fallback to language policy.
        /// </summary>
        public static Fallback ToLanguage => new Fallback(new[] { Language });

        /// <summary>
        /// Fallback to tree ancestors.
        /// </summary>
        public const int Ancestors = 3;

        /// <summary>
        /// Gets the fallback to tree ancestors policy.
        /// </summary>
        public static Fallback ToAncestors => new Fallback(new[] { Ancestors });

        /// <inheritdoc />
        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)_values ?? Array.Empty<int>()).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
