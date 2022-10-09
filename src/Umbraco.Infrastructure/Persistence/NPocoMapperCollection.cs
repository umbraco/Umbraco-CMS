using NPoco;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence;

public sealed class NPocoMapperCollection : BuilderCollectionBase<IMapper>
{
    public NPocoMapperCollection(Func<IEnumerable<IMapper>> items)
        : base(items)
    {
    }
}
