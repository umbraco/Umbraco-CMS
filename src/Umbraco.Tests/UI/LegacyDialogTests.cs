using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web.UI;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.user;
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

        [TestCase(typeof(UserTypeTasks), DefaultApps.users)]
        [TestCase(typeof(contentItemTypeTasks), DefaultApps.member)]
        [TestCase(typeof(contentItemTasks), DefaultApps.member)]
        [TestCase(typeof(XsltTasks), DefaultApps.developer)]
        [TestCase(typeof(userTasks), DefaultApps.users)]
        [TestCase(typeof(templateTasks), DefaultApps.settings)]
        [TestCase(typeof(StylesheetTasks), DefaultApps.settings)]
        [TestCase(typeof(stylesheetPropertyTasks), DefaultApps.settings)]
        [TestCase(typeof(ScriptTasks), DefaultApps.settings)]
        [TestCase(typeof(nodetypeTasks), DefaultApps.settings)]
        [TestCase(typeof(PythonTasks), DefaultApps.developer)]
        [TestCase(typeof(MemberTypeTasks), DefaultApps.member)]
        [TestCase(typeof(memberTasks), DefaultApps.member)]
        [TestCase(typeof(MemberGroupTasks), DefaultApps.member)]
        [TestCase(typeof(MediaTypeTasks), DefaultApps.settings)]
        [TestCase(typeof(dictionaryTasks), DefaultApps.settings)]
        [TestCase(typeof(macroTasks), DefaultApps.developer)]
        [TestCase(typeof(languageTasks), DefaultApps.settings)]
        [TestCase(typeof(DLRScriptingTasks), DefaultApps.developer)]
        [TestCase(typeof(DataTypeTasks), DefaultApps.developer)]
        [TestCase(typeof(CreatedPackageTasks), DefaultApps.developer)]
        [TestCase(typeof(PartialViewTasks), DefaultApps.settings)]
        public void Check_Assigned_Apps_For_Tasks(Type taskType, DefaultApps app)
        {
            var task = (LegacyDialogTask)Activator.CreateInstance(taskType);
            Assert.AreEqual(task.AssignedApp, app.ToString());
        }

    }
}
