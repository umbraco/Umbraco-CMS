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
        public static Attempt<T> Succeed<T>(T result)
        {
            return Attempt<T>.Succeed(result);
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
            return Attempt<T>.SucceedIf(success, result);
        }


        /// <summary>
        /// Executes an attempt function, with callbacks.
        /// </summary>
        /// <typeparam name="T">The type of the attempted operation result.</typeparam>
        /// <param name="attempt">The attempt returned by the attempt function.</param>
        /// <param name="onSuccess">An action to execute in case the attempt succeeds.</param>
        /// <param name="onFail">An action to execute in case the attempt fails.</param>
        /// <returns>The outcome of the attempt.</returns>
        /// <remarks>Runs <paramref name="onSuccess"/> or <paramref name="onFail"/> depending on the
        /// whether the attempt function reports a success or a failure.</remarks>
        public static Outcome Try<T>(Attempt<T> attempt, Action<T> onSuccess, Action<Exception> onFail = null)
        {
            if (attempt.Success)
            {
                onSuccess(attempt.Result);
                return Outcome.Success;
            }

            if (onFail != null)
            {
                onFail(attempt.Exception);
            }

            return Outcome.Failure;
        }

        /// <summary>
        /// Represents the outcome of an attempt.
        /// </summary>
        /// <remarks>Can be a success or a failure, and allows for attempts chaining.</remarks>
        public struct Outcome
        {
            private readonly bool _success;

            /// <summary>
            /// Gets an outcome representing a success.
            /// </summary>
            public static readonly Outcome Success = new Outcome(true);

            /// <summary>
            /// Gets an outcome representing a failure.
            /// </summary>
            public static readonly Outcome Failure = new Outcome(false);

            private Outcome(bool success)
            {
                _success = success;
            }

            /// <summary>
            /// Executes another attempt function, if the previous one failed, with callbacks.
            /// </summary>
            /// <typeparam name="T">The type of the attempted operation result.</typeparam>
            /// <param name="nextFunction">The attempt function to execute, returning an attempt.</param>
            /// <param name="onSuccess">An action to execute in case the attempt succeeds.</param>
            /// <param name="onFail">An action to execute in case the attempt fails.</param>
            /// <returns>If it executes, returns the outcome of the attempt, else returns a success outcome.</returns>
            /// <remarks>
            /// <para>Executes only if the previous attempt failed, else does not execute and return a success outcome.</para>
            /// <para>If it executes, then runs <paramref name="onSuccess"/> or <paramref name="onFail"/> depending on the
            /// whether the attempt function reports a success or a failure.</para>
            /// </remarks>
            public Outcome OnFailure<T>(Func<Attempt<T>> nextFunction, Action<T> onSuccess, Action<Exception> onFail = null)
            {
                return _success
                    ? Success
                    : ExecuteNextFunction(nextFunction, onSuccess, onFail);
            }

            /// <summary>
            /// Executes another attempt function, if the previous one succeeded, with callbacks.
            /// </summary>
            /// <typeparam name="T">The type of the attempted operation result.</typeparam>
            /// <param name="nextFunction">The attempt function to execute, returning an attempt.</param>
            /// <param name="onSuccess">An action to execute in case the attempt succeeds.</param>
            /// <param name="onFail">An action to execute in case the attempt fails.</param>
            /// <returns>If it executes, returns the outcome of the attempt, else returns a failed outcome.</returns>
            /// <remarks>
            /// <para>Executes only if the previous attempt succeeded, else does not execute and return a success outcome.</para>
            /// <para>If it executes, then runs <paramref name="onSuccess"/> or <paramref name="onFail"/> depending on the
            /// whether the attempt function reports a success or a failure.</para>
            /// </remarks>
            public Outcome OnSuccess<T>(Func<Attempt<T>> nextFunction, Action<T> onSuccess, Action<Exception> onFail = null)
            {
                return _success
                    ? ExecuteNextFunction(nextFunction, onSuccess, onFail) 
                    : Failure;
            }

            private static Outcome ExecuteNextFunction<T>(Func<Attempt<T>> nextFunction, Action<T> onSuccess, Action<Exception> onFail = null)
            {
                var attempt = nextFunction();

                if (attempt.Success)
                {
                    onSuccess(attempt.Result);
                    return Success;
                }

                if (onFail != null)
                {
                    onFail(attempt.Exception);
                }

                return Failure;
            }
        }

    }
}