using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Web.Install.UpgradeScripts;

namespace Umbraco.Tests.Install
{
    [TestFixture]
    public class UpgradeScriptsTests
    {
        [TearDown]
        public void TearDown()
        {
            UpgradeScriptManager.Clear();
        }

        [TestCase("0.0.0", "6.0.0", "4.10.0", true)]
        [TestCase("4.10.0", "6.0.0", "4.10.0", true)]
        [TestCase("4.10.0", "4.11.4", "4.10.0", true)]
        [TestCase("4.11.0", "4.11.4", "4.10.0", false)]
        [TestCase("4.11.0", "6.0.0", "4.10.0", false)]
        [TestCase("6.0.0", "6.0.0", "6.0.0", false)] //this is not in range because it is up to 6.0 but not including 6.0
        [TestCase("6.0.0", "6.0.0", "6.0.1", false)]
        public void Test_Version_Range(string startVersion, string endVersion, string current, bool inRange)
        {
            var currVersionParts = current.Split('.').Select(int.Parse).ToArray();
            var currentVersion = new Version(currVersionParts[0], currVersionParts[1], currVersionParts[2]);

            var startVersionParts = startVersion.Split('.').Select(int.Parse).ToArray();
            var endVersionParts = endVersion.Split('.').Select(int.Parse).ToArray();
            
            UpgradeScriptManager.AddUpgradeScript<UpgradeScript1>(
                new VersionRange(
                    new Version(startVersionParts[0], startVersionParts[1], startVersionParts[2]),
                    new Version(endVersionParts[0], endVersionParts[1], endVersionParts[2])));

            Assert.AreEqual(inRange, UpgradeScriptManager.HasScriptsForVersion(currentVersion));
        }

        [Test]
        public void Test_Specific_Version()
        {            
            var currentVersion = new Version(4, 10, 0);

            UpgradeScriptManager.AddUpgradeScript<UpgradeScript1>(
                new VersionRange(
                    new Version(4, 10, 0)));

            Assert.IsTrue(UpgradeScriptManager.HasScriptsForVersion(currentVersion));
            Assert.IsFalse(UpgradeScriptManager.HasScriptsForVersion(new Version(4, 10, 11)));
            Assert.IsFalse(UpgradeScriptManager.HasScriptsForVersion(new Version(4, 11, 0)));
        }

        public class UpgradeScript1 : IUpgradeScript
        {
            public void Execute()
            {
                Debug.WriteLine("Executing!");
            }
        }

    }
}
