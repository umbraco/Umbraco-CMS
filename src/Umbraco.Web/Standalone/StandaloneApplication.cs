using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;

namespace Umbraco.Web.Standalone
{
    /// <summary>
    /// An application standalone applications.
    /// </summary>
    class StandaloneApplication : UmbracoApplicationBase
    {
        /// <summary>
        /// Provides the application boot manager.
        /// </summary>
        /// <returns>An application boot manager.</returns>
        protected override IBootManager GetBootManager()
        {
            return new StandaloneBootManager(this, _appEventHandlers);
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        public void Start()
        {
            Application_Start(this, EventArgs.Empty);
        }

        private readonly List<Type> _appEventHandlers = new List<Type>();

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
            _appEventHandlers.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Associates an <see cref="IApplicationEventHandler"/> type with the application.
        /// </summary>
        /// <returns>The application.</returns>
        /// <remarks>Types implementing <see cref="IApplicationEventHandler"/> from within
        /// an executable are not automatically discovered by Umbraco and have to be
        /// explicitely associated with the application using this method.</remarks>
        public StandaloneApplication WithApplicationEventHandler(Type type)
        {
            if (type.Implements<IApplicationEventHandler>() == false)
                throw new ArgumentException("Type does not implement IApplicationEventHandler.", "type");
            _appEventHandlers.Add(type);
            return this;
        }

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
    }
}
