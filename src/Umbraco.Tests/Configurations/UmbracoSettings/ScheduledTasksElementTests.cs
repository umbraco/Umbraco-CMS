using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ScheduledTasksElementTests : UmbracoSettingsTests
    {
        [Test]
        public virtual void Tasks()
        {
            Assert.IsTrue(Section.ScheduledTasks.Tasks.Count() == 2);
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(0).Alias == "test60");
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(0).Log == true);
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(0).Interval == 60);
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(0).Url == "http://localhost/umbraco/test.aspx");
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(1).Alias == "testtest");
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(1).Log == false);
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(1).Interval == 61);
            Assert.IsTrue(Section.ScheduledTasks.Tasks.ElementAt(1).Url == "http://localhost/umbraco/test1.aspx");
        }

    }
}