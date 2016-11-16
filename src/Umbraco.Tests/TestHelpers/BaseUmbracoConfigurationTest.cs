using NUnit.Framework;
using Umbraco.Core;

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
            Reset();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Reset();
        }

        private static void Reset()
        {
            // reset settings
            SettingsForTests.Reset();

            // clear the logical call context
            SafeCallContext.Clear();
        }
    }
}