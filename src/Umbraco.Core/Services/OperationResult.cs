using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Services;

// TODO: no need for Attempt<OperationResult> - the operation result SHOULD KNOW if it's a success or a failure!
// but then each WhateverResultType must

/// <summary>
///     Represents the result of a service operation.
/// </summary>
/// <typeparam name="TResultType">The type of the result type.</typeparam>
/// <remarks>
///     Type <typeparamref name="TResultType" /> must be an enumeration, and its
///     underlying type must be byte. Values indicating success should be in the 0-127
///     range, while values indicating failure should be in the 128-255 range. See
///     <see cref="OperationResultType" /> for a base implementation.
/// </remarks>
public class OperationResult<TResultType>
    where TResultType : struct
{
    static OperationResult()
    {
        // ensure that TResultType is an enum and the underlying type is byte
        // so we can safely cast in Success and test against 128 for failures
        Type type = typeof(TResultType);
        if (type.IsEnum == false)
        {
            throw new InvalidOperationException($"Type {type} is not an enum.");
        }

        if (Enum.GetUnderlyingType(type) != typeof(byte))
        {
            throw new InvalidOperationException($"Enum {type} underlying type is not <byte>.");
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationResult{TResultType}" /> class.
    /// </summary>
    public OperationResult(TResultType result, EventMessages? eventMessages)
    {
        Result = result;
        EventMessages = eventMessages;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success => ((byte)(object)Result & 128) == 0; // we *know* it's a byte

    /// <summary>
    ///     Gets the result of the operation.
    /// </summary>
    public TResultType Result { get; }

    /// <summary>
    ///     Gets the event messages produced by the operation.
    /// </summary>
    public EventMessages? EventMessages { get; }
}

/// <inheritdoc />
/// <summary>
///     Represents the result of a service operation for a given entity.
/// </summary>
/// <typeparam name="TResultType">The type of the result type.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
///     Type <typeparamref name="TResultType" /> must be an enumeration, and its
///     underlying type must be byte. Values indicating success should be in the 0-127
///     range, while values indicating failure should be in the 128-255 range. See
///     <see cref="OperationResultType" /> for a base implementation.
/// </remarks>
public class OperationResult<TResultType, TEntity> : OperationResult<TResultType>
    where TResultType : struct
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationResult{TResultType, TEntity}" /> class.
    /// </summary>
    /// <param name="result">The status of the operation.</param>
    /// <param name="eventMessages">Event messages produced by the operation.</param>
    public OperationResult(TResultType result, EventMessages eventMessages)
        : base(result, eventMessages)
    {
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationResult{TResultType, TEntity}" /> class.
    /// </summary>
    public OperationResult(TResultType result, EventMessages? eventMessages, TEntity? entity)
        : base(result, eventMessages) =>
        Entity = entity;

    /// <summary>
    ///     Gets the entity.
    /// </summary>
    public TEntity? Entity { get; }
}

/// <inheritdoc />
/// <summary>
///     Represents the default operation result.
/// </summary>
public class OperationResult : OperationResult<OperationResultType>
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationResult" /> class with a status and event messages.
    /// </summary>
    /// <param name="result">The status of the operation.</param>
    /// <param name="eventMessages">Event messages produced by the operation.</param>
    public OperationResult(OperationResultType result, EventMessages eventMessages)
        : base(result, eventMessages)
    {
    }

    public static OperationResult Succeed(EventMessages eventMessages) =>
        new OperationResult(OperationResultType.Success, eventMessages);

    public static OperationResult Cancel(EventMessages eventMessages) =>
        new OperationResult(OperationResultType.FailedCancelledByEvent, eventMessages);

    // TODO: this exists to support services that still return Attempt<OperationResult>
    // these services should directly return an OperationResult, and then this static class should be deleted
    public static class Attempt
    {
        /// <summary>
        ///     Creates a successful operation attempt.
        /// </summary>
        /// <param name="eventMessages">The event messages produced by the operation.</param>
        /// <returns>A new attempt instance.</returns>
        public static Attempt<OperationResult?> Succeed(EventMessages eventMessages) =>
            Core.Attempt.Succeed(new OperationResult(OperationResultType.Success, eventMessages));

        public static Attempt<OperationResult<OperationResultType, TValue>?>
            Succeed<TValue>(EventMessages eventMessages) => Core.Attempt.Succeed(
            new OperationResult<OperationResultType, TValue>(OperationResultType.Success, eventMessages));

        public static Attempt<OperationResult<OperationResultType, TValue>?>
            Succeed<TValue>(EventMessages eventMessages, TValue value) => Core.Attempt.Succeed(
            new OperationResult<OperationResultType, TValue>(OperationResultType.Success, eventMessages, value));

        public static Attempt<OperationResult<TStatusType>?> Succeed<TStatusType>(
            TStatusType statusType,
            EventMessages eventMessages)
            where TStatusType : struct =>
            Core.Attempt.Succeed(new OperationResult<TStatusType>(statusType, eventMessages));

        public static Attempt<OperationResult<TStatusType, TValue>?> Succeed<TStatusType, TValue>(
            TStatusType statusType, EventMessages eventMessages, TValue value)
            where TStatusType : struct =>
            Core.Attempt.Succeed(new OperationResult<TStatusType, TValue>(statusType, eventMessages, value));

        /// <summary>
        ///     Creates a successful operation attempt indicating that nothing was done.
        /// </summary>
        /// <param name="eventMessages">The event messages produced by the operation.</param>
        /// <returns>A new attempt instance.</returns>
        public static Attempt<OperationResult?> NoOperation(EventMessages eventMessages) =>
            Core.Attempt.Succeed(new OperationResult(OperationResultType.NoOperation, eventMessages));

        /// <summary>
        ///     Creates a failed operation attempt indicating that the operation has been cancelled.
        /// </summary>
        /// <param name="eventMessages">The event messages produced by the operation.</param>
        /// <returns>A new attempt instance.</returns>
        public static Attempt<OperationResult?> Cancel(EventMessages eventMessages) =>
            Core.Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, eventMessages));

        public static Attempt<OperationResult<OperationResultType, TValue>?>
            Cancel<TValue>(EventMessages eventMessages) => Core.Attempt.Fail(
            new OperationResult<OperationResultType, TValue>(
                OperationResultType.FailedCancelledByEvent,
                eventMessages));

        public static Attempt<OperationResult<OperationResultType, TValue>?>
            Cancel<TValue>(EventMessages eventMessages, TValue value) => Core.Attempt.Fail(
            new OperationResult<OperationResultType, TValue>(OperationResultType.FailedCancelledByEvent, eventMessages, value));

        /// <summary>
        ///     Creates a failed operation attempt indicating that an exception was thrown during the operation.
        /// </summary>
        /// <param name="eventMessages">The event messages produced by the operation.</param>
        /// <param name="exception">The exception that caused the operation to fail.</param>
        /// <returns>A new attempt instance.</returns>
        public static Attempt<OperationResult?> Fail(EventMessages eventMessages, Exception exception)
        {
            eventMessages.Add(new EventMessage(string.Empty, exception.Message, EventMessageType.Error));
            return Core.Attempt.Fail(
                new OperationResult(OperationResultType.FailedExceptionThrown, eventMessages),
                exception);
        }

        public static Attempt<OperationResult<OperationResultType, TValue>?>
            Fail<TValue>(EventMessages eventMessages, Exception exception) => Core.Attempt.Fail(
            new OperationResult<OperationResultType, TValue>(OperationResultType.FailedExceptionThrown, eventMessages),
            exception);

        public static Attempt<OperationResult<TStatusType>?> Fail<TStatusType>(
            TStatusType statusType,
            EventMessages eventMessages)
            where TStatusType : struct =>
            Core.Attempt.Fail(new OperationResult<TStatusType>(statusType, eventMessages));

        public static Attempt<OperationResult<TStatusType>?> Fail<TStatusType>(
            TStatusType statusType,
            EventMessages eventMessages,
            Exception exception)
            where TStatusType : struct =>
            Core.Attempt.Fail(new OperationResult<TStatusType>(statusType, eventMessages), exception);

        public static Attempt<OperationResult<TStatusType, TValue>?> Fail<TStatusType, TValue>(
            TStatusType statusType,
            EventMessages eventMessages)
            where TStatusType : struct =>
            Core.Attempt.Fail(new OperationResult<TStatusType, TValue>(statusType, eventMessages));

        public static Attempt<OperationResult<TStatusType, TValue>?> Fail<TStatusType, TValue>(
            TStatusType statusType,
            EventMessages eventMessages,
            TValue value)
            where TStatusType : struct =>
            Core.Attempt.Fail(new OperationResult<TStatusType, TValue>(statusType, eventMessages, value));

        public static Attempt<OperationResult<TStatusType, TValue>?> Fail<TStatusType, TValue>(
            TStatusType statusType,
            EventMessages eventMessages,
            Exception exception)
            where TStatusType : struct =>
            Core.Attempt.Fail(new OperationResult<TStatusType, TValue>(statusType, eventMessages), exception);

        public static Attempt<OperationResult<TStatusType, TValue>?> Fail<TStatusType, TValue>(
            TStatusType statusType,
            EventMessages eventMessages,
            TValue value,
            Exception exception)
            where TStatusType : struct =>
            Core.Attempt.Fail(new OperationResult<TStatusType, TValue>(statusType, eventMessages, value), exception);

        public static Attempt<OperationResult<OperationResultType, TValue>?>
            Cannot<TValue>(EventMessages eventMessages) => Core.Attempt.Fail(
            new OperationResult<OperationResultType, TValue>(OperationResultType.FailedCannot, eventMessages));
    }
}
