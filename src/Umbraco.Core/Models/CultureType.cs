using System;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A <see cref="CultureType"/> represents either All cultures, a Single culture or the Invariant culture
    /// </summary>
    internal class CultureType
    {
        /// <summary>
        /// Represents All cultures
        /// </summary>
        public static CultureType All { get; } = new CultureType("*");

        /// <summary>
        /// Represents the Invariant culture
        /// </summary>
        public static CultureType Invariant { get; } = new CultureType(null);

        /// <summary>
        /// Represents a Single culture
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public static CultureType Single(string culture, bool isDefault)
        {
            return new CultureType(culture, isDefault);
        }

        private CultureType(string culture, bool isDefault = false)
        {
            Culture = culture;
            IsDefaultCulture = isDefault;
        }

        public string Culture { get; }
        public Behavior CultureBehavior
        {
            get
            {
                if (Culture == "*") return Behavior.All;
                if (Culture == null) return Behavior.Invariant;

                var result = Behavior.Explicit;

                //if the explicit culture is the default, then the behavior is also Invariant
                if (IsDefaultCulture)
                    result |= Behavior.Invariant;

                return result;
            }
        }

        public bool IsDefaultCulture { get; }

        [Flags]
        public enum Behavior : byte
        {
            All = 0,
            Invariant = 1,
            Explicit = 2
        }
    }
}
