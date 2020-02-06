using System;

namespace Umbraco.Core.Exceptions
{
    class ResolveUnregisteredDependencyException : ApplicationException
    {
        public ResolveUnregisteredDependencyException(Type serviceType)
            : base($"An attempt was made to resolve the unregistered type: {serviceType.FullName}")
        { }
    }
}
