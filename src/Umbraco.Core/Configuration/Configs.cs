using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Represents Umbraco configurations.
    /// </summary>
    /// <remarks>
    /// <para>During composition, use composition.Configs. When running, either inject the required configuration,
    /// or use Current.Configs.</para>
    /// </remarks>
    public class Configs
    {
        private readonly Dictionary<Type, Lazy<object>> _configs = new Dictionary<Type, Lazy<object>>();
        private Dictionary<Type, Action<IServiceCollection>> _registerings = new Dictionary<Type, Action<IServiceCollection>>();

        /// <summary>
        /// Gets a configuration.
        /// </summary>
        public TConfig GetConfig<TConfig>()
            where TConfig : class
        {
            if (!_configs.TryGetValue(typeof(TConfig), out var configFactory))
                throw new InvalidOperationException($"No configuration of type {typeof(TConfig)} has been added.");

            return (TConfig) configFactory.Value;
        }

        /// <summary>
        /// Adds a configuration, provided by a factory.
        /// </summary>
        public void Add<TConfig>(Func<TConfig> configFactory)
            where TConfig : class
        {
            // make sure it is not too late
            if (_registerings == null)
                throw new InvalidOperationException("Configurations have already been registered.");

            var typeOfConfig = typeof(TConfig);

            var lazyConfigFactory = _configs[typeOfConfig] = new Lazy<object>(configFactory);
            _registerings[typeOfConfig] = register =>   register.Add(new ServiceDescriptor(typeof(TConfig), _ => (TConfig) lazyConfigFactory.Value, ServiceLifetime.Singleton));
        }

        /// <summary>
        /// Registers configurations in a register.
        /// </summary>
        public void RegisterWith(IServiceCollection services)
        {
            // do it only once
            if (_registerings == null)
                throw new InvalidOperationException("Configurations have already been registered.");

            services.AddSingleton(this);

            foreach (var registering in _registerings.Values)
                registering(services);

            // no need to keep them around
            _registerings = null;
        }
    }
}
