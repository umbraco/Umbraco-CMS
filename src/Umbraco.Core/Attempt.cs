using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides ways to create attempts.
    /// </summary>
    public static class Attempt
    {
        /// <summary>
        /// Creates a successful attempt with a result.
        /// </summary>
        /// <typeparam name="T">The type of the attempted operation result.</typeparam>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The successful attempt.</returns>
        public static Attempt<T> Succ<T>(T result)
        {
            return Attempt<T>.Succ(result);
        }

        /// <summary>
        /// Creates a failed attempt with a result.
        /// </summary>
        /// <typeparam name="T">The type of the attempted operation result.</typeparam>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<T> Fail<T>(T result)
        {
            return Attempt<T>.Fail(result);
        }

        /// <summary>
        /// Creates a failed attempt with a result and an exception.
        /// </summary>
        /// <typeparam name="T">The type of the attempted operation result.</typeparam>
        /// <param name="result">The result of the attempt.</param>
        /// <param name="exception">The exception causing the failure of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<T> Fail<T>(T result, Exception exception)
        {
            return Attempt<T>.Fail(result, exception);
        }

        /// <summary>
        /// Creates a successful or a failed attempt, with a result.
        /// </summary>
        /// <typeparam name="T">The type of the attempted operation result.</typeparam>
        /// <param name="success">A value indicating whether the attempt is successful.</param>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The attempt.</returns>
        public static Attempt<T> If<T>(bool success, T result)
        {
            return Attempt<T>.If(success, result);
        }
    }
}