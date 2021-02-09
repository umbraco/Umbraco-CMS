using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Web.Website.Collections
{
    public class SurfaceControllerTypeCollection : BuilderCollectionBase<Type>
    {
        public SurfaceControllerTypeCollection(IEnumerable<Type> items)
            : base(items)
        { }
    }
}
