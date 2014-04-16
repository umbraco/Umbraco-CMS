using NUnit.Framework;
using Umbraco.Tests.PartialTrust;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    public abstract class ExamineBaseTest<T>
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