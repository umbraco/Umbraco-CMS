using NUnit.Framework;
using Umbraco.Core.DependencyInjection;
using Current = Umbraco.Web.Current;

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
            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [TearDown]
        public virtual void TearDown()
        {
            SettingsForTests.Reset();
            Current.Reset();
        }
    }
}