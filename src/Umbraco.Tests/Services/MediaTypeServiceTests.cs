using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class MediaTypeServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void Empty_Description_Is_Always_Null_After_Saving_Media_Type()
        {
            var mediaType = MockedContentTypes.CreateSimpleMediaType("mediaType", "Media Type");
            mediaType.Description = null;
            ServiceContext.MediaTypeService.Save(mediaType);

            var mediaType2 = MockedContentTypes.CreateSimpleMediaType("mediaType2", "Media Type 2");
            mediaType2.Description = string.Empty;
            ServiceContext.MediaTypeService.Save(mediaType2);

            Assert.IsNull(mediaType.Description);
            Assert.IsNull(mediaType2.Description);
        }

        [Test]
        public void Deleting_Media_Type_With_Hierarchy_Of_Media_Items_Moves_Orphaned_Media_To_Recycle_Bin()
        {
            IMediaType contentType1 = MockedContentTypes.CreateSimpleMediaType("test1", "Test1");
            ServiceContext.MediaTypeService.Save(contentType1);
            IMediaType contentType2 = MockedContentTypes.CreateSimpleMediaType("test2", "Test2");
            ServiceContext.MediaTypeService.Save(contentType2);
            IMediaType contentType3 = MockedContentTypes.CreateSimpleMediaType("test3", "Test3");
            ServiceContext.MediaTypeService.Save(contentType3);

            var contentTypes = new[] { contentType1, contentType2, contentType3 };
            var parentId = -1;

            var ids = new List<int>();

            for (int i = 0; i < 2; i++)
            {
                for (var index = 0; index < contentTypes.Length; index++)
                {
                    var contentType = contentTypes[index];
                    var contentItem = MockedMedia.CreateSimpleMedia(contentType, "MyName_" + index + "_" + i, parentId);
                    ServiceContext.MediaService.Save(contentItem);
                    parentId = contentItem.Id;

                    ids.Add(contentItem.Id);
                }
            }

            //delete the first content type, all other content of different content types should be in the recycle bin
            ServiceContext.MediaTypeService.Delete(contentTypes[0]);

            var found = ServiceContext.MediaService.GetByIds(ids);

            Assert.AreEqual(4, found.Count());
            foreach (var content in found)
            {
                Assert.IsTrue(content.Trashed);
            }
        }

        [Test]
        public void Deleting_Media_Types_With_Hierarchy_Of_Media_Items_Doesnt_Raise_Trashed_Event_For_Deleted_Items()
        {
            MediaService.Trashed += MediaServiceOnTrashed;

            try
            {
                IMediaType contentType1 = MockedContentTypes.CreateSimpleMediaType("test1", "Test1");
                ServiceContext.MediaTypeService.Save(contentType1);
                IMediaType contentType2 = MockedContentTypes.CreateSimpleMediaType("test2", "Test2");
                ServiceContext.MediaTypeService.Save(contentType2);
                IMediaType contentType3 = MockedContentTypes.CreateSimpleMediaType("test3", "Test3");
                ServiceContext.MediaTypeService.Save(contentType3);

                var contentTypes = new[] { contentType1, contentType2, contentType3 };
                var parentId = -1;

                var ids = new List<int>();

                for (int i = 0; i < 2; i++)
                {
                    for (var index = 0; index < contentTypes.Length; index++)
                    {
                        var contentType = contentTypes[index];
                        var contentItem = MockedMedia.CreateSimpleMedia(contentType, "MyName_" + index + "_" + i, parentId);
                        ServiceContext.MediaService.Save(contentItem);
                        parentId = contentItem.Id;

                        ids.Add(contentItem.Id);
                    }
                }

                foreach (var contentType in contentTypes.Reverse())
                {
                    ServiceContext.MediaTypeService.Delete(contentType);
                }
            }
            finally
            {
                MediaService.Trashed -= MediaServiceOnTrashed;
            }
        }

        private void MediaServiceOnTrashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            foreach (var item in e.MoveInfoCollection)
            {
                //if this item doesn't exist then Fail!
                var exists = ServiceContext.MediaService.GetById(item.Entity.Id);
                if (exists == null)
                    Assert.Fail("The item doesn't exist");
            }
        }

        [Test]
        public void Can_Copy_MediaType_By_Performing_Clone()
        {
            // Arrange
            var mediaType = MockedContentTypes.CreateImageMediaType("Image2") as IMediaType;
            ServiceContext.MediaTypeService.Save(mediaType);

            // Act
            var sut = mediaType.DeepCloneWithResetIdentities("Image2_2");
            Assert.IsNotNull(sut);
            ServiceContext.MediaTypeService.Save(sut);

            // Assert
            Assert.That(sut.HasIdentity, Is.True);
            Assert.AreEqual(mediaType.ParentId, sut.ParentId);
            Assert.AreEqual(mediaType.Level, sut.Level);
            Assert.AreEqual(mediaType.PropertyTypes.Count(), sut.PropertyTypes.Count());
            Assert.AreNotEqual(mediaType.Id, sut.Id);
            Assert.AreNotEqual(mediaType.Key, sut.Key);
            Assert.AreNotEqual(mediaType.Path, sut.Path);
            Assert.AreNotEqual(mediaType.SortOrder, sut.SortOrder);
            Assert.AreNotEqual(mediaType.PropertyTypes.First(x => x.Alias.Equals("umbracoFile")).Id, sut.PropertyTypes.First(x => x.Alias.Equals("umbracoFile")).Id);
            Assert.AreNotEqual(mediaType.PropertyGroups.First(x => x.Name.Equals("Media")).Id, sut.PropertyGroups.First(x => x.Name.Equals("Media")).Id);
        }

        [Test]
        public void Can_Copy_MediaType_To_New_Parent_By_Performing_Clone()
        {
            // Arrange
            var parentMediaType1 = MockedContentTypes.CreateSimpleMediaType("parent1", "Parent1");
            ServiceContext.MediaTypeService.Save(parentMediaType1);
            var parentMediaType2 = MockedContentTypes.CreateSimpleMediaType("parent2", "Parent2", null, true);
            ServiceContext.MediaTypeService.Save(parentMediaType2);
            var mediaType = MockedContentTypes.CreateImageMediaType("Image2") as IMediaType;
            ServiceContext.MediaTypeService.Save(mediaType);

            // Act
            var clone = mediaType.DeepCloneWithResetIdentities("newcategory");
            Assert.IsNotNull(clone);
            clone.RemoveContentType("parent1");
            clone.AddContentType(parentMediaType2);
            clone.ParentId = parentMediaType2.Id;
            ServiceContext.MediaTypeService.Save(clone);

            // Assert
            Assert.That(clone.HasIdentity, Is.True);

            var clonedMediaType = ServiceContext.MediaTypeService.Get(clone.Id);
            var originalMediaType = ServiceContext.MediaTypeService.Get(mediaType.Id);

            Assert.That(clonedMediaType.CompositionAliases().Any(x => x.Equals("parent2")), Is.True);
            Assert.That(clonedMediaType.CompositionAliases().Any(x => x.Equals("parent1")), Is.False);

            Assert.AreEqual(clonedMediaType.Path, "-1," + parentMediaType2.Id + "," + clonedMediaType.Id);
            Assert.AreEqual(clonedMediaType.PropertyTypes.Count(), originalMediaType.PropertyTypes.Count());

            Assert.AreNotEqual(clonedMediaType.ParentId, originalMediaType.ParentId);
            Assert.AreEqual(clonedMediaType.ParentId, parentMediaType2.Id);

            Assert.AreNotEqual(clonedMediaType.Id, originalMediaType.Id);
            Assert.AreNotEqual(clonedMediaType.Key, originalMediaType.Key);
            Assert.AreNotEqual(clonedMediaType.Path, originalMediaType.Path);

            Assert.AreNotEqual(clonedMediaType.PropertyTypes.First(x => x.Alias.StartsWith("umbracoFile")).Id, originalMediaType.PropertyTypes.First(x => x.Alias.StartsWith("umbracoFile")).Id);
            Assert.AreNotEqual(clonedMediaType.PropertyGroups.First(x => x.Name.StartsWith("Media")).Id, originalMediaType.PropertyGroups.First(x => x.Name.StartsWith("Media")).Id);
        }
    }
}
