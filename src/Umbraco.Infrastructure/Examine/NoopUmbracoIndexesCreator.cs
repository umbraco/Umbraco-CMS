using System.Collections.Generic;
using System.Linq;
using Examine;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class NoopUmbracoIndexesCreator : IUmbracoIndexesCreator
    {
        public IEnumerable<IIndex> Create()
        {
            return Enumerable.Empty<IIndex>();
        }
    }
}
