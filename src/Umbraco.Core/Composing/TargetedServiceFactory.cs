using System;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a base class for targeted service factories.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public abstract class TargetedServiceFactory<TService>
    {
        private readonly IServiceProvider _serviceProvider;

        protected TargetedServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TService For<TTarget>() => _serviceProvider.GetRequiredService<TService>();
    }
}
