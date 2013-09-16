using System;
using System.Collections.Concurrent;
using System.Configuration;
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

        //#region Extensible settings
        
        //TODO: Need to think about this... it seems nicer to do this than having giant nested access to the configuration
        // sections, BUT we don't want to attribute the interfaces. We can make IUmbracoConfigurationSection plugins and then search for the matching
        // one based on the specified interface ?

        //private static readonly ConcurrentDictionary<Type, IUmbracoConfigurationSection> Sections = new ConcurrentDictionary<Type, IUmbracoConfigurationSection>();

        ///// <summary>
        ///// Gets the specified UmbracoConfigurationSection.
        ///// </summary>
        ///// <typeparam name="T">The type of the UmbracoConfigurationSectiont.</typeparam>
        ///// <returns>The UmbracoConfigurationSection of the specified type.</returns>
        //public static T For<T>()
        //    where T : IUmbracoConfigurationSection
        //{
        //    var sectionType = typeof(T);
        //    return (T)Sections.GetOrAdd(sectionType, type =>
        //        {
        //            var attr = sectionType.GetCustomAttribute<ConfigurationKeyAttribute>(false);
        //            if (attr == null)
        //                throw new InvalidOperationException(string.Format("Type \"{0}\" is missing attribute ConfigurationKeyAttribute.", sectionType.FullName));

        //            var sectionKey = attr.ConfigurationKey;
        //            if (string.IsNullOrWhiteSpace(sectionKey))
        //                throw new InvalidOperationException(string.Format("Type \"{0}\" ConfigurationKeyAttribute value is null or empty.", sectionType.FullName));

        //            var section = GetSection(sectionType, sectionKey);

        //            return (T)section;        
        //        });
        //}

        //private static IUmbracoConfigurationSection GetSection(Type sectionType, string key)
        //{
        //    if (TypeHelper.IsTypeAssignableFrom<IUmbracoConfigurationSection>(sectionType) == false)
        //    {
        //        throw new ArgumentException(string.Format(
        //           "Type \"{0}\" does not inherit from UmbracoConfigurationSection.", sectionType.FullName), "sectionType");
        //    }                

        //    var section = ConfigurationManager.GetSection(key);

        //    if (section != null && section.GetType() != sectionType)
        //        throw new InvalidCastException(string.Format("Section at key \"{0}\" is of type \"{1}\" and not \"{2}\".",
        //            key, section.GetType().FullName, sectionType.FullName));

        //    return section as IUmbracoConfigurationSection;
        //}

        //#endregion

        /// <summary>
        /// Default constructor 
        /// </summary>
        private UmbracoConfiguration()
        {
            if (UmbracoSettings == null)
            {
                var umbracoSettings = ConfigurationManager.GetSection("umbracoConfiguration/settings") as IUmbracoSettings;
                if (umbracoSettings == null)
                {
                    LogHelper.Warn<UmbracoConfiguration>("Could not load the IUmbracoSettings from config file!");
                }
                UmbracoSettings = umbracoSettings;
            }
        }

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoSettings"></param>
        /// <param name="baseRestSettings"></param>
        public UmbracoConfiguration(IUmbracoSettings umbracoSettings, IBaseRest baseRestSettings)
        {
            UmbracoSettings = umbracoSettings;
            BaseRestExtensions = baseRestSettings;
        }
        
        /// <summary>
        /// Gets the IUmbracoSettings
        /// </summary>
        public IUmbracoSettings UmbracoSettings
        {
            get; 
            //This is purely for setting for Unit tests ONLY
            internal set;
        }

        public IBaseRest BaseRestExtensions { get; private set; }

        //TODO: Add other configurations here !
    }
}