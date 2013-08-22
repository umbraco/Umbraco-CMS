using Umbraco.Tests.PartialTrust;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    public abstract class ExamineBaseTest<T> : AbstractPartialTrustFixture<T> where T : class, IPartialTrustFixture, new()
    {
        public override void TestSetup()
        {
            UmbracoExamineSearcher.DisableInitializationCheck = true;
            BaseUmbracoIndexer.DisableInitializationCheck = true;
        }

        public override void TestTearDown()
        {
            UmbracoExamineSearcher.DisableInitializationCheck = null;
            BaseUmbracoIndexer.DisableInitializationCheck = null;
        }
    }
}