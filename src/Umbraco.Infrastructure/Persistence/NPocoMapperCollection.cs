using NPoco;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Represents a collection of NPoco mappers for mapping database records to objects.
/// </summary>
public sealed class NPocoMapperCollection : BuilderCollectionBase<IMapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NPocoMapperCollection"/> class with the specified function to provide <see cref="IMapper"/> instances.
    /// </summary>
    /// <param name="items">A function that returns an <see cref="IEnumerable{IMapper}"/> representing the mappers to include in the collection.</param>
    public NPocoMapperCollection(Func<IEnumerable<IMapper>> items)
        : base(items)
    {
    }
}
