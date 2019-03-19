using Umbraco.Core.Composing;

namespace Umbraco.Core.Mapping
{
    public class MapperProfileCollectionBuilder : OrderedCollectionBuilderBase<MapperProfileCollectionBuilder, MapperProfileCollection, IMapperProfile>
    {
        protected override MapperProfileCollectionBuilder This => this;

        protected override Lifetime CollectionLifetime => Lifetime.Transient;
    }
}
