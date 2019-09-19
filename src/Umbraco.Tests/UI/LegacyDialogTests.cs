using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web.UI;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.interfaces;

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

        [TestCase(typeof(XsltTasks), DefaultApps.developer)]
        [TestCase(typeof(StylesheetTasks), DefaultApps.settings)]
        [TestCase(typeof(stylesheetPropertyTasks), DefaultApps.settings)]
        [TestCase(typeof(MemberGroupTasks), DefaultApps.member)]
        [TestCase(typeof(dictionaryTasks), DefaultApps.settings)]
        [TestCase(typeof(macroTasks), DefaultApps.developer)]
        [TestCase(typeof(languageTasks), DefaultApps.settings)]
        [TestCase(typeof(CreatedPackageTasks), DefaultApps.developer)]
        public void Check_Assigned_Apps_For_Tasks(Type taskType, DefaultApps app)
        {
            var task = (LegacyDialogTask)Activator.CreateInstance(taskType);
            Assert.AreEqual(task.AssignedApp, app.ToString());
        }

    }
}
