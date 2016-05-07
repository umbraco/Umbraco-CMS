using System;
using Umbraco.Core.Events;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the status of a service operation.
    /// </summary>
    /// <typeparam name="TStatusType">The type of the status type.</typeparam>
    public class OperationStatus<TStatusType>
        where TStatusType : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStatus{TStatusType}"/> class with a status and event messages.
        /// </summary>
        /// <param name="statusType">The status of the operation.</param>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        public OperationStatus(TStatusType statusType, EventMessages eventMessages)
        {
            if (eventMessages == null) throw new ArgumentNullException(nameof(eventMessages));
            StatusType = statusType;
            EventMessages = eventMessages;
        }

        /// <summary>
        /// Gets or sets the status of the operation.
        /// </summary>
        /// <remarks>May be internally updated during the operation, but should NOT be updated once the operation has completed.</remarks>
        public TStatusType StatusType { get; internal set; }

        /// <summary>
        /// Gets the event messages produced by the operation.
        /// </summary>
        public EventMessages EventMessages { get; }
    }

    /// <summary>
    /// Represents the status of a service operation that manages (processes, produces...) a value.
    /// </summary>
    /// <typeparam name="TStatusType">The type of the status type.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class OperationStatus<TStatusType, TValue> : OperationStatus<TStatusType>
        where TStatusType : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStatus{TStatusType, TValue}"/> class with a status type and event messages.
        /// </summary>
        /// <param name="statusType">The status of the operation.</param>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        public OperationStatus(TStatusType statusType, EventMessages eventMessages)
            : base(statusType, eventMessages)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStatus{TStatusType, TValue}"/> class with a status type, event messages and a value.
        /// </summary>
        /// <param name="statusType">The status of the operation.</param>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        /// <param name="value">The value managed by the operation.</param>
        public OperationStatus(TStatusType statusType, EventMessages eventMessages, TValue value)
            : base(statusType, eventMessages)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value managed by the operation.
        /// </summary>
        public TValue Value { get; }
    }

    /// <summary>
    /// Represents the default operation status.
    /// </summary>
    /// <remarks>Also provides static helper methods to create operation statuses.</remarks>
    public class OperationStatus : OperationStatus<OperationStatusType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStatus"/> class with a status and event messages.
        /// </summary>
        /// <param name="statusType">The status of the operation.</param>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        public OperationStatus(OperationStatusType statusType, EventMessages eventMessages) 
            : base(statusType, eventMessages)
        { }

        internal static class Attempt
        {
            /// <summary>
            /// Creates a successful operation attempt.
            /// </summary>
            /// <param name="eventMessages">The event messages produced by the operation.</param>
            /// <returns>A new attempt instance.</returns>
            public static Attempt<OperationStatus> Succeed(EventMessages eventMessages)
            {
                return Core.Attempt.Succeed(new OperationStatus(OperationStatusType.Success, eventMessages));
            }

            public static Attempt<OperationStatus<OperationStatusType, TValue>> Succeed<TValue>(EventMessages eventMessages)
            {
                return Core.Attempt.Succeed(new OperationStatus<OperationStatusType, TValue>(OperationStatusType.Success, eventMessages));
            }

            public static Attempt<OperationStatus<OperationStatusType, TValue>> Succeed<TValue>(EventMessages eventMessages, TValue value)
            {
                return Core.Attempt.Succeed(new OperationStatus<OperationStatusType, TValue>(OperationStatusType.Success, eventMessages, value));
            }

            public static Attempt<OperationStatus<TStatusType>> Succeed<TStatusType>(TStatusType statusType, EventMessages eventMessages)
                where TStatusType : struct
            {
                return Core.Attempt.Succeed(new OperationStatus<TStatusType>(statusType, eventMessages));
            }

            public static Attempt<OperationStatus<TStatusType, TValue>> Succeed<TStatusType, TValue>(TStatusType statusType, EventMessages eventMessages, TValue value)
                where TStatusType : struct
            {
                return Core.Attempt.Succeed(new OperationStatus<TStatusType, TValue>(statusType, eventMessages, value));
            }

            /// <summary>
            /// Creates a successful operation attempt indicating that nothing was done.
            /// </summary>
            /// <param name="eventMessages">The event messages produced by the operation.</param>
            /// <returns>A new attempt instance.</returns>
            public static Attempt<OperationStatus> NoOperation(EventMessages eventMessages)
            {
                return Core.Attempt.Succeed(new OperationStatus(OperationStatusType.NoOperation, eventMessages));
            }

            /// <summary>
            /// Creates a failed operation attempt indicating that the operation has been cancelled.
            /// </summary>
            /// <param name="eventMessages">The event messages produced by the operation.</param>
            /// <returns>A new attempt instance.</returns>
            public static Attempt<OperationStatus> Cancel(EventMessages eventMessages)
            {
                return Core.Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, eventMessages));
            }

            public static Attempt<OperationStatus<OperationStatusType, TValue>> Cancel<TValue>(EventMessages eventMessages)
            {
                return Core.Attempt.Fail(new OperationStatus<OperationStatusType, TValue>(OperationStatusType.FailedCancelledByEvent, eventMessages));
            }

            public static Attempt<OperationStatus<OperationStatusType, TValue>> Cancel<TValue>(EventMessages eventMessages, TValue value)
            {
                return Core.Attempt.Fail(new OperationStatus<OperationStatusType, TValue>(OperationStatusType.FailedCancelledByEvent, eventMessages, value));
            }

            /// <summary>
            /// Creates a failed operation attempt indicating that an exception was thrown during the operation.
            /// </summary>
            /// <param name="eventMessages">The event messages produced by the operation.</param>
            /// <param name="exception">The exception that caused the operation to fail.</param>
            /// <returns>A new attempt instance.</returns>
            public static Attempt<OperationStatus> Fail(EventMessages eventMessages, Exception exception)
            {
                eventMessages.Add(new EventMessage("", exception.Message, EventMessageType.Error));
                return Core.Attempt.Fail(new OperationStatus(OperationStatusType.FailedExceptionThrown, eventMessages), exception);
            }

            public static Attempt<OperationStatus<OperationStatusType, TValue>> Fail<TValue>(EventMessages eventMessages, Exception exception)
            {
                return Core.Attempt.Fail(new OperationStatus<OperationStatusType, TValue>(OperationStatusType.FailedExceptionThrown, eventMessages), exception);
            }

            public static Attempt<OperationStatus<TStatusType>> Fail<TStatusType>(TStatusType statusType, EventMessages eventMessages)
                where TStatusType : struct
            {
                return Core.Attempt.Fail(new OperationStatus<TStatusType>(statusType, eventMessages));
            }

            public static Attempt<OperationStatus<TStatusType>> Fail<TStatusType>(TStatusType statusType, EventMessages eventMessages, Exception exception)
                where TStatusType : struct
            {
                return Core.Attempt.Fail(new OperationStatus<TStatusType>(statusType, eventMessages), exception);
            }

            public static Attempt<OperationStatus<TStatusType, TValue>> Fail<TStatusType, TValue>(TStatusType statusType, EventMessages eventMessages)
                where TStatusType : struct
            {
                return Core.Attempt.Fail(new OperationStatus<TStatusType, TValue>(statusType, eventMessages));
            }

            public static Attempt<OperationStatus<TStatusType, TValue>> Fail<TStatusType, TValue>(TStatusType statusType, EventMessages eventMessages, TValue value)
                where TStatusType : struct
            {
                return Core.Attempt.Fail(new OperationStatus<TStatusType, TValue>(statusType, eventMessages, value));
            }

            public static Attempt<OperationStatus<TStatusType, TValue>> Fail<TStatusType, TValue>(TStatusType statusType, EventMessages eventMessages, Exception exception)
                where TStatusType : struct
            {
                return Core.Attempt.Fail(new OperationStatus<TStatusType, TValue>(statusType, eventMessages), exception);
            }

            public static Attempt<OperationStatus<TStatusType, TValue>> Fail<TStatusType, TValue>(TStatusType statusType, EventMessages eventMessages, TValue value, Exception exception)
                where TStatusType : struct
            {
                return Core.Attempt.Fail(new OperationStatus<TStatusType, TValue>(statusType, eventMessages, value), exception);
            }

            public static Attempt<OperationStatus<OperationStatusType, TValue>> Cannot<TValue>(EventMessages eventMessages)
            {
                return Core.Attempt.Fail(new OperationStatus<OperationStatusType, TValue>(OperationStatusType.FailedCannot, eventMessages));
            }
        }
    }
}