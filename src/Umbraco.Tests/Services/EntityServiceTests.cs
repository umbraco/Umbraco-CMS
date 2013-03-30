using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the EntityService
    /// </summary>
    [TestFixture, RequiresSTA]
    public class EntityServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectTypes()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll(UmbracoObjectTypes.Document);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectType_Id()
        {
            var service = ServiceContext.EntityService;

            var objectTypeId = new Guid(Constants.ObjectTypes.Document);
            var entities = service.GetAll(objectTypeId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_Type()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll<IContent>();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Throws_When_Getting_All_With_Invalid_Type()
        {
            var service = ServiceContext.EntityService;
            var objectTypeId = new Guid(Constants.ObjectTypes.ContentItem);

            Assert.Throws<NotSupportedException>(() => service.GetAll<IContentBase>());
            Assert.Throws<NullReferenceException>(() => service.GetAll(UmbracoObjectTypes.ContentItem));
            Assert.Throws<NullReferenceException>(() => service.GetAll(objectTypeId));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectTypes()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll(UmbracoObjectTypes.DocumentType);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectType_Id()
        {
            var service = ServiceContext.EntityService;

            var objectTypeId = new Guid(Constants.ObjectTypes.DocumentType);
            var entities = service.GetAll(objectTypeId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_Type()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll<IContentType>();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }
    }
}