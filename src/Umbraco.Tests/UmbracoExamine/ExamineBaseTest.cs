using NUnit.Framework;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest
    {
        [SetUp]
        public virtual void TestSetup()
        {
            UmbracoExamineSearcher.DisableInitializationCheck = true;
            BaseUmbracoIndexer.DisableInitializationCheck = true;
        }

        [TearDown]
        public virtual void TestTearDown()
        {
            UmbracoExamineSearcher.DisableInitializationCheck = null;
            BaseUmbracoIndexer.DisableInitializationCheck = null;
        }
    }
}