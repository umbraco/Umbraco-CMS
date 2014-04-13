using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Standalone
{
    /// <summary>
    /// An application standalone applications.
    /// </summary>
    internal class StandaloneApplication : UmbracoApplicationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneApplication"/> class.
        /// </summary>
        protected StandaloneApplication(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Provides the application boot manager.
        /// </summary>
        /// <returns>An application boot manager.</returns>
        protected override IBootManager GetBootManager()
        {
            return new StandaloneBootManager(this, _handlersToAdd, _handlersToRemove, _baseDirectory);
        }

        #region Application

        private static StandaloneApplication _application;
        private readonly string _baseDirectory;
        private static bool _started;
        private static readonly object AppLock = new object();

        /// <summary>
        /// Gets the instance of the standalone Umbraco application.
        /// </summary>
        public static StandaloneApplication GetApplication(string baseDirectory)
        {
            lock (AppLock)
            {
                return _application ?? (_application = new StandaloneApplication(baseDirectory));
            }
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        public void Start(bool noerr = false)
        {
            lock (AppLock)
            {
                if (_started)
                {
                    if (noerr) return;
                    throw new InvalidOperationException("Application has already started.");
                }
                try
                {
                    Application_Start(this, EventArgs.Empty);
                }
                catch
                {
                    TerminateInternal();
                    throw;
                }
                _started = true;
            }
        }

        public void Terminate(bool noerr = false)
        {
            lock (AppLock)
            {
                if (_started == false)
                {
                    if (noerr) return;
                    throw new InvalidOperationException("Application has already been terminated.");
                }

                TerminateInternal();
            }
        }

        private void TerminateInternal()
        {
            if (ApplicationContext.Current != null)
            {
                ApplicationContext.Current.DisposeIfDisposable(); // should reset resolution, clear caches & resolvers...
                ApplicationContext.Current = null;
            }
            if (UmbracoContext.Current != null)
            {
                UmbracoContext.Current.DisposeIfDisposable(); // dunno
                UmbracoContext.Current = null;
            }
            _started = false;
            _application = null;
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
        public StandaloneApplication WithApplicationEventHandler<T>()
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
        public StandaloneApplication WithoutApplicationEventHandler<T>()
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
        public StandaloneApplication WithApplicationEventHandler(Type type)
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
        public StandaloneApplication WithoutApplicationEventHandler(Type type)
        {
            if (type.Implements<IApplicationEventHandler>() == false)
                throw new ArgumentException("Type does not implement IApplicationEventHandler.", "type");
            _handlersToRemove.Add(type);
            return this;
        }

        #endregion

        #region Shortcuts to contexts

        /// <summary>
        /// Gets the current <see cref="ApplicationContext"/>.
        /// </summary>
        /// <remarks>This is a shortcut for scripts to be able to do <c>$app.ApplicationContext</c>.</remarks>
        public ApplicationContext ApplicationContext { get { return ApplicationContext.Current; } }

        /// <summary>
        /// Gets the current <see cref="UmbracoContext"/>.
        /// </summary>
        /// <remarks>This is a shortcut for scripts to be able to do <c>$app.UmbracoContext</c>.</remarks>
        public UmbracoContext UmbracoContext { get { return UmbracoContext.Current; } }

        #endregion
    }
}
