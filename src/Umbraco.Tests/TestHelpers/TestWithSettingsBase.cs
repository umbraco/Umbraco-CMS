using NUnit.Framework;
using Umbraco.Tests.TestHelpers.Stubs;
using Current = Umbraco.Web.Current;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides a base class for all Umbraco tests that use <c>SettingsForTests</c>.
    /// </summary>
    /// <remarks>
    /// <para>Ensures that SettingsForTests is property resetted before and after each test executes.</para>
    /// <para>Sets a test Umbraco context accessor.</para>
    /// </remarks>
    public abstract class TestWithSettingsBase : UmbracoTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            SettingsForTests.Reset();
            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        public override void TearDown()
        {
            base.TearDown();

            SettingsForTests.Reset();
        }
    }
}