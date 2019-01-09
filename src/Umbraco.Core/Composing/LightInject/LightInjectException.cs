using System;
using System.Text;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Represents errors that occur due to LightInject.
    /// </summary>
    public class LightInjectException : Exception
    {
        public LightInjectException(string message)
            : base(message)
        { }

        public LightInjectException(string message, Exception innerException)
            : base(message, innerException)
        { }

        private const string LightInjectUnableToResolveType = "Unable to resolve type:";
        private const string LightInjectUnresolvedDependency = "Unresolved dependency ";
        private const string LightInjectRequestedDependency = "[Requested dependency:";

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
