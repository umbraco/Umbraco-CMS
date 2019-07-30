using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.WebApi
{
    public class UmbracoApiControllerTypeCollection : BuilderCollectionBase<Type>
    {
        public UmbracoApiControllerTypeCollection(IEnumerable<Type> items)
            : base(items)
        { }
    }
}
