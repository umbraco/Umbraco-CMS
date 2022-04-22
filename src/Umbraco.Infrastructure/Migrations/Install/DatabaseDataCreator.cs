using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Install
{
    /// <summary>
    /// Creates the initial database data during install.
    /// </summary>
    internal class DatabaseDataCreator
    {
        private readonly IDatabase _database;
        private readonly ILogger<DatabaseDataCreator> _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IOptionsMonitor<InstallDefaultDataSettings> _installDefaultDataSettings;

        private readonly IDictionary<string, IList<string>> _entitiesToAlwaysCreate = new Dictionary<string, IList<string>>()
            {
                {
                    Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                    new List<string>
                    {
                        Cms.Core.Constants.DataTypes.Guids.LabelString,
                    }
                }
            };

        public DatabaseDataCreator(IDatabase database, ILogger<DatabaseDataCreator> logger, IUmbracoVersion umbracoVersion, IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
        {
            _database = database;
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _installDefaultDataSettings = installDefaultDataSettings;
        }

        /// <summary>
        /// Initialize the base data creation by inserting the data foundation for umbraco
        /// specific to a table
        /// </summary>
        /// <param name="tableName">Name of the table to create base data for</param>
        public void InitializeBaseData(string tableName)
        {
            _logger.LogInformation("Creating data in {TableName}", tableName);

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.Node))
            {
                CreateNodeData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.Lock))
            {
                CreateLockData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.ContentType))
            {
                CreateContentTypeData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.User))
            {
                CreateUserData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup))
            {
                CreateUserGroupData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.User2UserGroup))
            {
                CreateUser2UserGroupData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2App))
            {
                CreateUserGroup2AppData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup))
            {
                CreatePropertyTypeGroupData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType))
            {
                CreatePropertyTypeData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.Language))
            {
                CreateLanguageData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.ContentChildType))
            {
                CreateContentChildTypeData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.DataType))
            {
                CreateDataTypeData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.RelationType))
            {
                CreateRelationTypeData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.KeyValue))
            {
                CreateKeyValueData();
            }

            if (tableName.Equals(Cms.Core.Constants.DatabaseSchema.Tables.LogViewerQuery))
            {
                CreateLogViewerQueryData();
            }

            _logger.LogInformation("Completed creating data in {TableName}", tableName);
        }

        private void CreateNodeData()
        {
            CreateNodeDataForDataTypes();
            CreateNodeDataForMediaTypes();
            CreateNodeDataForMemberTypes();
        }

        private void CreateNodeDataForDataTypes()
        {
            void InsertDataTypeNodeDto(int id, int sortOrder, string uniqueId, string text)
            {
                var nodeDto = new NodeDto
                {
                    NodeId = id,
                    Trashed = false,
                    ParentId = -1,
                    UserId = -1,
                    Level = 1,
                    Path = "-1," + id,
                    SortOrder = sortOrder,
                    UniqueId = new Guid(uniqueId),
                    Text = text,
                    NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType,
                    CreateDate = DateTime.Now,
                };

                ConditionalInsert(
                    Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                    uniqueId,
                    nodeDto,
                    Cms.Core.Constants.DatabaseSchema.Tables.Node,
                    "id");
            }

            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -1, Trashed = false, ParentId = -1, UserId = -1, Level = 0, Path = "-1", SortOrder = 0, UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"), Text = "SYSTEM DATA: umbraco master root", NodeObjectType = Cms.Core.Constants.ObjectTypes.SystemRoot, CreateDate = DateTime.Now });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -20, Trashed = false, ParentId = -1, UserId = -1, Level = 0, Path = "-1,-20", SortOrder = 0, UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"), Text = "Recycle Bin", NodeObjectType = Cms.Core.Constants.ObjectTypes.ContentRecycleBin, CreateDate = DateTime.Now });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -21, Trashed = false, ParentId = -1, UserId = -1, Level = 0, Path = "-1,-21", SortOrder = 0, UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"), Text = "Recycle Bin", NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaRecycleBin, CreateDate = DateTime.Now });

            InsertDataTypeNodeDto(Cms.Core.Constants.DataTypes.LabelString, 35, Cms.Core.Constants.DataTypes.Guids.LabelString, "Label (string)");
            InsertDataTypeNodeDto(Cms.Core.Constants.DataTypes.LabelInt, 36, Cms.Core.Constants.DataTypes.Guids.LabelInt, "Label (integer)");
            InsertDataTypeNodeDto(Cms.Core.Constants.DataTypes.LabelBigint, 36, Cms.Core.Constants.DataTypes.Guids.LabelBigInt, "Label (bigint)");
            InsertDataTypeNodeDto(Cms.Core.Constants.DataTypes.LabelDateTime, 37, Cms.Core.Constants.DataTypes.Guids.LabelDateTime, "Label (datetime)");
            InsertDataTypeNodeDto(Cms.Core.Constants.DataTypes.LabelTime, 38, Cms.Core.Constants.DataTypes.Guids.LabelTime, "Label (time)");
            InsertDataTypeNodeDto(Cms.Core.Constants.DataTypes.LabelDecimal, 39, Cms.Core.Constants.DataTypes.Guids.LabelDecimal, "Label (decimal)");

            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Upload,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.Upload, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.Upload}", SortOrder = 34, UniqueId = Cms.Core.Constants.DataTypes.Guids.UploadGuid, Text = "Upload File", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.UploadVideo,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.UploadVideo, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.UploadVideo}", SortOrder = 35, UniqueId = Cms.Core.Constants.DataTypes.Guids.UploadVideoGuid, Text = "Upload Video", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.UploadAudio,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.UploadAudio, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.UploadAudio}", SortOrder = 36, UniqueId = Cms.Core.Constants.DataTypes.Guids.UploadAudioGuid, Text = "Upload Audio", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.UploadArticle,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.UploadArticle, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.UploadArticle}", SortOrder = 37, UniqueId = Cms.Core.Constants.DataTypes.Guids.UploadArticleGuid, Text = "Upload Article", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.UploadVectorGraphics,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.UploadVectorGraphics, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.UploadVectorGraphics}", SortOrder = 38, UniqueId = Cms.Core.Constants.DataTypes.Guids.UploadVectorGraphicsGuid, Text = "Upload Vector Graphics", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Textarea,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.Textarea, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.Textarea}", SortOrder = 33, UniqueId = Cms.Core.Constants.DataTypes.Guids.TextareaGuid, Text = "Textarea", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Textstring,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.Textbox, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.Textbox}", SortOrder = 32, UniqueId = Cms.Core.Constants.DataTypes.Guids.TextstringGuid, Text = "Textstring", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.RichtextEditor,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.RichtextEditor, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.RichtextEditor}", SortOrder = 4, UniqueId = Cms.Core.Constants.DataTypes.Guids.RichtextEditorGuid, Text = "Richtext editor", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Numeric,
                new NodeDto { NodeId = -51, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-51", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.NumericGuid, Text = "Numeric", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Checkbox,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.Boolean, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.Boolean}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.CheckboxGuid, Text = "True/false", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.CheckboxList,
                new NodeDto { NodeId = -43, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-43", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.CheckboxListGuid, Text = "Checkbox list", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Dropdown,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.DropDownSingle, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.DropDownSingle}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.DropdownGuid, Text = "Dropdown", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.DatePicker,
                new NodeDto { NodeId = -41, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-41", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.DatePickerGuid, Text = "Date Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Radiobox,
                new NodeDto { NodeId = -40, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-40", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.RadioboxGuid, Text = "Radiobox", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.DropdownMultiple,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.DropDownMultiple, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.DropDownMultiple}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.DropdownMultipleGuid, Text = "Dropdown multiple", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.ApprovedColor,
                new NodeDto { NodeId = -37, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-37", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.ApprovedColorGuid, Text = "Approved Color", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.DatePickerWithTime,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.DateTime, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.DateTime}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.DatePickerWithTimeGuid, Text = "Date Picker with time", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.ListViewContent,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.DefaultContentListView, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.DefaultContentListView}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.ListViewContentGuid, Text = Cms.Core.Constants.Conventions.DataTypes.ListViewPrefix + "Content", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.ListViewMedia,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.DefaultMediaListView, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.DefaultMediaListView}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.ListViewMediaGuid, Text = Cms.Core.Constants.Conventions.DataTypes.ListViewPrefix + "Media", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.ListViewMembers,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.DefaultMembersListView, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.DefaultMembersListView}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.ListViewMembersGuid, Text = Cms.Core.Constants.Conventions.DataTypes.ListViewPrefix + "Members", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.Tags,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.Tags, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.Tags}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.TagsGuid, Text = "Tags", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.ImageCropper,
                new NodeDto { NodeId = Cms.Core.Constants.DataTypes.ImageCropper, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Cms.Core.Constants.DataTypes.ImageCropper}", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.ImageCropperGuid, Text = "Image Cropper", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            // New UDI pickers with newer Ids
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.ContentPicker,
                new NodeDto { NodeId = 1046, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1046", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.ContentPickerGuid, Text = "Content Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MemberPicker,
                new NodeDto { NodeId = 1047, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1047", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MemberPickerGuid, Text = "Member Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MediaPicker,
                new NodeDto { NodeId = 1048, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1048", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MediaPickerGuid, Text = "Media Picker (legacy)", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MultipleMediaPicker,
                new NodeDto { NodeId = 1049, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1049", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MultipleMediaPickerGuid, Text = "Multiple Media Picker (legacy)", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.RelatedLinks,
                new NodeDto { NodeId = 1050, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1050", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.RelatedLinksGuid, Text = "Multi URL Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MediaPicker3,
                new NodeDto { NodeId = 1051, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1051", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MediaPicker3Guid, Text = "Media Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MediaPicker3Multiple,
                new NodeDto { NodeId = 1052, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1052", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MediaPicker3MultipleGuid, Text = "Multiple Media Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MediaPicker3SingleImage,
                new NodeDto { NodeId = 1053, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1053", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MediaPicker3SingleImageGuid, Text = "Image Media Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                Cms.Core.Constants.DataTypes.Guids.MediaPicker3MultipleImages,
                new NodeDto { NodeId = 1054, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1054", SortOrder = 2, UniqueId = Cms.Core.Constants.DataTypes.Guids.MediaPicker3MultipleImagesGuid, Text = "Multiple Image Media Picker", NodeObjectType = Cms.Core.Constants.ObjectTypes.DataType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
        }

        private void CreateNodeDataForMediaTypes()
        {
            var folderUniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                folderUniqueId.ToString(),
                new NodeDto { NodeId = 1031, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1031", SortOrder = 2, UniqueId = folderUniqueId, Text = Cms.Core.Constants.Conventions.MediaTypes.Folder, NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            var imageUniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                imageUniqueId.ToString(),
                new NodeDto { NodeId = 1032, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1032", SortOrder = 2, UniqueId = imageUniqueId, Text = Cms.Core.Constants.Conventions.MediaTypes.Image, NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            var fileUniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                fileUniqueId.ToString(),
                new NodeDto { NodeId = 1033, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1033", SortOrder = 2, UniqueId = fileUniqueId, Text = Cms.Core.Constants.Conventions.MediaTypes.File, NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            var videoUniqueId = new Guid("f6c515bb-653c-4bdc-821c-987729ebe327");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                videoUniqueId.ToString(),
                new NodeDto { NodeId = 1034, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1034", SortOrder = 2, UniqueId = videoUniqueId, Text = Cms.Core.Constants.Conventions.MediaTypes.Video, NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            var audioUniqueId = new Guid("a5ddeee0-8fd8-4cee-a658-6f1fcdb00de3");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                audioUniqueId.ToString(),
                new NodeDto { NodeId = 1035, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1035", SortOrder = 2, UniqueId = audioUniqueId, Text = Cms.Core.Constants.Conventions.MediaTypes.Audio, NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            var articleUniqueId = new Guid("a43e3414-9599-4230-a7d3-943a21b20122");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                articleUniqueId.ToString(),
                new NodeDto { NodeId = 1036, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1036", SortOrder = 2, UniqueId = articleUniqueId, Text = Cms.Core.Constants.Conventions.MediaTypes.Article, NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");

            var svgUniqueId = new Guid("c4b1efcf-a9d5-41c4-9621-e9d273b52a9c");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
                svgUniqueId.ToString(),
                new NodeDto { NodeId = 1037, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1037", SortOrder = 2, UniqueId = svgUniqueId, Text = "Vector Graphics (SVG)", NodeObjectType = Cms.Core.Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
        }

        private void CreateNodeDataForMemberTypes()
        {
            var memberUniqueId = new Guid("d59be02f-1df9-4228-aa1e-01917d806cda");
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.MemberTypes,
                memberUniqueId.ToString(),
                new NodeDto { NodeId = 1044, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1044", SortOrder = 0, UniqueId = memberUniqueId, Text = Cms.Core.Constants.Conventions.MemberTypes.DefaultAlias, NodeObjectType = Cms.Core.Constants.ObjectTypes.MemberType, CreateDate = DateTime.Now },
                Cms.Core.Constants.DatabaseSchema.Tables.Node,
                "id");
        }

        private void CreateLockData()
        {
            // all lock objects
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.Servers, Name = "Servers" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.ContentTypes, Name = "ContentTypes" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.ContentTree, Name = "ContentTree" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.MediaTypes, Name = "MediaTypes" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.MediaTree, Name = "MediaTree" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.MemberTypes, Name = "MemberTypes" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.MemberTree, Name = "MemberTree" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.Domains, Name = "Domains" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.KeyValues, Name = "KeyValues" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.Languages, Name = "Languages" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.ScheduledPublishing, Name = "ScheduledPublishing" });

            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Cms.Core.Constants.Locks.MainDom, Name = "MainDom" });
        }

        private void CreateContentTypeData()
        {
            // Insert content types only if the corresponding Node record exists (which may or may not have been created depending on configuration
            // of media or member types to create).

            // Media types.
            if (_database.Exists<NodeDto>(1031))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 532, NodeId = 1031, Alias = Cms.Core.Constants.Conventions.MediaTypes.Folder, Icon = Cms.Core.Constants.Icons.MediaFolder, Thumbnail = Cms.Core.Constants.Icons.MediaFolder, IsContainer = false, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<NodeDto>(1032))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 533, NodeId = 1032, Alias = Cms.Core.Constants.Conventions.MediaTypes.Image, Icon = Cms.Core.Constants.Icons.MediaImage, Thumbnail = Cms.Core.Constants.Icons.MediaImage, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<NodeDto>(1033))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 534, NodeId = 1033, Alias = Cms.Core.Constants.Conventions.MediaTypes.File, Icon = Cms.Core.Constants.Icons.MediaFile, Thumbnail = Cms.Core.Constants.Icons.MediaFile, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<NodeDto>(1034))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 540, NodeId = 1034, Alias = Cms.Core.Constants.Conventions.MediaTypes.VideoAlias, Icon = Cms.Core.Constants.Icons.MediaVideo, Thumbnail = Cms.Core.Constants.Icons.MediaVideo, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<NodeDto>(1035))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 541, NodeId = 1035, Alias = Cms.Core.Constants.Conventions.MediaTypes.AudioAlias, Icon = Cms.Core.Constants.Icons.MediaAudio, Thumbnail = Cms.Core.Constants.Icons.MediaAudio, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<NodeDto>(1036))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 542, NodeId = 1036, Alias = Cms.Core.Constants.Conventions.MediaTypes.ArticleAlias, Icon = Cms.Core.Constants.Icons.MediaArticle, Thumbnail = Cms.Core.Constants.Icons.MediaArticle, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<NodeDto>(1037))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 543, NodeId = 1037, Alias = Cms.Core.Constants.Conventions.MediaTypes.VectorGraphicsAlias, Icon = Cms.Core.Constants.Icons.MediaVectorGraphics, Thumbnail = Cms.Core.Constants.Icons.MediaVectorGraphics, AllowAtRoot = true, Variations = (byte)ContentVariation.Nothing });
            }

            // Member type.
            if (_database.Exists<NodeDto>(1044))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 531, NodeId = 1044, Alias = Cms.Core.Constants.Conventions.MemberTypes.DefaultAlias, Icon = Cms.Core.Constants.Icons.Member, Thumbnail = Cms.Core.Constants.Icons.Member, Variations = (byte)ContentVariation.Nothing });
            }
        }

        private void CreateUserData()
        {
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.User, "id", false, new UserDto { Id = Cms.Core.Constants.Security.SuperUserId, Disabled = false, NoConsole = false, UserName = "Administrator", Login = "admin", Password = "default", Email = string.Empty, UserLanguage = "en-US", CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
        }

        private void CreateUserGroupData()
        {
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 1, StartMediaId = -1, StartContentId = -1, Alias = Cms.Core.Constants.Security.AdminGroupAlias, Name = "Administrators", DefaultPermissions = "CADMOSKTPIURZ:5F7ïN", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-medal" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 2, StartMediaId = -1, StartContentId = -1, Alias = Cms.Core.Constants.Security.WriterGroupAlias, Name = "Writers", DefaultPermissions = "CAH:FN", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-edit" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 3, StartMediaId = -1, StartContentId = -1, Alias = Cms.Core.Constants.Security.EditorGroupAlias, Name = "Editors", DefaultPermissions = "CADMOSKTPUZ:5FïN", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-tools" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 4, StartMediaId = -1, StartContentId = -1, Alias = Cms.Core.Constants.Security.TranslatorGroupAlias, Name = "Translators", DefaultPermissions = "AF", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-globe" });
            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 5, StartMediaId = -1, StartContentId = -1, Alias = Cms.Core.Constants.Security.SensitiveDataGroupAlias, Name = "Sensitive data", DefaultPermissions = string.Empty, CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-lock" });
        }

        private void CreateUser2UserGroupData()
        {
            _database.Insert(new User2UserGroupDto { UserGroupId = 1, UserId = Cms.Core.Constants.Security.SuperUserId }); // add super to admins
            _database.Insert(new User2UserGroupDto { UserGroupId = 5, UserId = Cms.Core.Constants.Security.SuperUserId }); // add super to sensitive data
        }

        private void CreateUserGroup2AppData()
        {
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Content });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Packages });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Media });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Members });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Settings });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Users });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Forms });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Cms.Core.Constants.Applications.Translation });

            _database.Insert(new UserGroup2AppDto { UserGroupId = 2, AppAlias = Cms.Core.Constants.Applications.Content });

            _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Cms.Core.Constants.Applications.Content });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Cms.Core.Constants.Applications.Media });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Cms.Core.Constants.Applications.Forms });

            _database.Insert(new UserGroup2AppDto { UserGroupId = 4, AppAlias = Cms.Core.Constants.Applications.Translation });
        }

        private void CreatePropertyTypeGroupData()
        {
            // Insert property groups only if the corresponding content type node record exists (which may or may not have been created depending on configuration
            // of media or member types to create).

            // Media property groups.
            if (_database.Exists<NodeDto>(1032))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 3, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.Image), ContentTypeNodeId = 1032, Text = "Image", Alias = "image", SortOrder = 1 });
            }

            if (_database.Exists<NodeDto>(1033))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 4, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.File), ContentTypeNodeId = 1033, Text = "File", Alias = "file", SortOrder = 1, });
            }

            if (_database.Exists<NodeDto>(1034))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 52, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.Video), ContentTypeNodeId = 1034, Text = "Video", Alias = "video", SortOrder = 1 });
            }

            if (_database.Exists<NodeDto>(1035))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 53, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.Audio), ContentTypeNodeId = 1035, Text = "Audio", Alias = "audio", SortOrder = 1 });
            }

            if (_database.Exists<NodeDto>(1036))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 54, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.Article), ContentTypeNodeId = 1036, Text = "Article", Alias = "article", SortOrder = 1 });
            }

            if (_database.Exists<NodeDto>(1037))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 55, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.VectorGraphics), ContentTypeNodeId = 1037, Text = "Vector Graphics", Alias = "vectorGraphics", SortOrder = 1 });
            }

            // Membership property group.
            if (_database.Exists<NodeDto>(1044))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 11, UniqueId = new Guid(Cms.Core.Constants.PropertyTypeGroups.Membership), ContentTypeNodeId = 1044, Text = Cms.Core.Constants.Conventions.Member.StandardPropertiesGroupName, Alias = Cms.Core.Constants.Conventions.Member.StandardPropertiesGroupAlias, SortOrder = 1 });
            }
        }

        private void CreatePropertyTypeData()
        {
            // Insert property types only if the corresponding property group record exists (which may or may not have been created depending on configuration
            // of media or member types to create).

            // Media property types.
            if (_database.Exists<PropertyTypeGroupDto>(3))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 6, UniqueId = 6.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.ImageCropper, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Cms.Core.Constants.Conventions.Media.File, Name = "Image", SortOrder = 0, Mandatory = true, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 7, UniqueId = 7.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelInt, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Cms.Core.Constants.Conventions.Media.Width, Name = "Width", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in pixels", Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 8, UniqueId = 8.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelInt, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Cms.Core.Constants.Conventions.Media.Height, Name = "Height", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in pixels", Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 9, UniqueId = 9.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelBigint, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Cms.Core.Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in bytes", Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 10, UniqueId = 10.ToGuid(), DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Cms.Core.Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<PropertyTypeGroupDto>(4))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 24, UniqueId = 24.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.Upload, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Cms.Core.Constants.Conventions.Media.File, Name = "File", SortOrder = 0, Mandatory = true, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 25, UniqueId = 25.ToGuid(), DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Cms.Core.Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 26, UniqueId = 26.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelBigint, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Cms.Core.Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in bytes", Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<PropertyTypeGroupDto>(52))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 40, UniqueId = 40.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.UploadVideo, ContentTypeId = 1034, PropertyTypeGroupId = 52, Alias = Cms.Core.Constants.Conventions.Media.File, Name = "Video", SortOrder = 0, Mandatory = true, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 41, UniqueId = 41.ToGuid(), DataTypeId = -92, ContentTypeId = 1034, PropertyTypeGroupId = 52, Alias = Cms.Core.Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 42, UniqueId = 42.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelBigint, ContentTypeId = 1034, PropertyTypeGroupId = 52, Alias = Cms.Core.Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in bytes", Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<PropertyTypeGroupDto>(53))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 43, UniqueId = 43.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.UploadAudio, ContentTypeId = 1035, PropertyTypeGroupId = 53, Alias = Cms.Core.Constants.Conventions.Media.File, Name = "Audio", SortOrder = 0, Mandatory = true, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 44, UniqueId = 44.ToGuid(), DataTypeId = -92, ContentTypeId = 1035, PropertyTypeGroupId = 53, Alias = Cms.Core.Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 45, UniqueId = 45.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelBigint, ContentTypeId = 1035, PropertyTypeGroupId = 53, Alias = Cms.Core.Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in bytes", Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<PropertyTypeGroupDto>(54))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 46, UniqueId = 46.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.UploadArticle, ContentTypeId = 1036, PropertyTypeGroupId = 54, Alias = Cms.Core.Constants.Conventions.Media.File, Name = "Article", SortOrder = 0, Mandatory = true, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 47, UniqueId = 47.ToGuid(), DataTypeId = -92, ContentTypeId = 1036, PropertyTypeGroupId = 54, Alias = Cms.Core.Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 48, UniqueId = 48.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelBigint, ContentTypeId = 1036, PropertyTypeGroupId = 54, Alias = Cms.Core.Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in bytes", Variations = (byte)ContentVariation.Nothing });
            }

            if (_database.Exists<PropertyTypeGroupDto>(55))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 49, UniqueId = 49.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.UploadVectorGraphics, ContentTypeId = 1037, PropertyTypeGroupId = 55, Alias = Cms.Core.Constants.Conventions.Media.File, Name = "Vector Graphics", SortOrder = 0, Mandatory = true, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 50, UniqueId = 50.ToGuid(), DataTypeId = -92, ContentTypeId = 1037, PropertyTypeGroupId = 55, Alias = Cms.Core.Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing });
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 51, UniqueId = 51.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.LabelBigint, ContentTypeId = 1037, PropertyTypeGroupId = 55, Alias = Cms.Core.Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = "in bytes", Variations = (byte)ContentVariation.Nothing });
            }

            // Membership property types.
            if (_database.Exists<PropertyTypeGroupDto>(11))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                    new PropertyTypeDto
                    {
                        Id = 28, UniqueId = 28.ToGuid(), DataTypeId = Cms.Core.Constants.DataTypes.Textarea,
                        ContentTypeId = 1044, PropertyTypeGroupId = 11,
                        Alias = Cms.Core.Constants.Conventions.Member.Comments,
                        Name = Cms.Core.Constants.Conventions.Member.CommentsLabel, SortOrder = 0, Mandatory = false,
                        ValidationRegExp = null, Description = null, Variations = (byte)ContentVariation.Nothing
                    });
            }
        }

        private void CreateLanguageData() =>
            ConditionalInsert(
                Cms.Core.Constants.Configuration.NamedOptions.InstallDefaultData.Languages,
                "en-us",
                new LanguageDto { Id = 1, IsoCode = "en-US", CultureName = "English (United States)", IsDefault = true },
                Cms.Core.Constants.DatabaseSchema.Tables.Language,
                "id");

        private void CreateContentChildTypeData()
        {
            // Insert data if the corresponding Node records exist (which may or may not have been created depending on configuration
            // of media types to create).
            if (!_database.Exists<NodeDto>(1031))
            {
                return;
            }

            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentChildType, "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1031 });

            for (int i = 1032; i <= 1037; i++)
            {
                if (_database.Exists<NodeDto>(i))
                {
                    _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.ContentChildType, "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = i });
                }
            }
        }

        private void CreateDataTypeData()
        {
            void InsertDataTypeDto(int id, string editorAlias, string dbType, string? configuration = null)
            {
                var dataTypeDto = new DataTypeDto
                {
                    NodeId = id,
                    EditorAlias = editorAlias,
                    DbType = dbType
                };

                if (configuration != null)
                {
                    dataTypeDto.Configuration = configuration;
                }

                if (_database.Exists<NodeDto>(id))
                {
                    _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, dataTypeDto);
                }
            }

            //layouts for the list view
            const string cardLayout = "{\"name\": \"Grid\",\"path\": \"views/propertyeditors/listview/layouts/grid/grid.html\", \"icon\": \"icon-thumbnails-small\", \"isSystem\": 1, \"selected\": true}";
            const string listLayout = "{\"name\": \"List\",\"path\": \"views/propertyeditors/listview/layouts/list/list.html\",\"icon\": \"icon-list\", \"isSystem\": 1,\"selected\": true}";
            const string layouts = "[" + cardLayout + "," + listLayout + "]";

            // Insert data types only if the corresponding Node record exists (which may or may not have been created depending on configuration
            // of data types to create).
            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.Boolean))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Cms.Core.Constants.DataTypes.Boolean, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.Boolean, DbType = "Integer" });
            }

            if (_database.Exists<NodeDto>(-51))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -51, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.Integer, DbType = "Integer" });
            }

            if (_database.Exists<NodeDto>(-87))
            {
                _database.Insert(
                    Cms.Core.Constants.DatabaseSchema.Tables.DataType,
                    "pk",
                    false,
                    new DataTypeDto
                    {
                        NodeId = -87,
                        EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.TinyMce,
                        DbType = "Ntext",
                        Configuration = "{\"value\":\",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|\"}"
                    });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.Textbox))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Cms.Core.Constants.DataTypes.Textbox, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.TextBox, DbType = "Nvarchar" });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.Textarea))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Cms.Core.Constants.DataTypes.Textarea, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.TextArea, DbType = "Ntext" });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.Upload))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Cms.Core.Constants.DataTypes.Upload, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.UploadField, DbType = "Nvarchar" });
            }

            InsertDataTypeDto(Cms.Core.Constants.DataTypes.LabelString, Cms.Core.Constants.PropertyEditors.Aliases.Label, "Nvarchar", "{\"umbracoDataValueType\":\"STRING\"}");
            InsertDataTypeDto(Cms.Core.Constants.DataTypes.LabelInt, Cms.Core.Constants.PropertyEditors.Aliases.Label, "Integer", "{\"umbracoDataValueType\":\"INT\"}");
            InsertDataTypeDto(Cms.Core.Constants.DataTypes.LabelBigint, Cms.Core.Constants.PropertyEditors.Aliases.Label, "Nvarchar", "{\"umbracoDataValueType\":\"BIGINT\"}");
            InsertDataTypeDto(Cms.Core.Constants.DataTypes.LabelDateTime, Cms.Core.Constants.PropertyEditors.Aliases.Label, "Date", "{\"umbracoDataValueType\":\"DATETIME\"}");
            InsertDataTypeDto(Cms.Core.Constants.DataTypes.LabelDecimal, Cms.Core.Constants.PropertyEditors.Aliases.Label, "Decimal", "{\"umbracoDataValueType\":\"DECIMAL\"}");
            InsertDataTypeDto(Cms.Core.Constants.DataTypes.LabelTime, Cms.Core.Constants.PropertyEditors.Aliases.Label, "Date", "{\"umbracoDataValueType\":\"TIME\"}");

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.DateTime))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Cms.Core.Constants.DataTypes.DateTime, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.DateTime, DbType = "Date" });
            }

            if (_database.Exists<NodeDto>(-37))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -37, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.ColorPicker, DbType = "Nvarchar" });
            }

            InsertDataTypeDto(Cms.Core.Constants.DataTypes.DropDownSingle, Cms.Core.Constants.PropertyEditors.Aliases.DropDownListFlexible, "Nvarchar", "{\"multiple\":false}");

            if (_database.Exists<NodeDto>(-40))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -40, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.RadioButtonList, DbType = "Nvarchar" });
            }

            if (_database.Exists<NodeDto>(-41))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -41, EditorAlias = "Umbraco.DateTime", DbType = "Date", Configuration = "{\"format\":\"YYYY-MM-DD\"}" });
            }

            InsertDataTypeDto(Cms.Core.Constants.DataTypes.DropDownMultiple, Cms.Core.Constants.PropertyEditors.Aliases.DropDownListFlexible, "Nvarchar", "{\"multiple\":true}");

            if (_database.Exists<NodeDto>(-43))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -43, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.CheckBoxList, DbType = "Nvarchar" });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.Tags))
            {
                _database.Insert(
                    Cms.Core.Constants.DatabaseSchema.Tables.DataType,
                    "pk",
                    false,
                    new DataTypeDto
                    {
                        NodeId = Cms.Core.Constants.DataTypes.Tags,
                        EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.Tags,
                        DbType = "Ntext",
                        Configuration = "{\"group\":\"default\", \"storageType\":\"Json\"}"
                    });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.ImageCropper))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Cms.Core.Constants.DataTypes.ImageCropper, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.ImageCropper, DbType = "Ntext" });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.DefaultContentListView))
            {
                _database.Insert(
                    Cms.Core.Constants.DatabaseSchema.Tables.DataType,
                    "pk",
                    false,
                    new DataTypeDto
                    {
                        NodeId = Cms.Core.Constants.DataTypes.DefaultContentListView,
                        EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.ListView,
                        DbType = "Nvarchar",
                        Configuration = "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" + layouts + ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}"
                    });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.DefaultMediaListView))
            {
                _database.Insert(
                    Cms.Core.Constants.DatabaseSchema.Tables.DataType,
                    "pk",
                    false,
                    new DataTypeDto
                    {
                        NodeId = Cms.Core.Constants.DataTypes.DefaultMediaListView,
                        EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.ListView,
                        DbType = "Nvarchar",
                        Configuration = "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" + layouts + ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}"
                    });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.DefaultMembersListView))
            {
                _database.Insert(
                    Cms.Core.Constants.DatabaseSchema.Tables.DataType,
                    "pk",
                    false,
                    new DataTypeDto
                    {
                        NodeId = Cms.Core.Constants.DataTypes.DefaultMembersListView,
                        EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.ListView,
                        DbType = "Nvarchar",
                        Configuration = "{\"pageSize\":10, \"orderBy\":\"username\", \"orderDirection\":\"asc\", \"includeProperties\":[{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]}"
                    });
            }

            // New UDI pickers with newer Ids
            if (_database.Exists<NodeDto>(1046))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1046, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.ContentPicker, DbType = "Nvarchar" });
            }

            if (_database.Exists<NodeDto>(1047))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1047, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MemberPicker, DbType = "Nvarchar" });
            }

            if (_database.Exists<NodeDto>(1048))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1048, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker, DbType = "Ntext" });
            }

            if (_database.Exists<NodeDto>(1049))
            {
                _database.Insert(
                    Cms.Core.Constants.DatabaseSchema.Tables.DataType,
                    "pk",
                    false,
                    new DataTypeDto
                    {
                        NodeId = 1049,
                        EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker,
                        DbType = "Ntext",
                        Configuration = "{\"multiPicker\":1}"
                    });
            }

            if (_database.Exists<NodeDto>(1050))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1050, EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MultiUrlPicker, DbType = "Ntext" });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.UploadVideo))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = Cms.Core.Constants.DataTypes.UploadVideo,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration = "{\"fileExtensions\":[{\"id\":0, \"value\":\"mp4\"}, {\"id\":1, \"value\":\"webm\"}, {\"id\":2, \"value\":\"ogv\"}]}"
                });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.UploadAudio))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = Cms.Core.Constants.DataTypes.UploadAudio,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration = "{\"fileExtensions\":[{\"id\":0, \"value\":\"mp3\"}, {\"id\":1, \"value\":\"weba\"}, {\"id\":2, \"value\":\"oga\"}, {\"id\":3, \"value\":\"opus\"}]}"
                });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.UploadArticle))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = Cms.Core.Constants.DataTypes.UploadArticle,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration = "{\"fileExtensions\":[{\"id\":0, \"value\":\"pdf\"}, {\"id\":1, \"value\":\"docx\"}, {\"id\":2, \"value\":\"doc\"}]}"
                });
            }

            if (_database.Exists<NodeDto>(Cms.Core.Constants.DataTypes.UploadVectorGraphics))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = Cms.Core.Constants.DataTypes.UploadVectorGraphics,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration = "{\"fileExtensions\":[{\"id\":0, \"value\":\"svg\"}]}"
                });
            }

            if (_database.Exists<NodeDto>(1051))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = 1051,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"multiple\": false, \"validationLimit\":{\"min\":0,\"max\":1}}"
                });
            }

            if (_database.Exists<NodeDto>(1052))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = 1052,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"multiple\": true}"
                });
            }

            if (_database.Exists<NodeDto>(1053))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = 1053,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"filter\":\"" + Cms.Core.Constants.Conventions.MediaTypes.Image + "\", \"multiple\": false, \"validationLimit\":{\"min\":0,\"max\":1}}"
                });
            }

            if (_database.Exists<NodeDto>(1054))
            {
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto
                {
                    NodeId = 1054,
                    EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"filter\":\"" + Cms.Core.Constants.Conventions.MediaTypes.Image + "\", \"multiple\": true}"
                });
            }
        }

        private void CreateRelationTypeData()
        {
            CreateRelationTypeData(1, Cms.Core.Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias, Cms.Core.Constants.Conventions.RelationTypes.RelateDocumentOnCopyName, Cms.Core.Constants.ObjectTypes.Document, Cms.Core.Constants.ObjectTypes.Document, true, false);
            CreateRelationTypeData(2, Cms.Core.Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias, Cms.Core.Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName, Cms.Core.Constants.ObjectTypes.Document, Cms.Core.Constants.ObjectTypes.Document, false, false);
            CreateRelationTypeData(3, Cms.Core.Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias, Cms.Core.Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName, Cms.Core.Constants.ObjectTypes.Media, Cms.Core.Constants.ObjectTypes.Media, false, false);
            CreateRelationTypeData(4, Cms.Core.Constants.Conventions.RelationTypes.RelatedMediaAlias, Cms.Core.Constants.Conventions.RelationTypes.RelatedMediaName, null, null, false, true);
            CreateRelationTypeData(5, Cms.Core.Constants.Conventions.RelationTypes.RelatedDocumentAlias, Cms.Core.Constants.Conventions.RelationTypes.RelatedDocumentName, null, null, false, true);
        }

        private void CreateRelationTypeData(int id, string alias, string name, Guid? parentObjectType, Guid? childObjectType, bool dual, bool isDependency)
        {
            var relationType = new RelationTypeDto { Id = id, Alias = alias, ChildObjectType = childObjectType, ParentObjectType = parentObjectType, Dual = dual, Name = name, IsDependency = isDependency };
            relationType.UniqueId = CreateUniqueRelationTypeId(relationType.Alias, relationType.Name);

            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.RelationType, "id", false, relationType);
        }

        internal static Guid CreateUniqueRelationTypeId(string alias, string name)
        {
            return (alias + "____" + name).ToGuid();
        }

        private void CreateKeyValueData()
        {
            // On install, initialize the umbraco migration plan with the final state.
            var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion));
            var stateValueKey = upgrader.StateValueKey;
            var finalState = upgrader.Plan.FinalState;

            _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.KeyValue, "key", false, new KeyValueDto { Key = stateValueKey, Value = finalState, UpdateDate = DateTime.Now });
        }

        private void CreateLogViewerQueryData()
        {
            LogViewerQueryDto[] defaultData = MigrateLogViewerQueriesFromFileToDb.DefaultLogQueries.ToArray();

            for (int i = 0; i < defaultData.Length; i++)
            {
                LogViewerQueryDto dto = defaultData[i];
                dto.Id = i+1;
                _database.Insert(Cms.Core.Constants.DatabaseSchema.Tables.LogViewerQuery, "id", false, dto);
            }
        }

        private void ConditionalInsert<TDto>(
            string configKey,
            string id,
            TDto dto,
            string tableName,
            string primaryKeyName,
            bool autoIncrement = false)
        {
            var alwaysInsert = _entitiesToAlwaysCreate.ContainsKey(configKey) &&
                _entitiesToAlwaysCreate[configKey].InvariantContains(id.ToString());

            InstallDefaultDataSettings installDefaultDataSettings = _installDefaultDataSettings.Get(configKey);

            // If there's no configuration, we assume to create.
            if (installDefaultDataSettings == null)
            {
                alwaysInsert = true;
            }

            if (!alwaysInsert && installDefaultDataSettings?.InstallData == InstallDefaultDataOption.None)
            {
                return;
            }

            if (!alwaysInsert && installDefaultDataSettings?.InstallData == InstallDefaultDataOption.Values && !installDefaultDataSettings.Values.InvariantContains(id))
            {
                return;
            }

            if (!alwaysInsert && installDefaultDataSettings?.InstallData == InstallDefaultDataOption.ExceptValues && installDefaultDataSettings.Values.InvariantContains(id))
            {
                return;
            }

            _database.Insert(tableName, primaryKeyName, autoIncrement, dto);
        }
    }
}
