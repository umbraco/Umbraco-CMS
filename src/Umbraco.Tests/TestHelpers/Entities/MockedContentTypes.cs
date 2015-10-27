using System;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContentTypes
    {
        public static ContentType CreateBasicContentType(string alias = "basePage", string name = "Base Page",
            ContentType parent = null)
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

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

        public static ContentType CreateTextpageContentType(string alias = "textPage", string name = "Text Page")
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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "keywords", Name = "Meta Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            metaCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "description", Name = "Meta Description", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -89 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });
            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Meta", SortOrder = 2 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

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

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "metakeywords", Name = "Meta Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            metaCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "metadescription", Name = "Meta Description", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -89 });

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

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });

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
                Trashed = false
            };

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType("seotest", DataTypeDatabaseType.Ntext) { Alias = "seokeywords", Name = "Seo Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            metaCollection.Add(new PropertyType("seotest", DataTypeDatabaseType.Ntext) { Alias = "seodescription", Name = "Seo Description", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -89 });

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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author",  Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

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
			
			var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = RandomAlias("title", randomizeAliases), Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TinyMCEAlias, DataTypeDatabaseType.Ntext) { Alias = RandomAlias("bodyText", randomizeAliases), Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = RandomAlias("author", randomizeAliases) , Name = "Author", Description = "Name of the author",  Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = propertyGroupName, SortOrder = 1 });

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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = mandatory, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "",  Mandatory = mandatory, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author",  Mandatory = mandatory, SortOrder = 3, DataTypeDefinitionId = -88 });

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
            nonGroupedCollection.ForEach(pt => contentType.AddPropertyType(pt));

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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer) { Alias = "isTrue", Name = "Is True or False", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -49 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer) { Alias = "number", Name = "Number", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -51 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TinyMCEAlias, DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Nvarchar) { Alias = "singleLineText", Name = "Text String", Mandatory = false, SortOrder = 4, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxMultipleAlias, DataTypeDatabaseType.Ntext) { Alias = "multilineText", Name = "Multiple Text Strings", Mandatory = false, SortOrder = 5, DataTypeDefinitionId = -89 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.UploadFieldAlias, DataTypeDatabaseType.Nvarchar) { Alias = "upload", Name = "Upload Field", Mandatory = false, SortOrder = 6, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Nvarchar) { Alias = "label", Name = "Label", Mandatory = false, SortOrder = 7, DataTypeDefinitionId = -92 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.DateTimeAlias, DataTypeDatabaseType.Date) { Alias = "dateTime", Name = "Date Time", Mandatory = false, SortOrder = 8, DataTypeDefinitionId = -36 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.ColorPickerAlias, DataTypeDatabaseType.Nvarchar) { Alias = "colorPicker", Name = "Color Picker", Mandatory = false, SortOrder = 9, DataTypeDefinitionId = -37 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.FolderBrowserAlias, DataTypeDatabaseType.Nvarchar) { Alias = "folderBrowser", Name = "Folder Browser", Mandatory = false, SortOrder = 10, DataTypeDefinitionId = -38 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.DropDownListMultipleAlias, DataTypeDatabaseType.Nvarchar) { Alias = "ddlMultiple", Name = "Dropdown List Multiple", Mandatory = false, SortOrder = 11, DataTypeDefinitionId = -39 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.RadioButtonListAlias, DataTypeDatabaseType.Nvarchar) { Alias = "rbList", Name = "Radio Button List", Mandatory = false, SortOrder = 12, DataTypeDefinitionId = -40 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.DateAlias, DataTypeDatabaseType.Date) { Alias = "date", Name = "Date", Mandatory = false, SortOrder = 13, DataTypeDefinitionId = -41 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.DropDownListAlias, DataTypeDatabaseType.Integer) { Alias = "ddl", Name = "Dropdown List", Mandatory = false, SortOrder = 14, DataTypeDefinitionId = -42 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.CheckBoxListAlias, DataTypeDatabaseType.Nvarchar) { Alias = "chklist", Name = "Checkbox List", Mandatory = false, SortOrder = 15, DataTypeDefinitionId = -43 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.ContentPickerAlias, DataTypeDatabaseType.Integer) { Alias = "contentPicker", Name = "Content Picker", Mandatory = false, SortOrder = 16, DataTypeDefinitionId = 1034 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.MediaPickerAlias, DataTypeDatabaseType.Integer) { Alias = "mediaPicker", Name = "Media Picker", Mandatory = false, SortOrder = 17, DataTypeDefinitionId = 1035 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.MemberPickerAlias, DataTypeDatabaseType.Integer) { Alias = "memberPicker", Name = "Member Picker", Mandatory = false, SortOrder = 18, DataTypeDefinitionId = 1036 });            
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.RelatedLinksAlias, DataTypeDatabaseType.Ntext) { Alias = "relatedLinks", Name = "Related Links", Mandatory = false, SortOrder = 21, DataTypeDefinitionId = 1040 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TagsAlias, DataTypeDatabaseType.Ntext) { Alias = "tags", Name = "Tags", Mandatory = false, SortOrder = 22, DataTypeDefinitionId = 1041 });

            //contentCollection.Add(new PropertyType(Constants.PropertyEditors.UltraSimpleEditorAlias, DataTypeDatabaseType.Ntext) { Alias = "simpleEditor", Name = "Ultra Simple Editor", Mandatory = false, SortOrder = 19, DataTypeDefinitionId = 1038 });
            //contentCollection.Add(new PropertyType(Constants.PropertyEditors.UltimatePickerAlias, DataTypeDatabaseType.Ntext) { Alias = "ultimatePicker", Name = "Ultimate Picker", Mandatory = false, SortOrder = 20, DataTypeDefinitionId = 1039 });
            //contentCollection.Add(new PropertyType(Constants.PropertyEditors.MacroContainerAlias, DataTypeDatabaseType.Ntext) { Alias = "macroContainer", Name = "Macro Container", Mandatory = false, SortOrder = 23, DataTypeDefinitionId = 1042 });
            //contentCollection.Add(new PropertyType(Constants.PropertyEditors.ImageCropperAlias, DataTypeDatabaseType.Ntext) { Alias = "imgCropper", Name = "Image Cropper", Mandatory = false, SortOrder = 24, DataTypeDefinitionId = 1043 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

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
                                    CreatorId = 0,
                                    Trashed = false
                                };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Nvarchar) { Alias = "videoFile", Name = "Video File", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });

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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.UploadFieldAlias, DataTypeDatabaseType.Nvarchar) { Alias = Constants.Conventions.Media.File, Name = "File", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Width, Name = "Width", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Height, Name = "Height", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Bytes, Name = "Bytes", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Extension, Name = "File Extension", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });

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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
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