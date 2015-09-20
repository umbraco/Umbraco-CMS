using System;

namespace Umbraco.Core.Models.PublishedContent
{
    public class PublishedContentWithKeyExtended : PublishedContentExtended, IPublishedContentWithKey
    {
        // protected for models, internal for PublishedContentExtended static Extend method
        protected internal PublishedContentWithKeyExtended(IPublishedContentWithKey content)
            : base(content)
        { }

        public Guid Key { get { return ((IPublishedContentWithKey) Content).Key; } }
    }
}
