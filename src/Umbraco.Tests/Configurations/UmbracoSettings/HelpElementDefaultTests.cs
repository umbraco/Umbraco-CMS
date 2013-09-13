using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class HelpElementDefaultTests : HelpElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void Links()
        {
            Assert.IsTrue(Section.Help.Links.Count() == 0);
        }
    }
}