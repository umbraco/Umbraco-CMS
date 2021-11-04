using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContentTypes
    {
        public static ContentType CreateBasicContentType(string alias = "basePage", string name = "Base Page", IContentType parent = null)
        {
            var contentType = parent == null ? new ContentType(-1) : new ContentType(parent, alias);

            contentType.Alias = alias;
            contentType.Name = name;
            contentType.Description = "ContentType used for basic pages";
            contentType.Icon = ".sprTreeDoc3";
            contentType.Thumbnail = "doc2.png";
            contentType.SortOrder = 1;
            contentType.CreatorId = 0;
            contentType.Trashed = false;
            contentType.HistoryCleanup = new HistoryCleanup();

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateTextPageContentType(string alias = "textPage", string name = "Text Page")
        {
            var contentType = new ContentType(-1)
                                  {
                                      Alias = alias,
                                      Name = name,
                                      Description = "ContentType used for Text pages",
                                      Icon = ".sprTreeDoc3",
                                      Thumbnail = "doc.png",
                                      SortOrder = 1,
                                      CreatorId = 0,
                                      Trashed = false
                                  };

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = Constants.DataTypes.Textbox, LabelOnTop = true });
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.DataTypes.RichtextEditor, LabelOnTop = false });

            var metaCollection = new PropertyTypeCollection(true);
            metaCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "keywords", Name = "Meta Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = Constants.DataTypes.Textbox });
            metaCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "description", Name = "Meta Description", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.DataTypes.Textarea });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });
            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Meta", SortOrder = 2 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            contentType.SetDefaultTemplate(new Template("Textpage", "textpage"));

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
                                      CreatorId = 0,
                                      Trashed = false
                                  };

            var metaCollection = new PropertyTypeCollection(true);
            metaCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "metakeywords", Name = "Meta Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            metaCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "metadescription", Name = "Meta Description", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = -89 });

            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Meta", SortOrder = 2 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateContentMetaContentType()
        {
            var contentType = new ContentType(-1)
            {
                Alias = "contentMeta",
                Name = "Content Meta",
                Description = "ContentType used for Content Meta",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false
            };

            var metaCollection = new PropertyTypeCollection(true);
            metaCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Content", SortOrder = 2 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateSeoContentType()
        {
            var contentType = new ContentType(-1)
            {
                Alias = "seo",
                Name = "Seo",
                Description = "ContentType used for Seo",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false,
                HistoryCleanup = new HistoryCleanup()
            };

            var metaCollection = new PropertyTypeCollection(true);
            metaCollection.Add(new PropertyType("seotest", ValueStorageType.Ntext) { Alias = "seokeywords", Name = "Seo Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            metaCollection.Add(new PropertyType("seotest", ValueStorageType.Ntext) { Alias = "seodescription", Name = "Seo Description", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = -89 });

            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Seo", SortOrder = 5 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

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
                                      CreatorId = 0,
                                      Trashed = false
                                  };

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TinyMce, ValueStorageType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author",  Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection)
            {
                Name = "Content",
                Alias = "content",
                SortOrder = 1
            });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateSimpleContentType3(string alias, string name, IContentType parent = null, bool randomizeAliases = false, string propertyGroupName = "Content")
        {
            var contentType = CreateSimpleContentType(alias, name, parent, randomizeAliases, propertyGroupName);

            var propertyType = new PropertyType(Constants.PropertyEditors.Aliases.Tags, ValueStorageType.Nvarchar)
            {
                Alias = RandomAlias("tags", randomizeAliases),
                Name = "Tags",
                Description = "Tags",
                Mandatory = false,
                SortOrder = 99,
                DataTypeId = Constants.DataTypes.Tags
            };
            contentType.AddPropertyType(propertyType);

            return contentType;
        }

        public static ContentType CreateSimpleContentType2(string alias, string name, IContentType parent = null, bool randomizeAliases = false, string propertyGroupName = "Content")
        {
            var contentType = CreateSimpleContentType(alias, name, parent, randomizeAliases, propertyGroupName);

            var propertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = RandomAlias("gen", randomizeAliases),
                Name = "Gen",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
            contentType.AddPropertyType(propertyType);

            return contentType;
        }

        public static ContentType CreateSimpleContentType(string alias, string name, IContentType parent = null, bool randomizeAliases = false, string propertyGroupName = "Content")
        {
            var contentType = parent == null ? new ContentType(-1) : new ContentType(parent, alias);

            contentType.Alias = alias;
            contentType.Name = name;
            contentType.Description = "ContentType used for simple text pages";
            contentType.Icon = ".sprTreeDoc3";
            contentType.Thumbnail = "doc2.png";
            contentType.SortOrder = 1;
            contentType.CreatorId = 0;
            contentType.Trashed = false;
            contentType.Key = Guid.NewGuid();

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = RandomAlias("title", randomizeAliases), Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88, LabelOnTop = true });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TinyMce, ValueStorageType.Ntext) { Alias = RandomAlias("bodyText", randomizeAliases), Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = RandomAlias("author", randomizeAliases) , Name = "Author", Description = "Name of the author",  Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            var pg = new PropertyGroup(contentCollection)
            {
                Name = propertyGroupName,
                SortOrder = 1
            };
            contentType.PropertyGroups.Add(pg);

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            contentType.SetDefaultTemplate(new Template("Textpage", "textpage"));

            return contentType;
        }

        public static MediaType CreateSimpleMediaType(string alias, string name, IMediaType parent = null, bool randomizeAliases = false, string propertyGroupName = "Content")
        {
            var contentType = parent == null ? new MediaType(-1) : new MediaType(parent, alias);

            contentType.Alias = alias;
            contentType.Name = name;
            contentType.Description = "ContentType used for simple text pages";
            contentType.Icon = ".sprTreeDoc3";
            contentType.Thumbnail = "doc2.png";
            contentType.SortOrder = 1;
            contentType.CreatorId = 0;
            contentType.Trashed = false;

            var contentCollection = new PropertyTypeCollection(false);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = RandomAlias("title", randomizeAliases), Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TinyMce, ValueStorageType.Ntext) { Alias = RandomAlias("bodyText", randomizeAliases), Name = "Body Text", Description = "", Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = RandomAlias("author", randomizeAliases), Name = "Author", Description = "Name of the author", Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            var pg = new PropertyGroup(contentCollection) { Name = propertyGroupName, SortOrder = 1 };
            contentType.PropertyGroups.Add(pg);

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

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
                CreatorId = 0,
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = mandatory, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "",  Mandatory = mandatory, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author",  Mandatory = mandatory, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

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
                                      CreatorId = 0,
                                      Trashed = false
                                  };

            contentType.PropertyGroups.Add(new PropertyGroup(collection) { Name = "Content", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateSimpleContentType(string alias, string name, PropertyTypeCollection collection, string propertyGroupName, IContentType parent = null)
        {
            var contentType = parent == null ? new ContentType(-1) : new ContentType(parent, alias);

            contentType.Alias = alias;
            contentType.Name = name;
            contentType.Description = "ContentType used for simple text pages";
            contentType.Icon = ".sprTreeDoc3";
            contentType.Thumbnail = "doc2.png";
            contentType.SortOrder = 1;
            contentType.CreatorId = 0;
            contentType.Trashed = false;

            contentType.PropertyGroups.Add(new PropertyGroup(collection) { Name = propertyGroupName, SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateSimpleContentType(string alias, string name, PropertyTypeCollection groupedCollection, PropertyTypeCollection nonGroupedCollection)
        {
            var contentType = CreateSimpleContentType(alias, name, groupedCollection);
            //now add the non-grouped properties
            foreach (var x in nonGroupedCollection)
                contentType.AddPropertyType(x);

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateAllTypesContentType(string alias, string name)
        {
            var contentType = new ContentType(-1)
                              {
                                  Alias = alias,
                                  Name = name,
                                  Description = "ContentType containing all standard DataTypes",
                                  Icon = ".sprTreeDoc3",
                                  Thumbnail = "doc.png",
                                  SortOrder = 1,
                                  CreatorId = 0,
                                  Trashed = false
                              };

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Boolean, ValueStorageType.Integer) { Alias = "isTrue", Name = "Is True or False", Mandatory = false, SortOrder = 1, DataTypeId = -49 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Integer, ValueStorageType.Integer) { Alias = "number", Name = "Number", Mandatory = false, SortOrder = 2, DataTypeId = -51 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TinyMce, ValueStorageType.Ntext) { Alias = "bodyText", Name = "Body Text", Mandatory = false, SortOrder = 3, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar) { Alias = "singleLineText", Name = "Text String", Mandatory = false, SortOrder = 4, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextArea, ValueStorageType.Ntext) { Alias = "multilineText", Name = "Multiple Text Strings", Mandatory = false, SortOrder = 5, DataTypeId = -89 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.UploadField, ValueStorageType.Nvarchar) { Alias = "upload", Name = "Upload Field", Mandatory = false, SortOrder = 6, DataTypeId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar) { Alias = "label", Name = "Label", Mandatory = false, SortOrder = 7, DataTypeId = -92 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.DateTime, ValueStorageType.Date) { Alias = "dateTime", Name = "Date Time", Mandatory = false, SortOrder = 8, DataTypeId = -36 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.ColorPicker, ValueStorageType.Nvarchar) { Alias = "colorPicker", Name = "Color Picker", Mandatory = false, SortOrder = 9, DataTypeId = -37 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.DropDownListFlexible, ValueStorageType.Nvarchar) { Alias = "ddlMultiple", Name = "Dropdown List Multiple", Mandatory = false, SortOrder = 11, DataTypeId = -39 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.RadioButtonList, ValueStorageType.Nvarchar) { Alias = "rbList", Name = "Radio Button List", Mandatory = false, SortOrder = 12, DataTypeId = -40 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.DateTime, ValueStorageType.Date) { Alias = "date", Name = "Date", Mandatory = false, SortOrder = 13, DataTypeId = -36 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.DropDownListFlexible, ValueStorageType.Integer) { Alias = "ddl", Name = "Dropdown List", Mandatory = false, SortOrder = 14, DataTypeId = -42 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.CheckBoxList, ValueStorageType.Nvarchar) { Alias = "chklist", Name = "Checkbox List", Mandatory = false, SortOrder = 15, DataTypeId = -43 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.ContentPicker, ValueStorageType.Integer) { Alias = "contentPicker", Name = "Content Picker", Mandatory = false, SortOrder = 16, DataTypeId = 1046 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.MediaPicker3, ValueStorageType.Integer) { Alias = "mediapicker3", Name = "Media Picker", Mandatory = false, SortOrder = 17, DataTypeId = 1051 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.MemberPicker, ValueStorageType.Integer) { Alias = "memberPicker", Name = "Member Picker", Mandatory = false, SortOrder = 18, DataTypeId = 1047 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.MultiUrlPicker, ValueStorageType.Nvarchar) { Alias = "multiUrlPicker", Name = "Multi URL Picker", Mandatory = false, SortOrder = 21, DataTypeId = 1050 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Tags, ValueStorageType.Ntext) { Alias = "tags", Name = "Tags", Mandatory = false, SortOrder = 22, DataTypeId = 1041 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

        public static MediaType CreateNewMediaType()
        {
            var mediaType = new MediaType(-1)
                                {
                                    Alias = "newMediaType",
                                    Name = "New Media Type",
                                    Description = "ContentType used for a new format",
                                    Icon = ".sprTreeDoc3",
                                    Thumbnail = "doc.png",
                                    SortOrder = 1,
                                    CreatorId = 0,
                                    Trashed = false
                                };

            var contentCollection = new PropertyTypeCollection(false);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType("test", ValueStorageType.Nvarchar) { Alias = "videoFile", Name = "Video File", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = -90 });

            mediaType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Media", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            mediaType.ResetDirtyProperties(false);

            return mediaType;
        }

        public static MediaType CreateImageMediaType(string alias = Constants.Conventions.MediaTypes.Image)
        {
            var mediaType = new MediaType(-1)
            {
                Alias = alias,
                Name = "Image",
                Description = "ContentType used for images",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection(false);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.UploadField, ValueStorageType.Nvarchar) { Alias = Constants.Conventions.Media.File, Name = "File", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer) { Alias = Constants.Conventions.Media.Width, Name = "Width", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer) { Alias = Constants.Conventions.Media.Height, Name = "Height", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer) { Alias = Constants.Conventions.Media.Bytes, Name = "Bytes", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar) { Alias = Constants.Conventions.Media.Extension, Name = "File Extension", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });

            mediaType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Media", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            mediaType.ResetDirtyProperties(false);

            return mediaType;
        }

        public static MediaType CreateImageMediaTypeWithCrop(string alias = Constants.Conventions.MediaTypes.Image)
        {
            var mediaType = new MediaType(-1)
            {
                Alias = alias,
                Name = "Image",
                Description = "ContentType used for images",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection(false);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.ImageCropper, ValueStorageType.Ntext) { Alias = Constants.Conventions.Media.File, Name = "File", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = 1043 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer) { Alias = Constants.Conventions.Media.Width, Name = "Width", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer) { Alias = Constants.Conventions.Media.Height, Name = "Height", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer) { Alias = Constants.Conventions.Media.Bytes, Name = "Bytes", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar) { Alias = Constants.Conventions.Media.Extension, Name = "File Extension", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = Constants.System.DefaultLabelDataTypeId });

            mediaType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Media", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            mediaType.ResetDirtyProperties(false);

            return mediaType;
        }

        public static MemberType CreateSimpleMemberType(string alias = null, string name = null)
        {
            var contentType = new MemberType(-1)
            {
                Alias = alias ?? "simple",
                Name = name ?? "Simple Page",
                Description = "Some member type",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection(false);
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static void EnsureAllIds(ContentTypeCompositionBase contentType, int seedId)
        {
            //ensure everything has ids
            contentType.Id = seedId;
            var itemid = seedId + 1;
            foreach (var propertyGroup in contentType.PropertyGroups)
            {
                propertyGroup.Id = itemid++;
            }
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = itemid++;
            }
        }


        private static string RandomAlias(string alias, bool randomizeAliases)
        {
            if (randomizeAliases)
            {
                return string.Concat(alias, Guid.NewGuid().ToString("N"));
            }

            return alias;
        }
    }
}
