using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class SurfaceControllerTypeCollection : BuilderCollectionBase<Type>
    {
        public SurfaceControllerTypeCollection(IEnumerable<Type> items)
            : base(items)
        { }
    }
}
