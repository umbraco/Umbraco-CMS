using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// A base test class that ensures that the umbraco configuration is mocked
    /// </summary>
    [TestFixture]
    public abstract class BaseUmbracoConfigurationTest
    {
        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            
            //mock the Umbraco settings that we need
            var settings = SettingsForTests.GetMockSettings();
            //sets the global singleton to use the mocked format
            SettingsForTests.ConfigureSettings(settings);
            //set our local variable for tests to use (preferably)
            UmbracoSettings = settings;
        }

        [TearDown]
        public virtual void TearDown()
        {
            //reset settings
            SettingsForTests.Reset();            
        }

        protected virtual IUmbracoSettingsSection UmbracoSettings { get; private set; }
    }
}