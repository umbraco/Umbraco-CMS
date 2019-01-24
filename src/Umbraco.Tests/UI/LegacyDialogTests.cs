using System;
using NUnit.Framework;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web._Legacy.UI;

namespace Umbraco.Tests.UI
{
    [TestFixture]
    public class LegacyDialogTests
    {

        [Test]
        public void Ensure_All_Tasks_Are_Secured()
        {
            var allTasks = TypeFinder.FindClassesOfType<ITask>();

            foreach (var t in allTasks)
            {
                Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<LegacyDialogTask>(t), "The type " + t + " is not of type " + typeof(LegacyDialogTask));
            }
        }
        
        //[TestCase(typeof(macroTasks), Constants.Applications.Settings)]
        //public void Check_Assigned_Apps_For_Tasks(Type taskType, string app)
        //{
        //    var task = (LegacyDialogTask)Activator.CreateInstance(taskType);
        //    Assert.AreEqual(task.AssignedApp, app);
        //}

    }
}
