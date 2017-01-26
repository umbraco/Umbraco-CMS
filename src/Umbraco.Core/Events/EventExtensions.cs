using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Events
{
	/// <summary>
	/// Extension methods for cancellable event operations
	/// </summary>
	public static class EventExtensions
	{
        // keep these two for backward compatibility reasons but understand that
        // they are *not* part of any scope / event dispatcher / anything...

        /// <summary>
        /// Raises a cancelable event and returns a value indicating whether the event should be cancelled.
        /// </summary>
        /// <typeparam name="TSender">The type of the event source.</typeparam>
        /// <typeparam name="TArgs">The type of the event data.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="args">The event source.</param>
        /// <param name="sender">The event data.</param>
        /// <returns>A value indicating whether the cancelable event should be cancelled</returns>
        /// <remarks>A cancelable event is raised by a component when it is about to perform an action that can be canceled.</remarks>
        public static bool IsRaisedEventCancelled<TSender, TArgs>(this TypedEventHandler<TSender, TArgs> eventHandler, TArgs args, TSender sender)
			where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
			eventHandler(sender, args);
			return args.Cancel;
		}

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <typeparam name="TSender">The type of the event source.</typeparam>
        /// <typeparam name="TArgs">The type of the event data.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="args">The event source.</param>
        /// <param name="sender">The event data.</param>
        public static void RaiseEvent<TSender, TArgs>(this TypedEventHandler<TSender, TArgs> eventHandler, TArgs args, TSender sender)
            where TArgs : EventArgs
        {
            if (eventHandler == null) return;
            eventHandler(sender, args);
        }

        // fixme - this makes entirely no sense whatsoever + is not documented
        // the "hack" thing should NOT be used, instead we should nest SCOPES ffs

        /// <summary>
        /// Raises the event with the event manager and returns a boolean value indicating if the event was cancelled
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        /// <param name="eventDispatcher"></param>
        /// <param name="eventName">Optional explicit event name if the event may not be able to be determined based on it's arguments (it's ambiguous)</param>
        /// <returns></returns>
        internal static bool IsRaisedEventCancelled<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            TArgs args,
            TSender sender,
            IEventDispatcher eventDispatcher,
            string eventName = null)
            where TArgs : CancellableEventArgs
        {
            //if the manager supports cancelation then just raise the event,
            //cancelable events cannot be queued
            if (eventDispatcher.SupportsEventCancellation)
            {
                if (eventHandler != null)
                    eventHandler(sender, args);

                return args.Cancel;
            }

            return false;
        }

        /// <summary>
        /// Hack: this is used to perform IsRaisedEventCancelled when a uow cannot be reused
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        /// <param name="uowProvider"></param>
        /// <param name="eventName">Optional explicit event name if the event may not be able to be determined based on it's arguments (it's ambiguous)</param>
        /// <returns></returns>
        internal static bool IsRaisedEventCancelled<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            TArgs args,
            TSender sender,
            IScopeUnitOfWorkProvider uowProvider,
            string eventName = null)
            where TArgs : CancellableEventArgs
        {
            using(var uow = uowProvider.GetUnitOfWork())
            {
                uow.Commit(); // readonly

                //if the manager supports cancelation then just raise the event,
                //cancelable events cannot be queued
                if (uow.Events.SupportsEventCancellation)
                {
                    if (eventHandler != null)
                        eventHandler(sender, args);

                    return args.Cancel;
                }
                return false;
            }

        }

        /// <summary>
        /// Raises/tracks an event with the event manager
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventDispatcher"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        /// <param name="eventName">Optional explicit event name if the event may not be able to be determined based on it's arguments (it's ambiguous)</param>
        internal static void RaiseEvent<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            TArgs args,
            TSender sender,
            IEventDispatcher eventDispatcher,
            string eventName = null)
            where TArgs : EventArgs
        {
            eventDispatcher.QueueEvent(eventHandler, sender, args, eventName);
        }

	    /// <summary>
	    /// Hack: this is used to perform IsRaisedEventCancelled when a uow cannot be reused
	    /// </summary>
	    /// <typeparam name="TSender"></typeparam>
	    /// <typeparam name="TArgs"></typeparam>
	    /// <param name="eventHandler"></param>
	    /// <param name="args"></param>
	    /// <param name="sender"></param>
	    /// <param name="uowProvider"></param>
	    /// <param name="eventName">Optional explicit event name if the event may not be able to be determined based on it's arguments (it's ambiguous)</param>
	    internal static void RaiseEvent<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            TArgs args,
            TSender sender,
            IScopeUnitOfWorkProvider uowProvider,
            string eventName = null)
            where TArgs : EventArgs
        {
            //This UOW is a readonly one but needs to be committed otherwise
            // it will rollback outer scopes
            using (var uow = uowProvider.GetUnitOfWork())
            {
                uow.Events.QueueEvent(eventHandler, sender, args, eventName);
                uow.Commit();
            }
        }
    }
}