using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ScheduledTasksElementDefaultTests : ScheduledTasksElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void Tasks()
        {
            Assert.IsTrue(Section.ScheduledTasks.Tasks.Count == 0);
        }
    }
}