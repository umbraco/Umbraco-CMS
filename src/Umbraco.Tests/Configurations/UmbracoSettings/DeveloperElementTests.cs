using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DeveloperElementDefaultTests : DeveloperElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }

    [TestFixture]
    public class DeveloperElementTests : UmbracoSettingsTests
    {
        [Test]
        public void AppCodeFileExtensions()
        {
            Assert.IsTrue(Section.Developer.AppCodeFileExtensions.AppCodeFileExtensionsCollection.Count == 2);
            Assert.IsTrue(Section.Developer.AppCodeFileExtensions.AppCodeFileExtensionsCollection.All(
                x => "cs,vb".Split(',').Contains(x.Value)));
        }
    }
}