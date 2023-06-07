using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a paged result for a model collection
/// </summary>
/// <typeparam name="T"></typeparam>
[DataContract(Name = "pagedCollection", Namespace = "")]
public abstract class PagedResult
{
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

    [DataMember(Name = "pageNumber")]
    public long PageNumber { get; private set; }

    [DataMember(Name = "pageSize")]
    public long PageSize { get; private set; }

    [DataMember(Name = "totalPages")]
    public long TotalPages { get; private set; }

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
