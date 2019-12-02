using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceForCollection : BuilderCollectionBase<IDataValueReferenceFor>
    {
        public DataValueReferenceForCollection(IEnumerable<IDataValueReferenceFor> items)
            : base(items)
        { }
    }
}
