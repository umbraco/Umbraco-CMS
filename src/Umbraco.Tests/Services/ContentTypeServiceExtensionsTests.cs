using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class ContentTypeServiceExtensionsTests : BaseUmbracoApplicationTest
    {
        [Test]
        public void GetAvailableCompositeContentTypes_Not_Itself()
        {
            var ct1 = MockedContentTypes.CreateBasicContentType("ct1", "CT1", null);            
            var ct2 = MockedContentTypes.CreateBasicContentType("ct2", "CT2", null);
            var ct3 = MockedContentTypes.CreateBasicContentType("ct3", "CT3", null);
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            var service = new Mock<IContentTypeService>();

            var availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] {ct1, ct2, ct3});

            Assert.AreEqual(2, availableTypes.Count());
            Assert.AreEqual(ct2.Id, availableTypes.ElementAt(0).Id);
            Assert.AreEqual(ct3.Id, availableTypes.ElementAt(1).Id);
        }

        //This shows that a nested comp is not allowed
        [Test]
        public void GetAvailableCompositeContentTypes_No_Results_If_Already_A_Composition_By_Parent()
        {
            var ct1 = MockedContentTypes.CreateBasicContentType("ct1", "CT1");
            var ct2 = MockedContentTypes.CreateBasicContentType("ct2", "CT2", ct1);
            var ct3 = MockedContentTypes.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            var service = new Mock<IContentTypeService>();

            var availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 });

            Assert.AreEqual(0, availableTypes.Count());
        }

        //This shows that a nested comp is not allowed
        [Test]
        public void GetAvailableCompositeContentTypes_No_Results_If_Already_A_Composition()
        {
            var ct1 = MockedContentTypes.CreateBasicContentType("ct1", "CT1");
            var ct2 = MockedContentTypes.CreateBasicContentType("ct2", "CT2");
            var ct3 = MockedContentTypes.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            ct2.AddContentType(ct1);

            var service = new Mock<IContentTypeService>();

            var availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 });

            Assert.AreEqual(0, availableTypes.Count());
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Do_Not_Include_Other_Composed_Types()
        {
            var ct1 = MockedContentTypes.CreateBasicContentType("ct1", "CT1");
            var ct2 = MockedContentTypes.CreateBasicContentType("ct2", "CT2");
            var ct3 = MockedContentTypes.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            ct2.AddContentType(ct3);

            var service = new Mock<IContentTypeService>();

            var availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 });

            Assert.AreEqual(1, availableTypes.Count());
            Assert.AreEqual(ct3.Id, availableTypes.Single().Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Include_Direct_Composed_Types()
        {
            var ct1 = MockedContentTypes.CreateBasicContentType("ct1", "CT1");
            var ct2 = MockedContentTypes.CreateBasicContentType("ct2", "CT2");
            var ct3 = MockedContentTypes.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            ct1.AddContentType(ct3);

            var service = new Mock<IContentTypeService>();

            var availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 });

            Assert.AreEqual(2, availableTypes.Count());
            Assert.AreEqual(ct2.Id, availableTypes.ElementAt(0).Id);
            Assert.AreEqual(ct3.Id, availableTypes.ElementAt(1).Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Include_Indirect_Composed_Types()
        {
            var ct1 = MockedContentTypes.CreateBasicContentType("ct1", "CT1");
            var ct2 = MockedContentTypes.CreateBasicContentType("ct2", "CT2");
            var ct3 = MockedContentTypes.CreateBasicContentType("ct3", "CT3");
            var ct4 = MockedContentTypes.CreateBasicContentType("ct4", "CT4");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;
            ct4.Id = 4;

            ct1.AddContentType(ct3);    //ct3 is direct to ct1
            ct3.AddContentType(ct4);    //ct4 is indirect to ct1

            var service = new Mock<IContentTypeService>();

            var availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 });

            Assert.AreEqual(3, availableTypes.Count());
            Assert.AreEqual(ct2.Id, availableTypes.ElementAt(0).Id);
            Assert.AreEqual(ct3.Id, availableTypes.ElementAt(1).Id);
            Assert.AreEqual(ct4.Id, availableTypes.ElementAt(2).Id);
        }
    }
}