// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Counts the commands a <see cref="Microsoft.EntityFrameworkCore.DbContext" /> sends to the database, so a test can
///     assert on the cost of an operation rather than only its result.
/// </summary>
/// <remarks>
///     <para>
///         Assert on a difference between two counts, never on an absolute number. A single repository read fans out
///         across property data, content types, variations and templates, and any collaborator resolved from the
///         container brings its own queries, so the absolute count is an implementation detail that changes for reasons
///         unrelated to the behaviour under test. Comparing two shapes of the same operation isolates the one thing that
///         differs between them.
///     </para>
///     <para>
///         Counting is off until <see cref="Enabled" /> is set, so that arrange steps do not contribute to the total.
///     </para>
/// </remarks>
internal sealed class CountingDbCommandInterceptor : DbCommandInterceptor
{
    private readonly List<string> _commands = [];
    private readonly Lock _lock = new();

    /// <summary>
    ///     Gets or sets a value indicating whether commands are being counted.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets the number of commands counted since the last <see cref="Reset" />.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _commands.Count;
            }
        }
    }

    /// <summary>
    ///     Gets the text of the commands counted since the last <see cref="Reset" />, for assertion failure messages.
    /// </summary>
    public IReadOnlyList<string> Commands
    {
        get
        {
            lock (_lock)
            {
                return _commands.ToArray();
            }
        }
    }

    /// <summary>
    ///     Discards everything counted so far, leaving <see cref="Enabled" /> untouched.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _commands.Clear();
        }
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Record(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Record(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Record(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void Record(DbCommand command)
    {
        if (Enabled is false)
        {
            return;
        }

        lock (_lock)
        {
            _commands.Add(command.CommandText);
        }
    }
}
