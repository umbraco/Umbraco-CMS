using System;
using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class PackageRepositoriesElementDefaultTests : PackageRepositoriesElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void Repositories()
        {
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.Count() == 1);
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).Id == Guid.Parse("65194810-1f85-11dd-bd0b-0800200c9a66"));
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).Name == "Umbraco package Repository");
        }
    }
}