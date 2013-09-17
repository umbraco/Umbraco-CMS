using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Umbraco.Core.Configuration.BaseRest;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The gateway to all umbraco configuration
    /// </summary>
    public class UmbracoConfiguration
    {
        #region Singleton

        private static readonly Lazy<UmbracoConfiguration> Lazy = new Lazy<UmbracoConfiguration>(() => new UmbracoConfiguration());

        public static UmbracoConfiguration Current
        {
            get { return Lazy.Value; }            
        }

        #endregion

        #region Extensible settings
        
        private static readonly ConcurrentDictionary<Type, IUmbracoConfigurationSection> Sections = new ConcurrentDictionary<Type, IUmbracoConfigurationSection>();

        
        /// <summary>
        /// Used for unit tests to explicitly associate an IUmbracoConfigurationSection to an implementation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="implementation"></param>
        internal static void Set<T>(T implementation)
            where T : IUmbracoConfigurationSection
        {
            Sections.AddOrUpdate(typeof (T), type => implementation, (type, section) => implementation);
        }

        /// <summary>
        /// Used for unit tests to reset the resolved sections
        /// </summary>
        internal static void Reset()
        {
            Sections.Clear();
        }

        /// <summary>
        /// Gets the specified UmbracoConfigurationSection.
        /// </summary>
        /// <typeparam name="T">The type of the UmbracoConfigurationSectiont.</typeparam>
        /// <returns>The UmbracoConfigurationSection of the specified type.</returns>
        public static T For<T>(System.Configuration.Configuration config = null)
            where T : IUmbracoConfigurationSection
        {
            var sectionType = typeof(T);
            return (T)Sections.GetOrAdd(sectionType, type =>
                {
                    //if there is no entry for this type 
                    var configurationSections = PluginManager.Current.ResolveUmbracoConfigurationSections();
                    var implementationType = configurationSections.FirstOrDefault(TypeHelper.IsTypeAssignableFrom<T>);
                    if (implementationType == null)
                    {
                        throw new InvalidOperationException("Could not find an implementation for " + typeof(T));
                    }

                    var attr = implementationType.GetCustomAttribute<ConfigurationKeyAttribute>(false);
                    if (attr == null)
                        throw new InvalidOperationException(string.Format("Type \"{0}\" is missing attribute ConfigurationKeyAttribute.", sectionType.FullName));

                    var sectionKey = attr.ConfigurationKey;
                    if (string.IsNullOrWhiteSpace(sectionKey))
                        throw new InvalidOperationException(string.Format("Type \"{0}\" {1} value is null or empty.", sectionType.FullName, typeof(ConfigurationKeyAttribute)));

                    var section = GetSection(sectionType, sectionKey, config);

                    return (T)section;        
                });
        }

        private static IUmbracoConfigurationSection GetSection(Type sectionType, string key, System.Configuration.Configuration config = null)
        {
            var section = config == null
                              ? ConfigurationManager.GetSection(key)
                              : config.GetSection(key);

            if (section == null)
            {
                throw new KeyNotFoundException("Could not find/load config section: " + key);
            }

            var result = section as IUmbracoConfigurationSection;
            if (result == null)
            {
                throw new InvalidOperationException(string.Format("The section type requested '{0}' does not match the resulting section type '{1}", sectionType, section.GetType()));
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Default constructor 
        /// </summary>
        private UmbracoConfiguration()
        {
            if (UmbracoSettings == null)
            {
                var umbracoSettings = ConfigurationManager.GetSection("umbracoConfiguration/settings") as IUmbracoSettingsSection;
                if (umbracoSettings == null)
                {
                    LogHelper.Warn<UmbracoConfiguration>("Could not load the " + typeof(IUmbracoSettingsSection) + " from config file!");
                }
                UmbracoSettings = umbracoSettings;
            }

            if (BaseRestExtensions == null)
            {
                var baseRestExtensions = ConfigurationManager.GetSection("umbracoConfiguration/BaseRestExtensions") as IBaseRestSection;
                if (baseRestExtensions == null)
                {
                    LogHelper.Warn<UmbracoConfiguration>("Could not load the " + typeof(IBaseRestSection) + " from config file!");
                }
                BaseRestExtensions = baseRestExtensions;
            }
        }

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoSettings"></param>
        /// <param name="baseRestSettings"></param>
        public UmbracoConfiguration(IUmbracoSettingsSection umbracoSettings, IBaseRestSection baseRestSettings)
        {
            UmbracoSettings = umbracoSettings;
            BaseRestExtensions = baseRestSettings;
        }
        
        /// <summary>
        /// Gets the IUmbracoSettings
        /// </summary>
        public IUmbracoSettingsSection UmbracoSettings
        {
            get; 
            //This is purely for setting for Unit tests ONLY
            internal set;
        }

        public IBaseRestSection BaseRestExtensions { get; private set; }

        //TODO: Add other configurations here !
    }
}