using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Services.Implement
{
    public abstract class ContentTypeServiceBase<TItem, TService> : ContentTypeServiceBase
        where TItem : class, IContentTypeComposition
        where TService : class, IContentTypeBaseService<TItem>
    {
        protected ContentTypeServiceBase(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
            : base(provider, loggerFactory, eventMessagesFactory)
        { }

        protected abstract TService This { get; }

        /// <summary>
        /// Raised when a <see cref="TItem"/> is changed
        /// </summary>
        /// <remarks>
        /// This event is dispatched after the trans is completed. Used by event refreshers.
        /// </remarks>
        public static event TypedEventHandler<TService, ContentTypeChange<TItem>.EventArgs> Changed;

        /// <summary>
        /// Occurs when an <see cref="TItem"/> is created or updated from within the <see cref="IScope"/> (transaction)
        /// </summary>
        /// <remarks>
        /// The purpose of this event being raised within the transaction is so that listeners can perform database
        /// operations from within the same transaction and guarantee data consistency so that if anything goes wrong
        /// the entire transaction can be rolled back. This is used by Nucache.
        /// TODO: See remarks in ContentRepositoryBase about these types of events.
        /// </remarks>
        public static event TypedEventHandler<TService, ContentTypeChange<TItem>.EventArgs> ScopedRefreshedEntity;

        // used by tests to clear events
        internal static void ClearScopeEvents()
        {
            ScopedRefreshedEntity = null;
        }

        // these must be dispatched
        public static event TypedEventHandler<TService, SaveEventArgs<TItem>> Saving;
        public static event TypedEventHandler<TService, SaveEventArgs<TItem>> Saved;
        public static event TypedEventHandler<TService, DeleteEventArgs<TItem>> Deleting;
        public static event TypedEventHandler<TService, DeleteEventArgs<TItem>> Deleted;
        public static event TypedEventHandler<TService, MoveEventArgs<TItem>> Moving;
        public static event TypedEventHandler<TService, MoveEventArgs<TItem>> Moved;
        public static event TypedEventHandler<TService, SaveEventArgs<EntityContainer>> SavingContainer;
        public static event TypedEventHandler<TService, SaveEventArgs<EntityContainer>> SavedContainer;
        public static event TypedEventHandler<TService, DeleteEventArgs<EntityContainer>> DeletingContainer;
        public static event TypedEventHandler<TService, DeleteEventArgs<EntityContainer>> DeletedContainer;

    }
}
