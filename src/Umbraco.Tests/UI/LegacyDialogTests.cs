using System;
using NUnit.Framework;
using Umbraco.Core;
using umbraco;
using umbraco.cms.presentation.user;
using Umbraco.Core.Composing;
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

        [TestCase(typeof(UserTypeTasks), Constants.Applications.Users)]
        [TestCase(typeof(XsltTasks), Constants.Applications.Developer)]
        [TestCase(typeof(userTasks), Constants.Applications.Users)]
        [TestCase(typeof(templateTasks), Constants.Applications.Settings)]
        [TestCase(typeof(StylesheetTasks), Constants.Applications.Settings)]
        [TestCase(typeof(stylesheetPropertyTasks), Constants.Applications.Settings)]
        [TestCase(typeof(ScriptTasks), Constants.Applications.Settings)]
        [TestCase(typeof(MemberGroupTasks), Constants.Applications.Members)]
        [TestCase(typeof(dictionaryTasks), Constants.Applications.Settings)]
        [TestCase(typeof(macroTasks), Constants.Applications.Developer)]
        [TestCase(typeof(languageTasks), Constants.Applications.Settings)]
        [TestCase(typeof(CreatedPackageTasks), Constants.Applications.Developer)]
        [TestCase(typeof(PartialViewTasks), Constants.Applications.Settings)]
        public void Check_Assigned_Apps_For_Tasks(Type taskType, string app)
        {
            var task = (LegacyDialogTask)Activator.CreateInstance(taskType);
            Assert.AreEqual(task.AssignedApp, app);
        }

    }
}
