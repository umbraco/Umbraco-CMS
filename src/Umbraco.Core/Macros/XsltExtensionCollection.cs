using System.Collections.Generic;
using Umbraco.Core.DI;

namespace Umbraco.Core.Macros
{
    internal class XsltExtensionCollection : BuilderCollectionBase<XsltExtension>
    {
        public XsltExtensionCollection(IEnumerable<XsltExtension> items) 
            : base(items)
        { }
    }
}
