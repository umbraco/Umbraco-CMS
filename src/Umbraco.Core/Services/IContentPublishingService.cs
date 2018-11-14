using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IContentPublishingService : IService
    {
        /// <summary>
        /// Registers a culture to be published.
        /// </summary>
        /// <returns>A value indicating whether the culture can be published.</returns>
        /// <remarks>
        /// <para>Fails if properties don't pass variant validtion rules.</para>
        /// <para>Publishing must be finalized via the content service SavePublishing method.</para>
        /// </remarks>
        bool PublishCulture(IContent content, string culture = "*");

        /// <summary>
        /// Registers a culture to be unpublished.
        /// </summary>
        /// <remarks>
        /// <para>Unpublishing must be finalized via the content service SavePublishing method.</para>
        /// </remarks>
        void UnpublishCulture(IContent content, string culture = "*");
    }
}
