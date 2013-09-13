using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
	[TestFixture, RequiresSTA]
	public abstract class BaseServiceTest : BaseDatabaseFactoryTest
	{
		[SetUp]
		public override void Initialize()
		{        
			base.Initialize();

			CreateTestData();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
		}

		public virtual void CreateTestData()
		{
			//NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.

			//Create and Save ContentType "umbTextpage" -> 1045
			ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
			contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
			ServiceContext.ContentTypeService.Save(contentType);

			//Create and Save Content "Homepage" based on "umbTextpage" -> 1046
			Content textpage = MockedContent.CreateSimpleContent(contentType);
			textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
			ServiceContext.ContentService.Save(textpage, 0);

			//Create and Save Content "Text Page 1" based on "umbTextpage" -> 1047
			Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
			subpage.ReleaseDate = DateTime.Now.AddMinutes(-5);
            subpage.ChangePublishedState(PublishedState.Saved);
			ServiceContext.ContentService.Save(subpage, 0);

			//Create and Save Content "Text Page 1" based on "umbTextpage" -> 1048
			Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
			ServiceContext.ContentService.Save(subpage2, 0);

			//Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1049
			Content trashed = MockedContent.CreateSimpleContent(contentType, "Text Page Deleted", -20);
			trashed.Trashed = true;
			ServiceContext.ContentService.Save(trashed, 0);

            //Create and Save folder-Media -> 1050
            var folderMediaType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
            ServiceContext.MediaService.Save(folder);

            //Create and Save folder-Media -> 1051
            var folder2 = MockedMedia.CreateMediaFolder(folderMediaType, -1);
            ServiceContext.MediaService.Save(folder2);

            //Create and Save image-Media  -> 1052
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);
            var image = (Media)MockedMedia.CreateMediaImage(imageMediaType, 1050);
            ServiceContext.MediaService.Save(image);

            //Create and Save folder-Media that is trashed -> 1053
            var folderTrashed = (Media)MockedMedia.CreateMediaFolder(folderMediaType, -21);
            folderTrashed.Trashed = true;
            ServiceContext.MediaService.Save(folderTrashed);

            //Create and Save image-Media child of folderTrashed -> 1054            
            var imageTrashed = (Media)MockedMedia.CreateMediaImage(imageMediaType, folderTrashed.Id);
		    imageTrashed.Trashed = true;
            ServiceContext.MediaService.Save(imageTrashed);
		}
	}
}