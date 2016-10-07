using LightInject;
using Umbraco.Core.DI;

namespace Umbraco.Core.Persistence.Mappers
{
    public class MapperCollectionBuilder : LazyCollectionBuilderBase<MapperCollectionBuilder, MapperCollection, BaseMapper>
    {
        public MapperCollectionBuilder(IServiceContainer container) 
            : base(container)
        { }

        protected override MapperCollectionBuilder This => this;

        protected override void Initialize()
        {
            base.Initialize();

            // default initializer registers
            // - service MapperCollectionBuilder, returns MapperCollectionBuilder
            // - service MapperCollection, returns MapperCollectionBuilder's collection
            // we want to register extra
            // - service IMapperCollection, returns MappersCollectionBuilder's collection

            Container.Register<IMapperCollection>(factory => factory.GetInstance<MapperCollection>());
        }
    }
}
