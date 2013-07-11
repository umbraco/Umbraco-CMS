using System;

namespace Umbraco.Core
{
    public struct AttemptOutcome
    {
        private readonly bool _success;

        public AttemptOutcome(bool success)
        {
            _success = success;
        }

        public AttemptOutcome IfFailed<T>(Func<Attempt<T>> nextAttempt, Action<T> onSuccess, Action<Exception> onFail = null)
        {
            if (_success == false)
            {
                return ExecuteNextAttempt(nextAttempt, onSuccess, onFail);
            }
            
            //return a successful outcome since the last one was successful, this allows the next AttemptOutcome chained to 
            // continue properly.
            return new AttemptOutcome(true);
        }

        public AttemptOutcome IfSuccessful<T>(Func<Attempt<T>> nextAttempt, Action<T> onSuccess, Action<Exception> onFail = null)
        {
            if (_success)
            {
                return ExecuteNextAttempt(nextAttempt, onSuccess, onFail);
            }
            //return a failed outcome since the last one was not successful, this allows the next AttemptOutcome chained to 
            // continue properly.
            return new AttemptOutcome(false);
        }

        private AttemptOutcome ExecuteNextAttempt<T>(Func<Attempt<T>> nextAttempt, Action<T> onSuccess, Action<Exception> onFail = null)
        {
            var attempt = nextAttempt();
            if (attempt.Success)
            {
                onSuccess(attempt.Result);
                return new AttemptOutcome(true);
            }

            if (onFail != null)
            {
                onFail(attempt.Error);
            }
            return new AttemptOutcome(false);
        }
    }

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
        /// Perform the attempt with callbacks
        /// </summary>
        /// <param name="attempt"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        public static AttemptOutcome Try(Attempt<T> attempt, Action<T> onSuccess, Action<Exception> onFail = null)
        {
            if (attempt.Success)
            {
                onSuccess(attempt.Result);
                return new AttemptOutcome(true);
            }

            if (onFail != null)
            {
                onFail(attempt.Error);    
            }
            return new AttemptOutcome(false);
        }

		/// <summary>
		/// Represents an unsuccessful parse operation
		/// </summary>
		public static readonly Attempt<T> False = new Attempt<T>(false, default(T));

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