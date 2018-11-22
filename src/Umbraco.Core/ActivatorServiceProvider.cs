using System;

namespace Umbraco.Core
{
    internal class ActivatorServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }
    }
}