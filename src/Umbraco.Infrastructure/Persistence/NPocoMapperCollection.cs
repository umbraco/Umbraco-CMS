using System.Collections.Generic;
using NPoco;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public sealed class NPocoMapperCollection : BuilderCollectionBase<IMapper>
    {
        public NPocoMapperCollection(IEnumerable<IMapper> items) : base(items)
        {
        }
    }
}
