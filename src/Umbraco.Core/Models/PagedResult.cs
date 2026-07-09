using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a paged result for a model collection
/// </summary>
[Obsolete ("Superseded by PagedModel for service layer and below OR PagedViewModel in apis. Expected to be removed when skip/take pattern has been fully implemented v14+")]
[DataContract(Name = "pagedCollection", Namespace = "")]
public abstract class PagedResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedResult" /> class.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PagedResult(long totalItems, long pageNumber, long pageSize)
    {
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;

        if (pageSize > 0)
        {
            TotalPages = (long)Math.Ceiling(totalItems / (decimal)pageSize);
        }
        else
        {
            TotalPages = 1;
        }
    }

    /// <summary>
    ///     Gets the current page number (1-based).
    /// </summary>
    [DataMember(Name = "pageNumber")]
    public long PageNumber { get; private set; }

    /// <summary>
    ///     Gets the number of items per page.
    /// </summary>
    [DataMember(Name = "pageSize")]
    public long PageSize { get; private set; }

    /// <summary>
    ///     Gets the total number of pages.
    /// </summary>
    [DataMember(Name = "totalPages")]
    public long TotalPages { get; private set; }

    /// <summary>
    ///     Gets the total number of items across all pages.
    /// </summary>
    [DataMember(Name = "totalItems")]
    public long TotalItems { get; private set; }

    /// <summary>
    ///     Calculates the skip size based on the paged parameters specified
    /// </summary>
    /// <remarks>
    ///     Returns 0 if the page number or page size is zero
    /// </remarks>
    public int GetSkipSize()
    {
        if (PageNumber > 0 && PageSize > 0)
        {
            return Convert.ToInt32((PageNumber - 1) * PageSize);
        }

        return 0;
    }
}
