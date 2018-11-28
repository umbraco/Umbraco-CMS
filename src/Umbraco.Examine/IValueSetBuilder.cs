using Examine;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Examine
{
    /// <summary>
    /// Creates a collection of <see cref="ValueSet"/> to be indexed based on a collection of <see cref="TContent"/>
    /// </summary>
    /// <typeparam name="TContent"></typeparam>
    public interface IValueSetBuilder<in TContent>
        where TContent : IContentBase
    {
        /// <summary>
        /// Creates a collection of <see cref="ValueSet"/> to be indexed based on a collection of <see cref="TContent"/>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        IEnumerable<ValueSet> GetValueSets(params TContent[] content);
    }

}
