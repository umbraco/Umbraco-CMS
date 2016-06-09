using NUnit.Framework;

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
            Umbraco.Web.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [TearDown]
        public virtual void TearDown()
        {
            SettingsForTests.Reset();
            Umbraco.Web.Current.UmbracoContextAccessor = null;
        }
    }
}