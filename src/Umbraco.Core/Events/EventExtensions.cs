using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        internal static bool IsRaisedEventCancelled<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            TArgs args,
            TSender sender,
            IEventManager eventManager)
            where TArgs : CancellableEventArgs
        {
            if (eventManager.SupportsEventCancellation)
            {
                eventManager.TrackEvent(eventHandler, sender, args);
                return args.Cancel;
            }

            return false;
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
        internal static void RaiseEvent<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,            
            TArgs args,
            TSender sender,
            IEventManager eventManager)
            where TArgs : EventArgs
        {
            eventManager.TrackEvent(eventHandler, sender, args);
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