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
    public PagedResult(long totalItems, long pageNumber, long pageSize)
        : base(totalItems, pageNumber, pageSize)
    {
    }

    [DataMember(Name = "items")]
    public IEnumerable<T>? Items { get; set; }
}
