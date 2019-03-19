using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Mapping
{
    public class MapperProfileCollection : BuilderCollectionBase<IMapperProfile>
    {
        public MapperProfileCollection(IEnumerable<IMapperProfile> items)
            : base(items)
        { }
    }
}
