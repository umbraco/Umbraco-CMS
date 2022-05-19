using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a paged result for a model collection
/// </summary>
/// <typeparam name="T"></typeparam>
[DataContract(Name = "pagedCollection", Namespace = "")]
public class PagedResult<T> : PagedResult
{
    public PagedResult(long totalItems, long pageNumber, long pageSize)
        : base(totalItems, pageNumber, pageSize)
    {
    }

    [DataMember(Name = "items")]
    public IEnumerable<T>? Items { get; set; }
}
