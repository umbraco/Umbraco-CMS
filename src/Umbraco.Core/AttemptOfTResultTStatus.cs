namespace Umbraco.Cms.Core;

/// <summary>
///     Represents the result of an operation attempt.
/// </summary>
/// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
/// <typeparam name="TStatus">The type of the attempted operation status.</typeparam>
[Serializable]
public struct Attempt<TResult, TStatus>
{
    // private - use Succeed() or Fail() methods to create attempts
    private Attempt(bool success, TResult result, TStatus status, Exception? exception)
    {
        Success = success;
        Result = result;
        Status = status;
        Exception = exception;
    }

    /// <summary>
    ///     Gets a value indicating whether this <see cref="Attempt{TResult,TStatus}" /> was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    ///     Gets the exception associated with an unsuccessful attempt.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    ///     Gets the attempt result.
    /// </summary>
    public TResult Result { get; }

    /// <summary>
    ///     Gets the attempt status.
    /// </summary>
    public TStatus Status { get; }

    /// <summary>
    ///     Implicitly operator to check if the attempt was successful without having to access the 'success' property
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static implicit operator bool(Attempt<TResult, TStatus> a) => a.Success;

    /// <summary>
    ///     Creates a successful attempt.
    /// </summary>
    /// <param name="status">The status of the attempt.</param>
    /// <returns>The successful attempt.</returns>
    public static Attempt<TResult?, TStatus> Succeed(TStatus status) =>
        new Attempt<TResult?, TStatus>(true, default, status, null);

    /// <summary>
    ///     Creates a successful attempt with a result.
    /// </summary>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The successful attempt.</returns>
    public static Attempt<TResult, TStatus> Succeed(TStatus status, TResult result) =>
        new Attempt<TResult, TStatus>(true, result, status, null);

    /// <summary>
    ///     Creates a failed attempt.
    /// </summary>
    /// <param name="status">The status of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult?, TStatus> Fail(TStatus status) =>
        new Attempt<TResult?, TStatus>(false, default, status, null);

    /// <summary>
    ///     Creates a failed attempt with an exception.
    /// </summary>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="exception">The exception causing the failure of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult?, TStatus> Fail(TStatus status, Exception exception) =>
        new Attempt<TResult?, TStatus>(false, default, status, exception);

    /// <summary>
    ///     Creates a failed attempt with a result.
    /// </summary>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult, TStatus> Fail(TStatus status, TResult result) =>
        new Attempt<TResult, TStatus>(false, result, status, null);

    /// <summary>
    ///     Creates a failed attempt with a result and an exception.
    /// </summary>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <param name="exception">The exception causing the failure of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult, TStatus> Fail(TStatus status, TResult result, Exception exception) =>
        new Attempt<TResult, TStatus>(false, result, status, exception);

    /// <summary>
    ///     Creates a successful or a failed attempt.
    /// </summary>
    /// <param name="condition">A value indicating whether the attempt is successful.</param>
    /// <param name="succStatus">The status of the successful attempt.</param>
    /// <param name="failStatus">The status of the failed attempt.</param>
    /// <returns>The attempt.</returns>
    public static Attempt<TResult?, TStatus> If(bool condition, TStatus succStatus, TStatus failStatus) =>
        new Attempt<TResult?, TStatus>(condition, default, condition ? succStatus : failStatus, null);

    /// <summary>
    ///     Creates a successful or a failed attempt, with a result.
    /// </summary>
    /// <param name="condition">A value indicating whether the attempt is successful.</param>
    /// <param name="succStatus">The status of the successful attempt.</param>
    /// <param name="failStatus">The status of the failed attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The attempt.</returns>
    public static Attempt<TResult, TStatus>
        If(bool condition, TStatus succStatus, TStatus failStatus, TResult result) =>
        new Attempt<TResult, TStatus>(condition, result, condition ? succStatus : failStatus, null);
}
