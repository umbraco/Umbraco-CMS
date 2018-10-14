using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Core.Services.Implement
{
    internal abstract class ContentTypeServiceBase<TItem, TService> : ContentTypeServiceBase
        where TItem : class, IContentTypeComposition
        where TService : class, IContentTypeServiceBase<TItem>
    {
        protected ContentTypeServiceBase(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }

        protected abstract TService This { get; }

        // that one must be dispatched
        internal static event TypedEventHandler<TService, ContentTypeChange<TItem>.EventArgs> Changed;

        // that one is always immediate (transactional)
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

        protected void OnUowRefreshedEntity(ContentTypeChange<TItem>.EventArgs args)
        {
            // that one is always immediate (not dispatched, transactional)
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
