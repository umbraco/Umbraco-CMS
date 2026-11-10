// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

/// <summary>
/// Asserts how repositories order rows in the statements they execute.
/// </summary>
internal static class OrderingAssert
{
    /// <summary>
    /// Runs an action and asserts that each statement it executes that orders by node sort order ends its ordering
    /// with the node id, so that nodes with equal sort orders are returned in the same order on every database.
    /// </summary>
    public static void BreaksSortOrderTiesById(IScopeAccessor scopeAccessor, Action action)
    {
        ISqlSyntaxProvider sqlSyntax = scopeAccessor.AmbientScope!.SqlContext.SqlSyntax;
        var sortOrder = sqlSyntax.GetFieldName<NodeDto>(x => x.SortOrder);

        string[] orderings = GetOrderings(scopeAccessor, action, pagedOnly: false)
            .Where(ordering => ordering.Contains(sortOrder))
            .ToArray();

        Assert.That(orderings, Is.Not.Empty, "No statement ordered by node sort order.");
        Assert.That(orderings.Select(GetLastTerm), Has.All.EqualTo(sqlSyntax.GetFieldName<NodeDto>(x => x.NodeId)), string.Join(Environment.NewLine, orderings));
    }

    /// <summary>
    /// Runs an action and asserts that each page it reads is ordered by a unique column last, so that rows with
    /// equal values for the requested ordering are neither skipped nor repeated from one page to the next.
    /// </summary>
    public static void OrdersPagesLastBy(IScopeAccessor scopeAccessor, string uniqueColumn, Action action)
    {
        string[] orderings = GetOrderings(scopeAccessor, action, pagedOnly: true);

        Assert.That(orderings, Is.Not.Empty, "No page was read with an ordering.");
        Assert.That(orderings.Select(GetLastTerm), Has.All.EqualTo(uniqueColumn), string.Join(Environment.NewLine, orderings));
    }

    private static string[] GetOrderings(IScopeAccessor scopeAccessor, Action action, bool pagedOnly)
    {
        UmbracoDatabase database = scopeAccessor.AmbientScope!.Database.AsUmbracoDatabase();
        database.LogCommands = true;
        try
        {
            action();
            return database.Commands!
                .Select(command => command.Text)
                .Where(sql => pagedOnly is false || sql.Contains("OFFSET", StringComparison.OrdinalIgnoreCase))
                .Select(GetOrderByClause)
                .OfType<string>()
                .ToArray();
        }
        finally
        {
            database.LogCommands = false;
        }
    }

    private static string? GetOrderByClause(string sql)
    {
        const string orderBy = "ORDER BY";
        var index = sql.LastIndexOf(orderBy, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var clause = sql[(index + orderBy.Length)..];
        var end = new[] { "LIMIT", "OFFSET" }
            .Select(keyword => clause.IndexOf(keyword, StringComparison.OrdinalIgnoreCase))
            .Where(position => position >= 0)
            .DefaultIfEmpty(clause.Length)
            .Min();
        return clause[..end];
    }

    private static string GetLastTerm(string ordering)
    {
        var term = ordering.Split(',')[^1].Replace("(", string.Empty).Replace(")", string.Empty).Trim();
        foreach (var direction in new[] { " ASC", " DESC" })
        {
            if (term.EndsWith(direction, StringComparison.OrdinalIgnoreCase))
            {
                return term[..^direction.Length].TrimEnd();
            }
        }

        return term;
    }
}
