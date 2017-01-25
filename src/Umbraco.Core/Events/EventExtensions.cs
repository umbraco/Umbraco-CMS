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
        /// <summary>
        /// Raises the event and returns a boolean value indicating if the event was cancelled
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static bool IsRaisedEventCancelled<TSender, TArgs>(
			this TypedEventHandler<TSender, TArgs> eventHandler,
			TArgs args,
			TSender sender)
			where TArgs : CancellableEventArgs
		{
			if (eventHandler != null)
				eventHandler(sender, args);

			return args.Cancel;
		}

        /// <summary>
        /// Raises the event with the event manager and returns a boolean value indicating if the event was cancelled
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        /// <param name="eventManager"></param>
        /// <param name="eventName">Optional explicit event name if the event may not be able to be determined based on it's arguments (it's ambiguous)</param>
        /// <returns></returns>
        internal static bool IsRaisedEventCancelled<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            TArgs args,
            TSender sender,
            IEventManager eventManager,
            string eventName = null)
            where TArgs : CancellableEventArgs
        {
            //if the manager supports cancelation then just raise the event,
            //cancelable events cannot be queued
            if (eventManager.SupportsEventCancellation)
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
            using(var uow = uowProvider.GetReadOnlyUnitOfWork())
            {
                //if the manager supports cancelation then just raise the event,
                //cancelable events cannot be queued
                if (uow.EventManager.SupportsEventCancellation)
                {
                    if (eventHandler != null)
                        eventHandler(sender, args);
                    
                    return args.Cancel;
                }
                return false;
            }

        }

        /// <summary>
        /// Raises the event
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        public static void RaiseEvent<TSender, TArgs>(
			this TypedEventHandler<TSender, TArgs> eventHandler,
			TArgs args,
			TSender sender)
			where TArgs : EventArgs
		{
			if (eventHandler != null)
				eventHandler(sender, args);
		}

        /// <summary>
        /// Raises/tracks an event with the event manager
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="eventHandler"></param>
        /// <param name="eventManager"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        /// <param name="eventName">Optional explicit event name if the event may not be able to be determined based on it's arguments (it's ambiguous)</param>
        internal static void RaiseEvent<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,            
            TArgs args,
            TSender sender,
            IEventManager eventManager,
            string eventName = null)
            where TArgs : EventArgs
        {
            eventManager.QueueEvent(eventHandler, sender, args, eventName);
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
                uow.EventManager.QueueEvent(eventHandler, sender, args, eventName);
                uow.Commit();
            }
        }

        // moves the last handler that was added to an instance event, to first position
        public static void PromoteLastHandler(object sender, string eventName)
	    {
            var fieldInfo = sender.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null) throw new InvalidOperationException("No event named " + eventName + ".");
            PromoteLastHandler(sender, fieldInfo);
        }

        // moves the last handler that was added to a static event, to first position
        public static void PromoteLastHandler<TSender>(string eventName)
        {
            var fieldInfo = typeof(TSender).GetField(eventName, BindingFlags.Static | BindingFlags.NonPublic);
            if (fieldInfo == null) throw new InvalidOperationException("No event named " + eventName + ".");
            PromoteLastHandler(null, fieldInfo);
        }

	    private static void PromoteLastHandler(object sender, FieldInfo fieldInfo)
	    {
            var d = fieldInfo.GetValue(sender) as Delegate;
	        if (d == null) return;

            var l = d.GetInvocationList();
            var x = l[l.Length - 1];
            for (var i = l.Length - 1; i > 0; i--)
                l[i] = l[i - 1];
            l[0] = x;

            fieldInfo.SetValue(sender, Delegate.Combine(l));
        }
    }
}