using System;
using Umbraco.Core.Models;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContentTypes
    {
        public static ContentType CreateTextpageContentType()
        {
            var contentType = new ContentType(-1)
                                  {
                                      Alias = "textPage",
                                      Name = "Text Page",
                                      Description = "ContentType used for Text pages",
                                      Icon = ".sprTreeDoc3",
                                      Thumbnail = "doc.png",
                                      SortOrder = 1,
                                      UserId = 0,
                                      DefaultTemplate = "~/masterpages/umbTextPage.master",
                                      Trashed = false
                                  };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeId = -87 });

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "keywords", Name = "Meta Keywords", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "metaDescription", Name = "Meta Description", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeId = -89 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });
            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Meta", SortOrder = 2 });

            return contentType;
        }

        public static ContentType CreateMetaContentType()
        {
            var contentType = new ContentType(-1)
                                  {
                                      Alias = "meta",
                                      Name = "Meta",
                                      Description = "ContentType used for Meta tags",
                                      Icon = ".sprTreeDoc3",
                                      Thumbnail = "doc.png",
                                      SortOrder = 1,
                                      UserId = 0,
                                      DefaultTemplate = "",
                                      Trashed = false
                                  };

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "metakeywords", Name = "Meta Keywords", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "metadescription", Name = "Meta Description", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeId = -89 });

            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Meta", SortOrder = 2 });

            return contentType;
        }

        public static ContentType CreateSimpleContentType()
        {
            var contentType = new ContentType(-1)
                                  {
                                      Alias = "simple",
                                      Name = "Simple Page",
                                      Description = "ContentType used for simple text pages",
                                      Icon = ".sprTreeDoc3",
                                      Thumbnail = "doc.png",
                                      SortOrder = 1,
                                      UserId = 0,
                                      DefaultTemplate = "~/masterpages/umbSimplePage.master",
                                      Trashed = false
                                  };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

        public static ContentType CreateSimpleContentType(string alias, string name)
        {
            var contentType = new ContentType(-1)
                                  {
                                      Alias = alias,
                                      Name = name,
                                      Description = "ContentType used for simple text pages",
                                      Icon = ".sprTreeDoc3",
                                      Thumbnail = "doc2.png",
                                      SortOrder = 1,
                                      UserId = 0,
                                      DefaultTemplate = "~/masterpages/umbSimplePage.master",
                                      Trashed = false
                                  };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

        public static ContentType CreateSimpleContentType(string alias, string name, bool mandatory)
        {
            var contentType = new ContentType(-1)
            {
                Alias = alias,
                Name = name,
                Description = "ContentType used for simple text pages",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc2.png",
                SortOrder = 1,
                UserId = 0,
                DefaultTemplate = "~/masterpages/umbSimplePage.master",
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = mandatory, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = mandatory, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = mandatory, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

        public static ContentType CreateSimpleContentType(string alias, string name, PropertyTypeCollection collection)
        {
            var contentType = new ContentType(-1)
                                  {
                                      Alias = alias,
                                      Name = name,
                                      Description = "ContentType used for simple text pages",
                                      Icon = ".sprTreeDoc3",
                                      Thumbnail = "doc3.png",
                                      SortOrder = 1,
                                      UserId = 0,
                                      DefaultTemplate = "~/masterpages/umbSimplePage.master",
                                      Trashed = false
                                  };

            contentType.PropertyGroups.Add(new PropertyGroup(collection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

        public static MediaType CreateVideoMediaType()
        {
            var mediaType = new MediaType(-1)
                                {
                                    Alias = "video",
                                    Name = "Video",
                                    Description = "ContentType used for videos",
                                    Icon = ".sprTreeDoc3",
                                    Thumbnail = "doc.png",
                                    SortOrder = 1,
                                    UserId = 0,
                                    Trashed = false
                                };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Nvarchar) { Alias = "videoFile", Name = "Video File", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeId = -90 });

            mediaType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Media", SortOrder = 1 });

            return mediaType;
        }
    }
}