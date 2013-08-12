using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContentTypes
    {
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
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });

            var metaCollection = new PropertyTypeCollection();
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "keywords", Name = "Meta Keywords", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "metaDescription", Name = "Meta Description", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -89 });

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
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "metakeywords", Name = "Meta Keywords", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            metaCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "metadescription", Name = "Meta Description", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -89 });

            contentType.PropertyGroups.Add(new PropertyGroup(metaCollection) { Name = "Meta", SortOrder = 2 });

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
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            return contentType;
        }

		public static ContentType CreateSimpleContentType(string alias, string name, IContentType parent = null)
		{
			var contentType = parent == null ? new ContentType(-1) : new ContentType(parent);

			contentType.Alias = alias;
			contentType.Name = name;
			contentType.Description = "ContentType used for simple text pages";
			contentType.Icon = ".sprTreeDoc3";
			contentType.Thumbnail = "doc2.png";
			contentType.SortOrder = 1;
			contentType.CreatorId = 0;
			contentType.Trashed = false;
			
			var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea"), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid("5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83"), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea"), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

			contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

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
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = mandatory, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = mandatory, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = mandatory, SortOrder = 3, DataTypeDefinitionId = -88 });

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
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.TrueFalse), DataTypeDatabaseType.Integer) { Alias = "isTrue", Name = "Is True or False", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -49 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.Integer), DataTypeDatabaseType.Integer) { Alias = "number", Name = "Number", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -51 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.TinyMCEv3), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.Textbox), DataTypeDatabaseType.Nvarchar) { Alias = "singleLineText", Name = "Text String", Mandatory = false, SortOrder = 4, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.TextboxMultiple), DataTypeDatabaseType.Ntext) { Alias = "multilineText", Name = "Multiple Text Strings", Mandatory = false, SortOrder = 5, DataTypeDefinitionId = -89 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.UploadField), DataTypeDatabaseType.Nvarchar) { Alias = "upload", Name = "Upload Field", Mandatory = false, SortOrder = 6, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.NoEdit), DataTypeDatabaseType.Nvarchar) { Alias = "label", Name = "Label", Mandatory = false, SortOrder = 7, DataTypeDefinitionId = -92 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.DateTime), DataTypeDatabaseType.Date) { Alias = "dateTime", Name = "Date Time", Mandatory = false, SortOrder = 8, DataTypeDefinitionId = -36 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.ColorPicker), DataTypeDatabaseType.Nvarchar) { Alias = "colorPicker", Name = "Color Picker", Mandatory = false, SortOrder = 9, DataTypeDefinitionId = -37 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.FolderBrowser), DataTypeDatabaseType.Nvarchar) { Alias = "folderBrowser", Name = "Folder Browser", Mandatory = false, SortOrder = 10, DataTypeDefinitionId = -38 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.DropDownListMultiple), DataTypeDatabaseType.Nvarchar) { Alias = "ddlMultiple", Name = "Dropdown List Multiple", Mandatory = false, SortOrder = 11, DataTypeDefinitionId = -39 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.RadioButtonList), DataTypeDatabaseType.Nvarchar) { Alias = "rbList", Name = "Radio Button List", Mandatory = false, SortOrder = 12, DataTypeDefinitionId = -40 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.Date), DataTypeDatabaseType.Date) { Alias = "date", Name = "Date", Mandatory = false, SortOrder = 13, DataTypeDefinitionId = -41 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.DropDownList), DataTypeDatabaseType.Integer) { Alias = "ddl", Name = "Dropdown List", Mandatory = false, SortOrder = 14, DataTypeDefinitionId = -42 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.CheckBoxList), DataTypeDatabaseType.Nvarchar) { Alias = "chklist", Name = "Checkbox List", Mandatory = false, SortOrder = 15, DataTypeDefinitionId = -43 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.ContentPicker), DataTypeDatabaseType.Integer) { Alias = "contentPicker", Name = "Content Picker", Mandatory = false, SortOrder = 16, DataTypeDefinitionId = 1034 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.MediaPicker), DataTypeDatabaseType.Integer) { Alias = "mediaPicker", Name = "Media Picker", Mandatory = false, SortOrder = 17, DataTypeDefinitionId = 1035 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.MemberPicker), DataTypeDatabaseType.Integer) { Alias = "memberPicker", Name = "Member Picker", Mandatory = false, SortOrder = 18, DataTypeDefinitionId = 1036 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.UltraSimpleEditor), DataTypeDatabaseType.Ntext) { Alias = "simpleEditor", Name = "Ultra Simple Editor", Mandatory = false, SortOrder = 19, DataTypeDefinitionId = 1038 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.UltimatePicker), DataTypeDatabaseType.Ntext) { Alias = "ultimatePicker", Name = "Ultimate Picker", Mandatory = false, SortOrder = 20, DataTypeDefinitionId = 1039 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.RelatedLinks), DataTypeDatabaseType.Ntext) { Alias = "relatedLinks", Name = "Related Links", Mandatory = false, SortOrder = 21, DataTypeDefinitionId = 1040 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.Tags), DataTypeDatabaseType.Ntext) { Alias = "tags", Name = "Tags", Mandatory = false, SortOrder = 22, DataTypeDefinitionId = 1041 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.MacroContainer), DataTypeDatabaseType.Ntext) { Alias = "macroContainer", Name = "Macro Container", Mandatory = false, SortOrder = 23, DataTypeDefinitionId = 1042 });
            contentCollection.Add(new PropertyType(new Guid(Constants.PropertyEditors.ImageCropper), DataTypeDatabaseType.Ntext) { Alias = "imgCropper", Name = "Image Cropper", Mandatory = false, SortOrder = 24, DataTypeDefinitionId = 1043 });

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
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Nvarchar) { Alias = "videoFile", Name = "Video File", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });

            mediaType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Media", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            mediaType.ResetDirtyProperties(false);

            return mediaType;
        }

        public static MediaType CreateImageMediaType()
        {
            var mediaType = new MediaType(-1)
            {
                Alias = Constants.Conventions.MediaTypes.Image,
                Name = "Image",
                Description = "ContentType used for images",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Nvarchar) { Alias = Constants.Conventions.Media.File, Name = "File", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Width, Name = "Width", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Height, Name = "Height", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Bytes, Name = "Bytes", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Integer) { Alias = Constants.Conventions.Media.Extension, Name = "File Extension", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -90 });

            mediaType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Media", SortOrder = 1 });

            //ensure that nothing is marked as dirty
            mediaType.ResetDirtyProperties(false);

            return mediaType;
        }
    }
}