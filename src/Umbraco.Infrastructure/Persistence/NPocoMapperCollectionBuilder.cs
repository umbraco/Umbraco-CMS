using NPoco;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence;

public sealed class NPocoMapperCollectionBuilder : SetCollectionBuilderBase<NPocoMapperCollectionBuilder, NPocoMapperCollection, IMapper>
{
    protected override NPocoMapperCollectionBuilder This => this;
}
