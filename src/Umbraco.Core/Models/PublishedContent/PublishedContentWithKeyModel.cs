using System;

namespace Umbraco.Core.Models.PublishedContent
{
    public abstract class PublishedContentWithKeyModel : PublishedContentModel, IPublishedContentWithKey
    {
        protected PublishedContentWithKeyModel(IPublishedContentWithKey content)
            : base (content)
        { }

        public Guid Key { get { return ((IPublishedContentWithKey) Content).Key; } }
    }
}
