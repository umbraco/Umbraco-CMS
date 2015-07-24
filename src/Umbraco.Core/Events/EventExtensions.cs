using System;
using System.Linq;

namespace Umbraco.Core.Events
{
	/// <summary>
	/// Extension methods for cancellable event operations
	/// </summary>
	public static class EventExtensions
	{
	    /// <summary>
	    /// Raises the event and checks if a cancelation has occurred, if so it checks if any messages have been 
	    /// added to the event messages collection and if not it adds a default cancelation message
	    /// </summary>
	    /// <typeparam name="TSender"></typeparam>
	    /// <typeparam name="TArgs"></typeparam>
	    /// <param name="eventHandler"></param>
	    /// <param name="evtMsgs"></param>
	    /// <param name="args"></param>
	    /// <param name="sender"></param>
	    /// <returns></returns>
	    public static bool IsRaisedEventCancelled<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            EventMessages evtMsgs,
            Func<EventMessages, TArgs> args,
            TSender sender)
            where TArgs : CancellableEventArgs
        {
            var evtArgs = args(evtMsgs);

	        if (eventHandler != null)
	        {
                eventHandler(sender, evtArgs);
            }
            
            return evtArgs.Cancel;
        }

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
	    /// Raises the event with EventMessages
	    /// </summary>
	    /// <typeparam name="TSender"></typeparam>
	    /// <typeparam name="TArgs"></typeparam>
	    /// <param name="eventHandler"></param>
	    /// <param name="evtMsgs"></param>
	    /// <param name="args"></param>
	    /// <param name="sender"></param>
	    public static void RaiseEvent<TSender, TArgs>(
            this TypedEventHandler<TSender, TArgs> eventHandler,
            EventMessages evtMsgs,
            Func<EventMessages, TArgs> args,
            TSender sender)
            where TArgs : EventArgs
        {
            if (eventHandler != null)
	        {
                var evtArgs = args(evtMsgs);
                eventHandler(sender, evtArgs);
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
	}
}