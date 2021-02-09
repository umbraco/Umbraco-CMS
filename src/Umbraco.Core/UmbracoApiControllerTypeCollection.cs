using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core
{
    public class UmbracoApiControllerTypeCollection : BuilderCollectionBase<Type>
    {
        public UmbracoApiControllerTypeCollection(IEnumerable<Type> items)
            : base(items)
        { }
    }
}
