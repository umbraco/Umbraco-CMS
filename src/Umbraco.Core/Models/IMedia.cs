using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    public interface IMedia : IContentBase
    {
        /// <summary>
        /// Changes the <see cref="IMediaType"/> for the current content object
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        void ChangeContentType(IMediaType contentType);

        /// <summary>
        /// Changes the <see cref="IMediaType"/> for the current content object and removes PropertyTypes,
        /// which are not part of the new ContentType.
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
        void ChangeContentType(IMediaType contentType, bool clearProperties);
    }
}
