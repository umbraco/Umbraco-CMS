using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to enums.
    /// </summary>
    public static class EnumExtensions
    {
        // note:
        // - no need to HasFlagExact, that's basically an == test
        // - HasFlagAll cannot be named HasFlag because ext. methods never take priority over instance methods

        /// <summary>
        /// Determines whether a flag enum has all the specified values.
        /// </summary>
        /// <remarks>
        /// <para>True when all bits set in <paramref name="uses"/> are set in <paramref name="use"/>, though other bits may be set too.</para>
        /// <para>This is the behavior of the original <see cref="Enum.HasFlag"/> method.</para>
        /// </remarks>
        public static bool HasFlagAll<T>(this T use, T uses)
            where T : Enum
        {
            var num = Convert.ToUInt64(use);
            var nums = Convert.ToUInt64(uses);

            return (num & nums) == nums;
        }

        /// <summary>
        /// Determines whether a flag enum has any of the specified values.
        /// </summary>
        /// <remarks>
        /// <para>True when at least one of the bits set in <paramref name="uses"/> is set in <paramref name="use"/>.</para>
        /// </remarks>
        public static bool HasFlagAny<T>(this T use, T uses)
            where T : Enum
        {
            var num = Convert.ToUInt64(use);
            var nums = Convert.ToUInt64(uses);

            return (num & nums) > 0;
        }
    }
}
