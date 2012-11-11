using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public static class MockedMedia
    {
         public static IMedia CreateMediaImage(IMediaType mediaType, int parentId)
         {
             var media = new Media(parentId, mediaType)
                             {
                                 Name = "Test Image",
                                 CreatorId = 0
                             };

             media.SetValue("umbracoFile", "/media/test-image.png");
             media.SetValue("umbracoWidth", "200");
             media.SetValue("umbracoHeight", "200");
             media.SetValue("umbracoBytes", "100");
             media.SetValue("umbracoExtension", "png");

             return media;
         }

         public static IMedia CreateMediaFile(IMediaType mediaType, int parentId)
         {
             var media = new Media(parentId, mediaType)
                             {
                                 Name = "Test File",
                                 CreatorId = 0
                             };

             media.SetValue("umbracoFile", "/media/test-file.txt");
             media.SetValue("umbracoBytes", "4");
             media.SetValue("umbracoExtension", "txt");

             return media;
         }

         public static IMedia CreateMediaFolder(IMediaType mediaType, int parentId)
         {
             var media = new Media(parentId, mediaType)
                             {
                                 Name = "Test Folder",
                                 CreatorId = 0
                             };

             return media;
         }
    }
}