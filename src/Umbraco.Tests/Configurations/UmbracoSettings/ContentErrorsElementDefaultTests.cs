using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentErrorsElementDefaultTests : ContentErrorsElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void Can_Set_Multiple()
        {
            Assert.IsTrue(Section.Content.Errors.Error404Collection.Count() == 1);
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(0).Culture == null);
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(0).ContentId == 1);
        }
    }
}