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
        public Behavior CultureBehavior => Culture == "*" ? Behavior.All : Culture == null ? Behavior.Invariant : Behavior.Explicit;
        public bool IsDefaultCulture { get; }

        public enum Behavior
        {
            All,
            Invariant,
            Explicit
        }
    }
}
