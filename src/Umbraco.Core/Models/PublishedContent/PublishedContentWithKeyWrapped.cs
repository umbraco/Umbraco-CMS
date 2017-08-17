using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides an abstract base class for <c>IPublishedContentWithKey</c> implementations that
    /// wrap and extend another <c>IPublishedContentWithKey</c>.
    /// </summary>
    public class PublishedContentWithKeyWrapped : PublishedContentWrapped, IPublishedContentWithKey
    {
        protected PublishedContentWithKeyWrapped(IPublishedContentWithKey content)
            : base(content)
        { }

        public virtual Guid Key { get { return ((IPublishedContentWithKey) Content).Key; } }
    }
}
