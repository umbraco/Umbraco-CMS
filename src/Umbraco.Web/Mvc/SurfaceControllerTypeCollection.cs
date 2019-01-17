using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    // unless we want to modify the content of the collection
    // which we are not doing at the moment
    // we can inherit from BuilderCollectionBase and just be enumerable

    //fixme: this should be LazyCollectionBuilderBase ?

    public class SurfaceControllerTypeCollection : BuilderCollectionBase<Type>
    {
        public SurfaceControllerTypeCollection(IEnumerable<Type> items)
            : base(items)
        { }
    }
}
