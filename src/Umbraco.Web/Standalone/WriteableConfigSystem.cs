using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Internal;
using System.Reflection;
using System.Threading;

namespace Umbraco.Web.Standalone
{
    // see http://stackoverflow.com/questions/15653621/how-to-update-add-modify-delete-keys-in-appsettings-section-of-web-config-at-r
    // see http://www.codeproject.com/Articles/69364/Override-Configuration-Manager

    internal sealed class WriteableConfigSystem : IInternalConfigSystem
    {
        private static readonly ReaderWriterLockSlim RwLock = new ReaderWriterLockSlim();
        private static WriteableConfigSystem _installed;
        private static IInternalConfigSystem _clientConfigSystem;
        private object _appsettings;
        private object _connectionStrings;
        private static object _sInitStateOrig;
        private static object _sConfigSystemOrig;

        public static bool Installed
        {
            get
            {
                try
                {
                    RwLock.EnterReadLock();
                    return _installed != null;
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Re-initializes the ConfigurationManager, allowing us to merge in the settings from Core.Config
        /// </summary>
        public static void Install()
        {
            try
            {
                RwLock.EnterWriteLock();

                if (_installed != null)
                    throw new InvalidOperationException("ConfigSystem is already installed.");

                FieldInfo[] fiStateValues = null;
                var tInitState = typeof(ConfigurationManager).GetNestedType("InitState", BindingFlags.NonPublic);

                if (tInitState != null)
                    fiStateValues = tInitState.GetFields();
                // 0: NotStarted
                // 1: Started
                // 2: Usable
                // 3: Completed

                var fiInit = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static);
                var fiSystem = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static);

                if (fiInit != null && fiSystem != null && fiStateValues != null)
                {
                    _sInitStateOrig = fiInit.GetValue(null);
                    _sConfigSystemOrig = fiSystem.GetValue(null);
                    fiInit.SetValue(null, fiStateValues[1].GetValue(null)); // set to Started
                    fiSystem.SetValue(null, null); // clear current config system
                }

                _installed = new WriteableConfigSystem();

                var configFactoryType = Type.GetType("System.Configuration.Internal.InternalConfigSettingsFactory, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
                var configSettingsFactory = (IInternalConfigSettingsFactory)Activator.CreateInstance(configFactoryType, true);
                // just does ConfigurationManager.SetConfigurationSystem(_installed, false);
                // 'false' turns initState to 2 ie usable (vs 3 ie completed)
                configSettingsFactory.SetConfigurationSystem(_installed, false);

                // note: prob. don't need the factory... see how we uninstall...

                var clientConfigSystemType = Type.GetType("System.Configuration.ClientConfigurationSystem, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
                _clientConfigSystem = (IInternalConfigSystem)Activator.CreateInstance(clientConfigSystemType, true);
            }
            finally
            {
                RwLock.ExitWriteLock();
            }
        }

        public static void Uninstall()
        {
            try
            {
                RwLock.EnterWriteLock();

                if (_installed == null)
                    throw new InvalidOperationException("ConfigSystem is not installed.");

                FieldInfo[] fiStateValues = null;
                var tInitState = typeof(ConfigurationManager).GetNestedType("InitState", BindingFlags.NonPublic);

                if (tInitState != null)
                    fiStateValues = tInitState.GetFields();

                var fiInit = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static);
                var fiSystem = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static);

                if (fiInit != null && fiSystem != null && fiStateValues != null)
                {
                    // reset - the hard way
                    fiInit.SetValue(null, _sInitStateOrig);
                    fiSystem.SetValue(null, _sConfigSystemOrig);
                }

                _installed = null;
                _clientConfigSystem = null;
            }
            finally
            {
                RwLock.ExitWriteLock();
            }
        }

        public static void Reset()
        {
            try
            {
                RwLock.EnterWriteLock();

                if (_installed == null)
                    throw new InvalidOperationException("ConfigSystem is not installed.");

                _installed._appsettings = null;
                _installed._connectionStrings = null;
            }
            finally
            {
                RwLock.ExitWriteLock();
            }
        }

        #region IInternalConfigSystem Members

        public object GetSection(string configKey)
        {
            // get the section from the default location (web.config or app.config)
            var section = _clientConfigSystem.GetSection(configKey);

            try
            {
                RwLock.EnterReadLock();

                switch (configKey)
                {
                    case "appSettings":
                        // Return cached version if exists
                        if (_appsettings != null)
                            return _appsettings;

                        // create a new collection because the underlying collection is read-only
                        var cfg = new NameValueCollection();

                        // If an AppSettings section exists in Web.config, read and add values from it
                        var nvSection = section as NameValueCollection;
                        if (nvSection != null)
                        {
                            var localSettings = nvSection;
                            foreach (string key in localSettings)
                                cfg.Add(key, localSettings[key]);
                        }

                        //// --------------------------------------------------------------------
                        //// Here I read and decrypt keys and add them to secureConfig dictionary
                        //// To test assume the following line is a key stored in secure sotrage.
                        ////secureConfig = SecureConfig.LoadConfig();
                        //secureConfig.Add("ACriticalKey", "VeryCriticalValue");
                        //// --------------------------------------------------------------------                        
                        //foreach (KeyValuePair<string, string> item in secureConfig)
                        //{
                        //    if (cfg.AllKeys.Contains(item.Key))
                        //    {
                        //        cfg[item.Key] = item.Value;
                        //    }
                        //    else
                        //    {
                        //        cfg.Add(item.Key, item.Value);
                        //    }
                        //}
                        //// --------------------------------------------------------------------                        


                        // Cach the settings for future use

                        _appsettings = cfg;
                        // return the merged version of the items from secure storage and appsettings
                        section = _appsettings;
                        break;

                    case "connectionStrings":
                        // Return cached version if exists
                        if (_connectionStrings != null)
                            return _connectionStrings;

                        // create a new collection because the underlying collection is read-only
                        var connectionStringsSection = new ConnectionStringsSection();

                        // copy the existing connection strings into the new collection
                        foreach (
                            ConnectionStringSettings connectionStringSetting in
                                ((ConnectionStringsSection)section).ConnectionStrings)
                            connectionStringsSection.ConnectionStrings.Add(connectionStringSetting);

                        // --------------------------------------------------------------------
                        // Again Load connection strings from secure storage and merge like below
                        // connectionStringsSection.ConnectionStrings.Add(connectionStringSetting);
                        // --------------------------------------------------------------------                        

                        // Cach the settings for future use
                        _connectionStrings = connectionStringsSection;
                        // return the merged version of the items from secure storage and appsettings
                        section = _connectionStrings;
                        break;
                }
            }
            finally
            {
                RwLock.ExitReadLock();
            }

            return section;
        }

        public void RefreshConfig(string sectionName)
        {
            try
            {
                RwLock.EnterWriteLock();

                if (sectionName == "appSettings")
                {
                    _appsettings = null;
                }

                if (sectionName == "connectionStrings")
                {
                    _connectionStrings = null;
                }
            }
            finally
            {
                RwLock.ExitWriteLock();
            }

            _clientConfigSystem.RefreshConfig(sectionName);
        }

        public bool SupportsUserConfig { get { return _clientConfigSystem.SupportsUserConfig; } }

        #endregion
    }
}