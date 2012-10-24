using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Services;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class ContentServiceTests : BaseDatabaseFactoryTest
    {
        /*[Test]*/
        public void Can_Create_Content()
        {
            // Arrange
            var contentService = new ContentService();

            // Act
            IContent content = contentService.CreateContent(-1, "umbTextpage");

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.HasIdentity, Is.False);
        }

        /*[Test]*/
        public void Cannot_Create_Content_With_Non_Existing_ContentType_Alias()
        {
            // Arrange
            var contentService = new ContentService();

            // Act & Assert
            Assert.Throws<Exception>(() => contentService.CreateContent(-1, "umbAliasDoesntExist"));
        }

        public void Can_Get_Content_By_Id()
        { }

        public void Can_Get_Content_By_Level()
        { }

        public void Can_Get_Children_Of_Content_Id()
        { }

        public void Can_Get_All_Versions_Of_Content()
        { }

        public void Can_Get_Root_Content()
        { }

        public void Can_Get_Content_For_Expiration()
        { }

        public void Can_Get_Content_For_Release()
        { }

        public void Can_Get_Content_In_RecycleBin()
        { }

        public void Can_RePublish_All_Content()
        { }

        public void Can_Publish_Content()
        { }

        public void Can_Publish_Content_Children()
        { }

        public void Can_Save_And_Publish_Content()
        { }

        public void Can_Save_Content()
        { }

        public void Can_Bulk_Save_Content()
        { }

        public void Can_Delete_Content_Of_Specific_ContentType()
        { }

        public void Can_Delete_Content()
        { }

        public void Can_Move_Content_To_RecycleBin()
        { }

        public void Can_Move_Content()
        { }

        public void Can_Copy_Content()
        { }

        public void Can_Send_To_Publication()
        { }

        public void Can_Rollback_Version_On_Content()
        { }
    }
}