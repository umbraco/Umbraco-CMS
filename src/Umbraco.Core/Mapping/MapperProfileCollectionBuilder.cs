using Umbraco.Core.Composing;

namespace Umbraco.Core.Mapping
{
    public class MapperProfileCollectionBuilder : SetCollectionBuilderBase<MapperProfileCollectionBuilder, MapperProfileCollection, IMapperProfile>
    {
        protected override MapperProfileCollectionBuilder This => this;

        protected override Lifetime CollectionLifetime => Lifetime.Transient;
    }
}
