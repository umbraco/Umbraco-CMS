using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    public class NoopPublishedContentModelFactory : IPublishedContentModelFactory
    {
        public IPropertySet CreateModel(IPropertySet set)
            => set;

        public Dictionary<string, Type> ModelTypeMap { get; } = new Dictionary<string, Type>();
    }
}
