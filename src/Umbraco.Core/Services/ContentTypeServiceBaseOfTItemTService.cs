using System.Runtime.CompilerServices;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Core.Services
{
    internal abstract class ContentTypeServiceBase<TItem, TService> : ContentTypeServiceBase
        where TItem : class, IContentTypeComposition
        where TService : class, IContentTypeServiceBase<TItem>
    {
        protected ContentTypeServiceBase(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }

        protected abstract TService This { get; }

        // that one must be dispatched
        internal static event TypedEventHandler<TService, ContentTypeChange<TItem>.EventArgs> Changed;

        // that one is always immediate (transactional)
        public static event TypedEventHandler<TService, ContentTypeChange<TItem>.EventArgs> UowRefreshedEntity;

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

        // fixme - can we have issues with event names?

        protected void OnChanged(IScopeUnitOfWork uow, ContentTypeChange<TItem>.EventArgs args)
        {
            uow.Events.Dispatch(Changed, This, args, "Changed");
        }

        protected void OnUowRefreshedEntity(ContentTypeChange<TItem>.EventArgs args)
        {
            // that one is always immediate (not dispatched, transactional)
            UowRefreshedEntity.RaiseEvent(args, This);
        }

        // fixme what is thsi?
        protected void OnSaving(IScopeUnitOfWork uow,  SaveEventArgs<TItem> args)
        {
            Saving.RaiseEvent(args, This);
        }

        protected bool OnSavingCancelled(IScopeUnitOfWork uow, SaveEventArgs<TItem> args)
        {
            return uow.Events.DispatchCancelable(Saving, This, args);
        }

        protected void OnSaved(IScopeUnitOfWork uow, SaveEventArgs<TItem> args)
        {
            uow.Events.Dispatch(Saved, This, args);
        }

        // fixme what is thsi?
        protected void OnDeleting(IScopeUnitOfWork uow,  DeleteEventArgs<TItem> args)
        {
            Deleting.RaiseEvent(args, This);
        }

        protected bool OnDeletingCancelled(IScopeUnitOfWork uow, DeleteEventArgs<TItem> args)
        {
            return uow.Events.DispatchCancelable(Deleting, This, args);
        }

        protected void OnDeleted(IScopeUnitOfWork uow, DeleteEventArgs<TItem> args)
        {
            uow.Events.Dispatch(Deleted, This, args);
        }

        // fixme what is thsi?
        protected void OnMoving(IScopeUnitOfWork uow,  MoveEventArgs<TItem> args)
        {
            Moving.RaiseEvent(args, This);
        }

        protected bool OnMovingCancelled(IScopeUnitOfWork uow, MoveEventArgs<TItem> args)
        {
            return uow.Events.DispatchCancelable(Moving, This, args);
        }

        protected void OnMoved(IScopeUnitOfWork uow, MoveEventArgs<TItem> args)
        {
            uow.Events.Dispatch(Moved, This, args);
        }

        // fixme what is this?
        protected void OnSavingContainer(IScopeUnitOfWork uow,  SaveEventArgs<EntityContainer> args)
        {
            SavingContainer.RaiseEvent(args, This);
        }

        protected bool OnSavingContainerCancelled(IScopeUnitOfWork uow, SaveEventArgs<EntityContainer> args)
        {
            return uow.Events.DispatchCancelable(SavingContainer, This, args);
        }

        protected void OnSavedContainer(IScopeUnitOfWork uow, SaveEventArgs<EntityContainer> args)
        {
            uow.Events.DispatchCancelable(SavedContainer, This, args);
        }

        // fixme what is this?
        protected void OnDeletingContainer(IScopeUnitOfWork uow,  DeleteEventArgs<EntityContainer> args)
        {
            DeletingContainer.RaiseEvent(args, This);
        }

        protected bool OnDeletingContainerCancelled(IScopeUnitOfWork uow, DeleteEventArgs<EntityContainer> args)
        {
            return uow.Events.DispatchCancelable(DeletingContainer, This, args);
        }

        protected void OnDeletedContainer(IScopeUnitOfWork uow, DeleteEventArgs<EntityContainer> args)
        {
            uow.Events.Dispatch(DeletedContainer, This, args);
        }
    }
}