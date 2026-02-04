using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a paged result for a model collection
/// </summary>
/// <typeparam name="T"></typeparam>
[DataContract(Name = "pagedCollection", Namespace = "")]
[Obsolete ("Superseded by PagedModel for service layer and below OR PagedViewModel in apis. Expected to be removed when skip/take pattern has been fully implemented v14+")]
public class PagedResult<T> : PagedResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedResult{T}" /> class.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PagedResult(long totalItems, long pageNumber, long pageSize)
        : base(totalItems, pageNumber, pageSize)
    {
    }

    /// <summary>
    ///     Gets or sets the items for the current page.
    /// </summary>
    [DataMember(Name = "items")]
    public IEnumerable<T>? Items { get; set; }
}
