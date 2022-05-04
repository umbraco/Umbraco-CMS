using System.Data;
using StackExchange.Profiling.Data;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class DbCommandExtensions
{
    /// <summary>
    ///     Unwraps a database command.
    /// </summary>
    /// <remarks>
    ///     UmbracoDatabase wraps the original database connection in various layers (see
    ///     OnConnectionOpened); this unwraps and returns the original database command.
    /// </remarks>
    public static IDbCommand UnwrapUmbraco(this IDbCommand command)
    {
        IDbCommand unwrapped;

        IDbCommand c = command;
        do
        {
            unwrapped = c;

            var profiled = unwrapped as ProfiledDbCommand;
            if (profiled != null)
            {
                unwrapped = profiled.InternalCommand;
            }
        } while (c != unwrapped);

        return unwrapped;
    }
}
