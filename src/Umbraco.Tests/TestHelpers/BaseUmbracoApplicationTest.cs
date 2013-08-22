using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// A base test class used for umbraco tests whcih sets up the logging, plugin manager any base resolvers, etc... and
    /// ensures everything is torn down properly.
    /// </summary>
    [TestFixture]
    public abstract class BaseUmbracoApplicationTest
    {
        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            SettingsForTests.UseLegacyXmlSchema = false;
            SettingsForTests.ForceSafeAliases = true;
            SettingsForTests.UmbracoLibraryCacheDuration = 1800;
            
            SetupPluginManager();

            SetupApplicationContext();

            FreezeResolution();
        }

        [TearDown]
        public virtual void TearDown()
        {
            //reset settings
            SettingsForTests.Reset();
            UmbracoContext.Current = null;
            TestHelper.CleanContentDirectories();
            //reset the app context, this should reset most things that require resetting like ALL resolvers
            ApplicationContext.Current.DisposeIfDisposable();
            ApplicationContext.Current = null;
            ResetPluginManager();
        }
        
        /// <summary>
        /// By default this returns false which means the plugin manager will not be reset so it doesn't need to re-scan 
        /// all of the assemblies. Inheritors can override this if plugin manager resetting is required, generally needs
        /// to be set to true if the  SetupPluginManager has been overridden.
        /// </summary>
        protected virtual bool PluginManagerResetRequired
        {
            get { return false; }
        }

        /// <summary>
        /// Inheritors can resset the plugin manager if they choose to on teardown
        /// </summary>
        protected virtual void ResetPluginManager()
        {
            if (PluginManagerResetRequired)
            {
                PluginManager.Current = null;    
            }
        }

        /// <summary>
        /// Inheritors can override this if they wish to create a custom application context
        /// </summary>
        protected virtual void SetupApplicationContext()
        {
            //DO NOT ENABLE CACHE
            ApplicationContext.Current = new ApplicationContext(false) {IsReady = true};
        }

        /// <summary>
        /// Inheritors can override this if they wish to setup the plugin manager differenty (i.e. specify certain assemblies to load)
        /// </summary>
        protected virtual void SetupPluginManager()
        {
            if (PluginManager.Current == null || PluginManagerResetRequired)
            {
                PluginManager.Current = new PluginManager(false);    
            }
        }

        /// <summary>
        /// Inheritors can override this to setup any resolvers before resolution is frozen
        /// </summary>
        protected virtual void FreezeResolution()
        {
            Resolution.Freeze();
        }

        protected ApplicationContext ApplicationContext
        {
            get { return ApplicationContext.Current; }
        }
    }
}