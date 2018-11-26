using Examine;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Examine
{
    public interface IValueSetBuilder<TContent>
        where TContent : IContentBase
    {
        IEnumerable<ValueSet> GetValueSets(params TContent[] content);
    }

}
