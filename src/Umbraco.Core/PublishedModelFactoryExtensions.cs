using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IPublishedModelFactory"/>.
    /// </summary>
    public static class PublishedModelFactoryExtensions
    {
        /// <summary>
        /// Returns true if the current <see cref="IPublishedModelFactory"/> is an implementation of <see cref="ILivePublishedModelFactory"/>
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static bool IsLiveFactory(this IPublishedModelFactory factory) => factory is ILivePublishedModelFactory;

        /// <summary>
        /// Executes an action with a safe live factory
        /// </summary>
        /// <remarks>
        /// <para>If the factory is a live factory, ensures it is refreshed and locked while executing the action.</para>
        /// </remarks>
        public static void WithSafeLiveFactory(this IPublishedModelFactory factory, Action action)
        {
            if (factory is ILivePublishedModelFactory liveFactory)
            {
                lock (liveFactory.SyncRoot)
                {
                    if (_suspend != null)
                    {
                        //if we are currently suspended, queue the action
                        _suspend.Queue(action);
                    }
                    else
                    {
                        //Call refresh on the live factory to re-compile the models
                        liveFactory.Refresh();
                        action();
                    }
                }
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Creates a strongly typed model while checking if the factory is <see cref="ILivePublishedModelFactory"/> and if a refresh flag has been set, in which
        /// case the models will be recompiled before model creation
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        internal static IPublishedContent CreateModelWithSafeLiveFactoryRefreshCheck(this IPublishedModelFactory factory, IPublishedContent content)
        {
            if (factory is ILivePublishedModelFactory liveFactory && _refresh)
            {
                lock (liveFactory.SyncRoot)
                {
                    if (_refresh)
                    {
                        _refresh = false;
                        //Call refresh on the live factory to re-compile the models
                        liveFactory.Refresh();
                    }
                }
            }

            var model = factory.CreateModel(content);
            if (model == null)
                throw new Exception("Factory returned null.");

            // if factory returns a different type, throw
            if (!(model is IPublishedContent publishedContent))
                throw new Exception($"Factory returned model of type {model.GetType().FullName} which does not implement IPublishedContent.");

            return publishedContent;
        }

        /// <summary>
        /// Sets a flag to re-compile the models if the <see cref="IPublishedModelFactory"/> is <see cref="ILivePublishedModelFactory"/>
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        internal static void WithSafeLiveFactoryRefreshSet(this IPublishedModelFactory factory, Action action)
        {
            if (factory is ILivePublishedModelFactory liveFactory)
            {
                lock (liveFactory.SyncRoot)
                {
                    _refresh = true;
                    action();
                }
            }
            else
            {
                action();
            }
        }

        private static volatile bool _refresh = false;

        public static IDisposable SuspendSafeLiveFactory(this IPublishedModelFactory factory)
        {
            if (factory is ILivePublishedModelFactory liveFactory)
            {
                lock (liveFactory.SyncRoot)
                {
                    if (_suspend == null)
                    {
                        _suspend = new SuspendSafeLiveFactory(
                            factory,
                            () => _suspend = null); //reset when it's done
                    }
                    return _suspend;
                }
            }
            else
            {
                return new SuspendSafeLiveFactory(factory); //returns a noop version of IDisposable, this won't actually do anything
            }
        }

        private static SuspendSafeLiveFactory _suspend;
    }

    internal class SuspendSafeLiveFactory : IDisposable
    {
        private readonly IPublishedModelFactory _factory;
        private readonly Action _reset;
        private readonly List<Action> _actions = new List<Action>();

        public SuspendSafeLiveFactory(IPublishedModelFactory factory, Action reset = null)
        {
            _factory = factory;
            _reset = reset;
        }

        /// <summary>
        /// Queue an action to execute on disposal after rebuild
        /// </summary>
        /// <param name="action"></param>
        public void Queue(Action action)
        {
            _actions.Add(action);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_factory is ILivePublishedModelFactory liveFactory)
                    {
                        lock (liveFactory.SyncRoot)
                        {
                            //Call refresh on the live factory to re-compile the models
                            liveFactory.Refresh();

                            //then we need to call all queued actions
                            foreach(var action in _actions)
                                action();
                        }
                    }
                    _reset?.Invoke();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
