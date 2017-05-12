using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Exceptions
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

            var messages = new List<string> { ex.Message };

            ex = ex.InnerException as InvalidOperationException;
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException as InvalidOperationException;
            }

            var sb = new StringBuilder();
            var last = messages.Last();
            if (last.StartsWith(LightInjectUnableToResolveType))
            {
                sb.AppendLine("Unresolved type: " + last.Substring(LightInjectUnableToResolveType.Length));
            }
            else if (last.StartsWith(LightInjectUnresolvedDependency))
            {
                var pos = last.InvariantIndexOf(LightInjectRequestedDependency);
                sb.AppendLine("Unresolved dependency: " + last.Substring(pos + LightInjectRequestedDependency.Length + 1).TrimEnd(']'));
                sb.AppendLine(" (see inner exceptions for the entire dependencies chain).");
            }
            else return; // wtf?

            throw new LightInjectException(sb.ToString(), e);
        }
    }
}
