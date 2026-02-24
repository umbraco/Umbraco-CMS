using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.TestServerTest
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
    public abstract class UmbracoTestServerTestBase : UmbracoTestServerFixtureBase
    {
        [SetUp]
        public virtual void Setup()
        {
            BuildAndStartWebApplication();
        }

        [TearDown]
        public void TearDownClient()
        {
            DisposeClientAndFactory();
        }

        [TearDown]
        public void TearDown()
        {
            ExecuteTearDownQueue();
        }

        [SetUp]
        public virtual void SetUp_Logging() =>
            TestContext.Out.Write($"Start test {TestCount++}: {TestContext.CurrentContext.Test.Name}");

        [TearDown]
        public void TearDown_Logging() =>
            TestContext.Out.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");
    }
}
