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
        public static CultureType Explicit(string culture, bool isDefault)
        {
            return new CultureType(culture, isDefault);
        }

        /// <summary>
        /// Creates a <see cref="CultureType"/> based on a <see cref="IContent"/> item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public static CultureType Create(IContent content, string culture, bool isDefault)
        {
            if (!TryCreate(content.ContentType.Variations, culture, isDefault, out var cultureType))
                throw new InvalidOperationException($"The null value for culture is reserved for invariant content but the content type {content.ContentType.Alias} is variant");

            return cultureType;
        }

        internal static bool TryCreate(ContentVariation variation, string culture, bool isDefault, out CultureType cultureType)
        {
            cultureType = null;
            if (culture == null || culture == "*")
            {
                if (culture == null && variation.VariesByCulture())
                    return false;

                //we support * for invariant since it means ALL but we need to explicitly translate it so it's behavior is Invariant
                if (culture == "*" && !variation.VariesByCulture())
                {
                    cultureType = new CultureType(null, isDefault);
                    return true;
                }                    
            }

            cultureType = new CultureType(culture, isDefault);
            return true;
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
                //null can only be invariant
                if (Culture == null) return Behavior.InvariantCulture | Behavior.InvariantProperties;

                // * is All which means its also invariant properties since this will include the default language
                if (Culture == "*") return (Behavior.AllCultures | Behavior.InvariantProperties);

                //else it's explicit
                var result = Behavior.ExplicitCulture;

                //if the explicit culture is the default, then the behavior is also InvariantProperties
                if (IsDefaultCulture)
                    result |= Behavior.InvariantProperties;

                return result;
            }
        }

        public bool IsDefaultCulture { get; }

        [Flags]
        public enum Behavior : byte
        {
            AllCultures = 1,
            InvariantCulture = 2,
            ExplicitCulture = 4,
            InvariantProperties = 8
        }
    }
}
