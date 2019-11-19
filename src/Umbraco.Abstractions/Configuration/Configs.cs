using System;
using System.Collections.Generic;
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
        private readonly Func<string, object> _configGetter;

        public Configs(Func<string, object> configGetter)
        {
            _configGetter = configGetter;
        }

        private readonly Dictionary<Type, Lazy<object>> _configs = new Dictionary<Type, Lazy<object>>();
        private Dictionary<Type, Action<IRegister>> _registerings = new Dictionary<Type, Action<IRegister>>();
        private Lazy<IFactory> _factory;

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
            _registerings[typeOfConfig] = register => register.Register(_ => (TConfig) lazyConfigFactory.Value, Lifetime.Singleton);
        }

        /// <summary>
        /// Adds a configuration, provided by a factory.
        /// </summary>
        public void Add<TConfig>(Func<IFactory, TConfig> configFactory)
            where TConfig : class
        {
            // make sure it is not too late
            if (_registerings == null)
                throw new InvalidOperationException("Configurations have already been registered.");

            var typeOfConfig = typeof(TConfig);

            _configs[typeOfConfig] = new Lazy<object>(() =>
            {
                if (!(_factory is null)) return _factory.Value.GetInstance<TConfig>();
                throw new InvalidOperationException($"Cannot get configuration of type {typeOfConfig} during composition.");
            });
            _registerings[typeOfConfig] = register => register.Register(configFactory, Lifetime.Singleton);
        }

        /// <summary>
        /// Adds a configuration, provided by a configuration section.
        /// </summary>
        public void Add<TConfig>(string sectionName)
            where TConfig : class
        {
            Add(() => GetConfig<TConfig>(sectionName));
        }

        private TConfig GetConfig<TConfig>(string sectionName)
            where TConfig : class
        {
            // note: need to use SafeCallContext here because ConfigurationManager.GetSection ends up getting AppDomain.Evidence
            // which will want to serialize the call context including anything that is in there - what a mess!

            using (new SafeCallContext())
            {
                if ((_configGetter(sectionName) is TConfig config))
                    return config;
                var ex = new InvalidOperationException($"Could not get configuration section \"{sectionName}\" from config files.");
                throw ex;
            }
        }

        /// <summary>
        /// Registers configurations in a register.
        /// </summary>
        public void RegisterWith(IRegister register, Func<IFactory> factory)
        {
            // do it only once
            if (_registerings == null)
                throw new InvalidOperationException("Configurations have already been registered.");

            _factory = new Lazy<IFactory>(factory);

            register.Register(this);

            foreach (var registering in _registerings.Values)
                registering(register);

            // no need to keep them around
            _registerings = null;
        }
    }
}
