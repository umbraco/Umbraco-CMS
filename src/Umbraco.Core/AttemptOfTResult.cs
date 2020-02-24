﻿using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the result of an operation attempt.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    [Serializable]
    public struct Attempt<TResult>
    {
        // private - use Succeed() or Fail() methods to create attempts
        private Attempt(bool success, TResult result, Exception exception)
        {
            Success = success;
            Result = result;
            Exception = exception;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Attempt{TResult}"/> was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the exception associated with an unsuccessful attempt.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the attempt result.
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// Gets the attempt result, if successful, else a default value.
        /// </summary>
        public TResult ResultOr(TResult value) => Success ? Result : value;

        // optimize, use a singleton failed attempt
        private static readonly Attempt<TResult> Failed = new Attempt<TResult>(false, default(TResult), null);

        /// <summary>
        /// Creates a successful attempt.
        /// </summary>
        /// <returns>The successful attempt.</returns>
        public static Attempt<TResult> Succeed()
        {
            return new Attempt<TResult>(true, default(TResult), null);
        }

        /// <summary>
        /// Creates a successful attempt with a result.
        /// </summary>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The successful attempt.</returns>
        public static Attempt<TResult> Succeed(TResult result)
        {
            return new Attempt<TResult>(true, result, null);
        }

        /// <summary>
        /// Creates a failed attempt.
        /// </summary>
        /// <returns>The failed attempt.</returns>
        public static Attempt<TResult> Fail()
        {
            return Failed;
        }

        /// <summary>
        /// Creates a failed attempt with an exception.
        /// </summary>
        /// <param name="exception">The exception causing the failure of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<TResult> Fail(Exception exception)
        {
            return new Attempt<TResult>(false, default(TResult), exception);
        }

        /// <summary>
        /// Creates a failed attempt with a result.
        /// </summary>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<TResult> Fail(TResult result)
        {
            return new Attempt<TResult>(false, result, null);
        }

        /// <summary>
        /// Creates a failed attempt with a result and an exception.
        /// </summary>
        /// <param name="result">The result of the attempt.</param>
        /// <param name="exception">The exception causing the failure of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<TResult> Fail(TResult result, Exception exception)
        {
            return new Attempt<TResult>(false, result, exception);
        }

        /// <summary>
        /// Creates a successful or a failed attempt.
        /// </summary>
        /// <param name="condition">A value indicating whether the attempt is successful.</param>
        /// <returns>The attempt.</returns>
        public static Attempt<TResult> If(bool condition)
        {
            return condition ? new Attempt<TResult>(true, default(TResult), null) : Failed;
        }

        /// <summary>
        /// Creates a successful or a failed attempt, with a result.
        /// </summary>
        /// <param name="condition">A value indicating whether the attempt is successful.</param>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The attempt.</returns>
        public static Attempt<TResult> If(bool condition, TResult result)
        {
            return new Attempt<TResult>(condition, result, null);
        }

        /// <summary>
        /// Implicitly operator to check if the attempt was successful without having to access the 'success' property
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static implicit operator bool(Attempt<TResult> a)
        {
            return a.Success;
        }
    }
}
