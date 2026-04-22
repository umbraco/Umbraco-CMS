using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Per-DbContext configuration for EF Core scoping behavior.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal class EFCoreScopeConfiguration<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets or sets a value indicating whether the EF Core scope should share
    /// the NPoco (Umbraco main database) connection and transaction.
    /// When <c>false</c>, the DbContext uses its own configured connection
    /// and manages its own transaction independently.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ShareUmbracoConnection { get; set; } = true;
}
