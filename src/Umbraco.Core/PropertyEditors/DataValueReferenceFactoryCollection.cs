using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceFactoryCollection : BuilderCollectionBase<IDataValueReferenceFactory>
    {
        public DataValueReferenceFactoryCollection(IEnumerable<IDataValueReferenceFactory> items)
            : base(items)
        { }
    }
}
