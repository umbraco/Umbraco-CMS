using System.Collections.Generic;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// An interface describing that the object should be aware of it's containing collection
    /// </summary>
    internal interface IOwnerCollectionAware<T>
    {
        IEnumerable<T> OwnersCollection { get; set; }
    }
}