using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the result of unpublishing a document.
    /// </summary>
    public class UnpublishResult : OperationResult<UnpublishResultType, IContent>
    {
        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="eventMessages"></param>
        /// <param name="entity"></param>
        public UnpublishResult(EventMessages eventMessages, IContent entity) : base(UnpublishResultType.Success, eventMessages, entity)
        {
        }

        public UnpublishResult(UnpublishResultType result, EventMessages eventMessages, IContent entity) : base(result, eventMessages, entity)
        {
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public IContent Content => Entity;
    }
}
