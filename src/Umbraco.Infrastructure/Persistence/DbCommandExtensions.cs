using System.Data;
using StackExchange.Profiling.Data;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class DbCommandExtensions
{
    /// <summary>
    ///     Unwraps a database command, returning the original command instance if it is wrapped.
    /// </summary>
    /// <param name="command">The potentially wrapped <see cref="IDbCommand"/> to unwrap.</param>
    /// <returns>The original, unwrapped <see cref="IDbCommand"/> instance.</returns>
    /// <remarks>
    ///     <see cref="UmbracoDatabase"/> may wrap the original database command in various layers (see
    ///     <c>OnConnectionOpened</c>); this method unwraps and returns the original database command.
    /// </remarks>
    public static IDbCommand UnwrapUmbraco(this IDbCommand command)
    {
        IDbCommand unwrapped;

        IDbCommand c = command;
        do
        {
            unwrapped = c;

            if (unwrapped is ProfiledDbCommand profiled)
            {
                unwrapped = profiled.InternalCommand;
            }
        }
        while (c != unwrapped);

        return unwrapped;
    }
}
