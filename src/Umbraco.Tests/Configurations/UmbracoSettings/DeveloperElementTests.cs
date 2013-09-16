using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DeveloperElementTests : UmbracoSettingsTests
    {
        [Test]
        public void AppCodeFileExtensions()
        {
            Assert.IsTrue(SettingsSection.Developer.AppCodeFileExtensions.Count() == 2);
            Assert.IsTrue(SettingsSection.Developer.AppCodeFileExtensions.All(
                x => "cs,vb".Split(',').Contains(x.Extension)));
        }
    }
}