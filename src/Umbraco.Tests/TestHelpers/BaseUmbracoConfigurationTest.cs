using NUnit.Framework;
using Umbraco.Core;
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
            SettingsForTests.Reset();
            
        }

        [TearDown]
        public virtual void TearDown()
        {
            //reset settings
            SettingsForTests.Reset();            
            
        }
    }
}