using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Core.Services.Implement
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

        protected void OnChanged(IScope scope, ContentTypeChange<TItem>.EventArgs args)
        {
            scope.Events.Dispatch(Changed, This, args, nameof(Changed));
        }

        /// <summary>
        /// Raises the <see cref="ScopedRefreshedEntity"/> event during the <see cref="IScope"/> (transaction)
        /// </summary>
        protected void OnUowRefreshedEntity(ContentTypeChange<TItem>.EventArgs args)
        {
            ScopedRefreshedEntity.RaiseEvent(args, This);
        }

        protected bool OnSavingCancelled(IScope scope, SaveEventArgs<TItem> args)
        {
            return scope.Events.DispatchCancelable(Saving, This, args);
        }

        protected void OnSaved(IScope scope, SaveEventArgs<TItem> args)
        {
            scope.Events.Dispatch(Saved, This, args);
        }

        protected bool OnDeletingCancelled(IScope scope, DeleteEventArgs<TItem> args)
        {
            return scope.Events.DispatchCancelable(Deleting, This, args, nameof(Deleting));
        }

        protected void OnDeleted(IScope scope, DeleteEventArgs<TItem> args)
        {
            scope.Events.Dispatch(Deleted, This, args);
        }

        protected bool OnMovingCancelled(IScope scope, MoveEventArgs<TItem> args)
        {
            return scope.Events.DispatchCancelable(Moving, This, args);
        }

        protected void OnMoved(IScope scope, MoveEventArgs<TItem> args)
        {
            scope.Events.Dispatch(Moved, This, args);
        }

        protected bool OnSavingContainerCancelled(IScope scope, SaveEventArgs<EntityContainer> args)
        {
            return scope.Events.DispatchCancelable(SavingContainer, This, args, nameof(SavingContainer));
        }

        protected void OnSavedContainer(IScope scope, SaveEventArgs<EntityContainer> args)
        {
            scope.Events.Dispatch(SavedContainer, This, args);
        }

        protected bool OnRenamingContainerCancelled(IScope scope, SaveEventArgs<EntityContainer> args)
        {
            return scope.Events.DispatchCancelable(SavedContainer, This, args, nameof(SavedContainer));
        }

        protected void OnRenamedContainer(IScope scope, SaveEventArgs<EntityContainer> args)
        {
            scope.Events.Dispatch(SavedContainer, This, args, nameof(SavedContainer));
        }

        protected bool OnDeletingContainerCancelled(IScope scope, DeleteEventArgs<EntityContainer> args)
        {
            return scope.Events.DispatchCancelable(DeletingContainer, This, args);
        }

        protected void OnDeletedContainer(IScope scope, DeleteEventArgs<EntityContainer> args)
        {
            scope.Events.Dispatch(DeletedContainer, This, args, nameof(DeletedContainer));
        }
    }
}
