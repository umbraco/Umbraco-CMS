using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Examine;

namespace Umbraco.Infrastructure.Examine
{
    public class NoopUmbracoIndexesCreator : IUmbracoIndexesCreator
    {
        public IEnumerable<IIndex> Create()
        {
            return Enumerable.Empty<IIndex>();
        }
    }
}
