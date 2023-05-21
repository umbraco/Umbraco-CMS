namespace Umbraco.Cms.Core;

/// <summary>
///     Provides ways to create attempts.
/// </summary>
public static class Attempt
{
    // note:
    // cannot rely on overloads only to differentiate between with/without status
    // in some cases it will always be ambiguous, so be explicit w/ 'WithStatus' methods

    /// <summary>
    ///     Creates a successful attempt with a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The successful attempt.</returns>
    public static Attempt<TResult?> Succeed<TResult>(TResult? result) => Attempt<TResult?>.Succeed(result);

    /// <summary>
    ///     Creates a successful attempt with a result and a status.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <typeparam name="TStatus">The type of the attempted operation status.</typeparam>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The successful attempt.</returns>
    public static Attempt<TResult, TStatus> SucceedWithStatus<TResult, TStatus>(TStatus status, TResult result) =>
        Attempt<TResult, TStatus>.Succeed(status, result);

    /// <summary>
    ///     Creates a failed attempt.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult> Fail<TResult>() => Attempt<TResult>.Fail();

    /// <summary>
    ///     Creates a failed attempt with a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult?> Fail<TResult>(TResult result) => Attempt<TResult?>.Fail(result);

    /// <summary>
    ///     Creates a failed attempt with a result and a status.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <typeparam name="TStatus">The type of the attempted operation status.</typeparam>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult, TStatus> FailWithStatus<TResult, TStatus>(TStatus status, TResult result) =>
        Attempt<TResult, TStatus>.Fail(status, result);

    /// <summary>
    ///     Creates a failed attempt with a result and an exception.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <param name="result">The result of the attempt.</param>
    /// <param name="exception">The exception causing the failure of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult?> Fail<TResult>(TResult result, Exception exception) =>
        Attempt<TResult>.Fail(result, exception);

    /// <summary>
    ///     Creates a failed attempt with a result, an exception and a status.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <typeparam name="TStatus">The type of the attempted operation status.</typeparam>
    /// <param name="status">The status of the attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <param name="exception">The exception causing the failure of the attempt.</param>
    /// <returns>The failed attempt.</returns>
    public static Attempt<TResult, TStatus> FailWithStatus<TResult, TStatus>(TStatus status, TResult result, Exception exception) => Attempt<TResult, TStatus>.Fail(status, result, exception);

    /// <summary>
    ///     Creates a successful or a failed attempt, with a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <param name="condition">A value indicating whether the attempt is successful.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The attempt.</returns>
    public static Attempt<TResult> If<TResult>(bool condition, TResult result) =>
        Attempt<TResult>.If(condition, result);

    /// <summary>
    ///     Creates a successful or a failed attempt, with a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the attempted operation result.</typeparam>
    /// <typeparam name="TStatus">The type of the attempted operation status.</typeparam>
    /// <param name="condition">A value indicating whether the attempt is successful.</param>
    /// <param name="succStatus">The status of the successful attempt.</param>
    /// <param name="failStatus">The status of the failed attempt.</param>
    /// <param name="result">The result of the attempt.</param>
    /// <returns>The attempt.</returns>
    public static Attempt<TResult, TStatus> IfWithStatus<TResult, TStatus>(
        bool condition,
        TStatus succStatus,
        TStatus failStatus,
        TResult result) =>
        Attempt<TResult, TStatus>.If(
            condition,
            succStatus,
            failStatus,
            result);
}
