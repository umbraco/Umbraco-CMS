using System;

namespace Umbraco.Net
{
    // TODO: This shouldn't be in this namespace?
    public interface IUmbracoApplicationLifetime
    {
        /// <summary>
        /// A value indicating whether the application is restarting after the current request.
        /// </summary>
        bool IsRestarting { get; }
        /// <summary>
        /// Terminates the current application. The application restarts the next time a request is received for it.
        /// </summary>
        void Restart();

        event EventHandler ApplicationInit;
    }


    public interface IUmbracoApplicationLifetimeManager
    {
        void InvokeApplicationInit();
    }
}
