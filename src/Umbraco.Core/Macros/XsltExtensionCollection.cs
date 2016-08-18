using System.Collections.Generic;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Core.Macros
{
    internal class XsltExtensionCollection : BuilderCollectionBase<XsltExtension>
    {
        public XsltExtensionCollection(IEnumerable<XsltExtension> items) 
            : base(items)
        { }
    }
}
