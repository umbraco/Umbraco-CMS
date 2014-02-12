using NUnit.Framework;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class UmbracoEntityTests
    {
        [Test]
        public void UmbracoEntity_Can_Be_Initialized_From_Dynamic()
        {
            var boolIsTrue = true;
            var intIsTrue = 1;

            var trashedWithBool = new UmbracoEntity((dynamic)boolIsTrue);
            var trashedWithInt = new UmbracoEntity((dynamic)intIsTrue);

            Assert.IsTrue(trashedWithBool.Trashed);
            Assert.IsTrue(trashedWithInt.Trashed);
        }
    }
}