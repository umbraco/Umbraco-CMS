using System;

namespace Umbraco.Core
{
	/// <summary>
	/// Represents the result of an operation attempt
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <remarks></remarks>
	[Serializable]
	public struct Attempt<T>
	{
		private readonly bool _success;
		private readonly T _result;
		private readonly Exception _error;

		/// <summary>
		/// Gets a value indicating whether this <see cref="Attempt{T}"/> represents a successful operation.
		/// </summary>
		/// <remarks></remarks>
		public bool Success
		{
			get { return _success; }
		}

		/// <summary>
		/// Gets the error associated with an unsuccessful attempt.
		/// </summary>
		/// <value>The error.</value>
		public Exception Error { get { return _error; } }

		/// <summary>
		/// Gets the parse result.
		/// </summary>
		/// <remarks></remarks>
		public T Result
		{
			get { return _result; }
		}

		/// <summary>
		/// Represents an unsuccessful parse operation
		/// </summary>
		public static readonly Attempt<T> False;

		/// <summary>
		/// Initializes a new instance of the <see cref="Attempt{T}"/> struct.
		/// </summary>
		/// <param name="success">If set to <c>true</c> this tuple represents a successful parse result.</param>
		/// <param name="result">The parse result.</param>
		/// <remarks></remarks>
		public Attempt(bool success, T result)
		{
			_success = success;
			_result = result;
			_error = null;
		}

		public Attempt(Exception error)
		{
			_success = false;
			_result = default(T);
			_error = error;
		}
	}
}