using System;
using System.Collections.Generic;

namespace Umbraco.Core.Standalone
{
    internal class StandaloneCoreApplication : UmbracoApplicationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneCoreApplication"/> class.
        /// </summary>
        protected StandaloneCoreApplication() { }

        /// <summary>
        /// Provides the application boot manager.
        /// </summary>
        /// <returns>An application boot manager.</returns>
        protected override IBootManager GetBootManager()
        {
            return new StandaloneCoreBootManager(this, _handlersToAdd, _handlersToRemove);
        }

        #region Application

        private static StandaloneCoreApplication _application;
        private static bool _started;
        private static readonly object AppLock = new object();

        /// <summary>
        /// Gets the instance of the standalone Umbraco application.
        /// </summary>
        public static StandaloneCoreApplication GetApplication()
        {
            lock (AppLock)
            {
                return _application ?? (_application = new StandaloneCoreApplication());
            }
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        public void Start()
        {
            lock (AppLock)
            {
                if (_started)
                    throw new InvalidOperationException("Application has already started.");
                Application_Start(this, EventArgs.Empty);
                _started = true;
            }
        }

        #endregion

        #region IApplicationEventHandler management

        private readonly List<Type> _handlersToAdd = new List<Type>();
        private readonly List<Type> _handlersToRemove = new List<Type>();

        /// <summary>
        /// Associates an <see cref="IApplicationEventHandler"/> type with the application.
        /// </summary>
        /// <typeparam name="T">The type to associate.</typeparam>
        /// <returns>The application.</returns>
        /// <remarks>Types implementing <see cref="IApplicationEventHandler"/> from within
        /// an executable are not automatically discovered by Umbraco and have to be
        /// explicitely associated with the application using this method.</remarks>
        public StandaloneCoreApplication WithApplicationEventHandler<T>()
            where T : IApplicationEventHandler
        {
            _handlersToAdd.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Dissociates an <see cref="IApplicationEventHandler"/> type from the application.
        /// </summary>
        /// <typeparam name="T">The type to dissociate.</typeparam>
        /// <returns>The application.</returns>
        public StandaloneCoreApplication WithoutApplicationEventHandler<T>()
            where T : IApplicationEventHandler
        {
            _handlersToRemove.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Associates an <see cref="IApplicationEventHandler"/> type with the application.
        /// </summary>
        /// <param name="type">The type to associate.</param>
        /// <returns>The application.</returns>
        /// <remarks>Types implementing <see cref="IApplicationEventHandler"/> from within
        /// an executable are not automatically discovered by Umbraco and have to be
        /// explicitely associated with the application using this method.</remarks>
        public StandaloneCoreApplication WithApplicationEventHandler(Type type)
        {
            if (type.Implements<IApplicationEventHandler>() == false)
                throw new ArgumentException("Type does not implement IApplicationEventHandler.", "type");
            _handlersToAdd.Add(type);
            return this;
        }

        /// <summary>
        /// Dissociates an <see cref="IApplicationEventHandler"/> type from the application.
        /// </summary>
        /// <param name="type">The type to dissociate.</param>
        /// <returns>The application.</returns>
        public StandaloneCoreApplication WithoutApplicationEventHandler(Type type)
        {
            if (type.Implements<IApplicationEventHandler>() == false)
                throw new ArgumentException("Type does not implement IApplicationEventHandler.", "type");
            _handlersToRemove.Add(type);
            return this;
        }

        #endregion
    }
}