using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceCollection : BuilderCollectionBase<IDataValueReference>
    {
        public DataValueReferenceCollection(IEnumerable<IDataValueReference> items)
            : base(items)
        { }
    }
}
