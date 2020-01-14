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

        /// <summary>
        /// Sets a flag of the given input enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">Enum to set flag of</param>
        /// <param name="flag">Flag to set</param>
        /// <returns>A new enum with the flag set</returns>
        public static T SetFlag<T>(this T input, T flag)
             where T : Enum
        {
            var i = Convert.ToUInt64(input);
            var f = Convert.ToUInt64(flag);

            // bitwise OR to set flag f of enum i
            var result = i | f;
            
            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// Unsets a flag of the given input enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">Enum to unset flag of</param>
        /// <param name="flag">Flag to unset</param>
        /// <returns>A new enum with the flag unset</returns>
        public static T UnsetFlag<T>(this T input, T flag)
             where T : Enum
        {
            var i = Convert.ToUInt64(input);
            var f = Convert.ToUInt64(flag);

            // bitwise AND combined with bitwise complement to unset flag f of enum i
            var result = i & ~f;
            
            return (T)Enum.ToObject(typeof(T), result);
        }
    }
}
