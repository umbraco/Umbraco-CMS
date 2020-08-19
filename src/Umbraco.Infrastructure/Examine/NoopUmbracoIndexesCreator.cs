using System.Collections.Generic;
using Examine;
using Umbraco.Examine;

namespace Umbraco.Infrastructure.Examine
{
    public class NoopUmbracoIndexesCreator : IUmbracoIndexesCreator
    {
        public IEnumerable<IIndex> Create()
        {
            return new IIndex[0];
        }
    }
}
