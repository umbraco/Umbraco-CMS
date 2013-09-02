using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentErrorsElementTests : UmbracoSettingsTests
    {
        [Test]
        public virtual void Can_Set_Multiple()
        {
            Assert.IsTrue(Section.Content.Errors.Error404Collection.Count == 3);
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(0).Culture == "default");
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(0).Value == 1047);
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(1).Culture == "en-US");
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(1).Value == 1048);
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(2).Culture == "en-UK");
            Assert.IsTrue(Section.Content.Errors.Error404Collection.ElementAt(2).Value == 1049);
        }        
    }
}