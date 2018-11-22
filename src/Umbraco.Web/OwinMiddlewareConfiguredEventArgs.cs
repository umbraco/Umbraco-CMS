using System;
using Owin;

namespace Umbraco.Web
{
    public class OwinMiddlewareConfiguredEventArgs : EventArgs
    {
        public OwinMiddlewareConfiguredEventArgs(IAppBuilder appBuilder)
        {
            AppBuilder = appBuilder;
        }

        public IAppBuilder AppBuilder { get; private set; }
    }
}