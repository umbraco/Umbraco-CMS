using NUnit.Framework;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : BaseUmbracoConfigurationTest
    {

        [SetUp]
        public virtual void TestSetup()
        {
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new DefaultShortStringHelper(SettingsForTests.GetDefault()));

            Resolution.Freeze();
        }

        [TearDown]
        public virtual void TestTearDown()
        {
            //reset all resolvers
            ResolverCollection.ResetAll();
            //reset resolution itself (though this should be taken care of by resetting any of the resolvers above)
            Resolution.Reset();
        }
    }
}