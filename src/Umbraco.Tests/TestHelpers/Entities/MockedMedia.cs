using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public static class MockedMedia
    {
        public static IMedia CreateMediaImage(IMediaType mediaType, int parentId)
        {
            var media = new Media("Test Image", parentId, mediaType)
            {
                CreatorId = 0
            };

            media.SetValue(Constants.Conventions.Media.File, "/media/test-image.png");
            media.SetValue(Constants.Conventions.Media.Width, "200");
            media.SetValue(Constants.Conventions.Media.Height, "200");
            media.SetValue(Constants.Conventions.Media.Bytes, "100");
            media.SetValue(Constants.Conventions.Media.Extension, "png");

            return media;
        }

        public static IMedia CreateMediaFile(IMediaType mediaType, int parentId)
        {
            var media = new Media("Test File", parentId, mediaType)
            {
                CreatorId = 0
            };

            media.SetValue(Constants.Conventions.Media.File, "/media/test-file.txt");
            media.SetValue(Constants.Conventions.Media.Bytes, "4");
            media.SetValue(Constants.Conventions.Media.Extension, "txt");

            return media;
        }

        public static IMedia CreateMediaImageWithCrop(IMediaType mediaType, int parentId)
        {
            var media = new Media("Test Image", parentId, mediaType)
            {
                CreatorId = 0
            };

            media.SetValue(Constants.Conventions.Media.File, "{src: '/media/test-image.png', crops: []}");
            media.SetValue(Constants.Conventions.Media.Width, "200");
            media.SetValue(Constants.Conventions.Media.Height, "200");
            media.SetValue(Constants.Conventions.Media.Bytes, "100");
            media.SetValue(Constants.Conventions.Media.Extension, "png");

            return media;
        }

        public static IMedia CreateMediaFolder(IMediaType mediaType, int parentId)
         {
             var media = new Media("Test Folder", parentId, mediaType)
                             {
                                 CreatorId = 0
                             };

             return media;
         }
    }
}