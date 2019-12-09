using System;
using System.Runtime.Serialization;
using System.Text;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Represents errors that occur due to LightInject.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class LightInjectException : Exception
    {
        private const string LightInjectUnableToResolveType = "Unable to resolve type:";
        private const string LightInjectUnresolvedDependency = "Unresolved dependency ";
        private const string LightInjectRequestedDependency = "[Requested dependency:";

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectException" /> class.
        /// </summary>
        public LightInjectException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LightInjectException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public LightInjectException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected LightInjectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Tries to throw the exception with additional details.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <exception cref="Umbraco.Core.Composing.LightInject.LightInjectException"></exception>
        public static void TryThrow(Exception e)
        {
            var ex = e as InvalidOperationException;
            if (ex == null || ex.Message.StartsWith(LightInjectUnableToResolveType) == false)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("Unresolved type: " + ex.Message.Substring(LightInjectUnableToResolveType.Length));
            WriteDetails(ex, sb);
            throw new LightInjectException(sb.ToString(), e);
        }

        /// <summary>
        /// Tries to throw the exception with additional details.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <exception cref="Umbraco.Core.Composing.LightInject.LightInjectException"></exception>
        public static void TryThrow(Exception e, Type implementingType)
        {
            var ex = e as InvalidOperationException;
            if (ex == null || ex.Message.StartsWith(LightInjectUnableToResolveType) == false)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("Unresolved type: " + ex.Message.Substring(LightInjectUnableToResolveType.Length));
            sb.AppendLine("Implementing type: " + implementingType);
            WriteDetails(ex, sb);
            throw new LightInjectException(sb.ToString(), e);
        }

        /// <summary>
        /// Writes the details.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="sb">The <see cref="StringBuilder" /> to write the details to.</param>
        private static void WriteDetails(InvalidOperationException ex, StringBuilder sb)
        {
            ex = ex.InnerException as InvalidOperationException;
            while (ex != null)
            {
                var message = ex.Message;

                if (message.StartsWith(LightInjectUnableToResolveType))
                {
                    sb.AppendLine("-> Unresolved type: " + message.Substring(LightInjectUnableToResolveType.Length));
                }
                else if (message.StartsWith(LightInjectUnresolvedDependency))
                {
                    var pos = message.InvariantIndexOf(LightInjectRequestedDependency);
                    sb.AppendLine("-> Unresolved dependency: " + message.Substring(pos + LightInjectRequestedDependency.Length + 1).TrimEnd(']'));
                }

                ex = ex.InnerException as InvalidOperationException;
            }
        }
    }
}
