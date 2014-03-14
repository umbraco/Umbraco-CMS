using System;
using Umbraco.Core.Dynamics;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the result of an operation attempt.
    /// </summary>
    /// <typeparam name="T">The type of the attempted operation result.</typeparam>
    [Serializable]
	public struct Attempt<T>
	{
		private readonly bool _success;
		private readonly T _result;
		private readonly Exception _exception;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Attempt{T}"/> was successful.
        /// </summary>
        public bool Success
		{
			get { return _success; }
		}

        /// <summary>
        /// Gets the exception associated with an unsuccessful attempt.
        /// </summary>
        public Exception Exception { get { return _exception; } }

        /// <summary>
        /// Gets the exception associated with an unsuccessful attempt.
        /// </summary>
        /// <remarks>Keep it for backward compatibility sake.</remarks>
        [Obsolete(".Error is obsolete, you should use .Exception instead.", false)]
        public Exception Error { get { return _exception; } }

        /// <summary>
        /// Gets the attempt result.
        /// </summary>
        public T Result
		{
			get { return _result; }
		}

        // optimize, use a singleton failed attempt
		private static readonly Attempt<T> Failed = new Attempt<T>(false, default(T), null);

        /// <summary>
        /// Represents an unsuccessful attempt.
        /// </summary>
        /// <remarks>Keep it for backward compatibility sake.</remarks>
        [Obsolete(".Failed is obsolete, you should use Attempt<T>.Fail() instead.", false)]
        public static readonly Attempt<T> False = Failed; 

        // private - use Succeed() or Fail() methods to create attempts
        private Attempt(bool success, T result, Exception exception)
        {
            _success = success;
            _result = result;
            _exception = exception;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="Attempt{T}"/> struct with a result.
        /// </summary>
        /// <param name="success">A value indicating whether the attempt is successful.</param>
        /// <param name="result">The result of the attempt.</param>
        /// <remarks>Keep it for backward compatibility sake.</remarks>
        [Obsolete("Attempt ctors are obsolete, you should use Attempt<T>.Succeed(), Attempt<T>.Fail() or Attempt<T>.If() instead.", false)]
        public Attempt(bool success, T result)
            : this(success, result, null)
        { }

        /// <summary>
        /// Initialize a new instance of the <see cref="Attempt{T}"/> struct representing a failed attempt, with an exception.
        /// </summary>
        /// <param name="exception">The exception causing the failure of the attempt.</param>
        /// <remarks>Keep it for backward compatibility sake.</remarks>
        [Obsolete("Attempt ctors are obsolete, you should use Attempt<T>.Succeed(), Attempt<T>.Fail() or Attempt<T>.If() instead.", false)]
        public Attempt(Exception exception)
            : this(false, default(T), exception)
        { }

        /// <summary>
        /// Creates a successful attempt.
        /// </summary>
        /// <returns>The successful attempt.</returns>
        public static Attempt<T> Succeed()
        {
            return new Attempt<T>(true, default(T), null);
        }

        /// <summary>
        /// Creates a successful attempt with a result.
        /// </summary>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The successful attempt.</returns>
        public static Attempt<T> Succeed(T result)
        {
            return new Attempt<T>(true, result, null);
        }

        /// <summary>
        /// Creates a failed attempt.
        /// </summary>
        /// <returns>The failed attempt.</returns>
        public static Attempt<T> Fail()
        {
            return Failed;
        }

        /// <summary>
        /// Creates a failed attempt with an exception.
        /// </summary>
        /// <param name="exception">The exception causing the failure of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<T> Fail(Exception exception)
        {
            return new Attempt<T>(false, default(T), exception);
        }

        /// <summary>
        /// Creates a failed attempt with a result.
        /// </summary>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<T> Fail(T result)
        {
            return new Attempt<T>(false, result, null);
        }

        /// <summary>
        /// Creates a failed attempt with a result and an exception.
        /// </summary>
        /// <param name="result">The result of the attempt.</param>
        /// <param name="exception">The exception causing the failure of the attempt.</param>
        /// <returns>The failed attempt.</returns>
        public static Attempt<T> Fail(T result, Exception exception)
        {
            return new Attempt<T>(false, result, exception);
        }

        /// <summary>
        /// Creates a successful or a failed attempt.
        /// </summary>
        /// <param name="condition">A value indicating whether the attempt is successful.</param>
        /// <returns>The attempt.</returns>
        public static Attempt<T> SucceedIf(bool condition)
        {
            return condition ? new Attempt<T>(true, default(T), null) : Failed;
        }

        /// <summary>
        /// Creates a successful or a failed attempt, with a result.
        /// </summary>
        /// <param name="condition">A value indicating whether the attempt is successful.</param>
        /// <param name="result">The result of the attempt.</param>
        /// <returns>The attempt.</returns>
        public static Attempt<T> SucceedIf(bool condition, T result)
        {
            return new Attempt<T>(condition, result, null);
        }

        /// <summary>
        /// Implicity operator to check if the attempt was successful without having to access the 'success' property
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static implicit operator bool(Attempt<T> a)
        {
            return a.Success;
        }
    }
}