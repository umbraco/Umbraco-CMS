using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Install;

/// <summary>
///     Creates the initial database data during install.
/// </summary>
internal class DatabaseDataCreator
{
    private readonly IDatabase _database;

    private readonly IDictionary<string, IList<string>> _entitiesToAlwaysCreate = new Dictionary<string, IList<string>>
    {
        {
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            new List<string> { Constants.DataTypes.Guids.LabelString }
        },
    };

    private readonly IOptionsMonitor<InstallDefaultDataSettings> _installDefaultDataSettings;
    private readonly ILogger<DatabaseDataCreator> _logger;
    private readonly IUmbracoVersion _umbracoVersion;

    public DatabaseDataCreator(IDatabase database, ILogger<DatabaseDataCreator> logger, IUmbracoVersion umbracoVersion,
        IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
    {
        _database = database;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _installDefaultDataSettings = installDefaultDataSettings;
    }

    /// <summary>
    ///     Initialize the base data creation by inserting the data foundation for umbraco
    ///     specific to a table
    /// </summary>
    /// <param name="tableName">Name of the table to create base data for</param>
    public void InitializeBaseData(string tableName)
    {
        _logger.LogInformation("Creating data in {TableName}", tableName);

        if (tableName.Equals(Constants.DatabaseSchema.Tables.Node))
        {
            CreateNodeData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.Lock))
        {
            CreateLockData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.ContentType))
        {
            CreateContentTypeData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.User))
        {
            CreateUserData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.UserGroup))
        {
            CreateUserGroupData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.User2UserGroup))
        {
            CreateUser2UserGroupData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.UserGroup2App))
        {
            CreateUserGroup2AppData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.PropertyTypeGroup))
        {
            CreatePropertyTypeGroupData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.PropertyType))
        {
            CreatePropertyTypeData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.Language))
        {
            CreateLanguageData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.ContentChildType))
        {
            CreateContentChildTypeData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.DataType))
        {
            CreateDataTypeData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.RelationType))
        {
            CreateRelationTypeData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.KeyValue))
        {
            CreateKeyValueData();
        }

        if (tableName.Equals(Constants.DatabaseSchema.Tables.LogViewerQuery))
        {
            CreateLogViewerQueryData();
        }

        _logger.LogInformation("Completed creating data in {TableName}", tableName);
    }

    internal static Guid CreateUniqueRelationTypeId(string alias, string name) => (alias + "____" + name).ToGuid();

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
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            };

            ConditionalInsert(
                Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                uniqueId,
                nodeDto,
                Constants.DatabaseSchema.Tables.Node,
                "id");
        }

        _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false,
            new NodeDto
            {
                NodeId = -1,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 0,
                Path = "-1",
                SortOrder = 0,
                UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"),
                Text = "SYSTEM DATA: umbraco master root",
                NodeObjectType = Constants.ObjectTypes.SystemRoot,
                CreateDate = DateTime.Now,
            });
        _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false,
            new NodeDto
            {
                NodeId = -20,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 0,
                Path = "-1,-20",
                SortOrder = 0,
                UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"),
                Text = "Recycle Bin",
                NodeObjectType = Constants.ObjectTypes.ContentRecycleBin,
                CreateDate = DateTime.Now,
            });
        _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false,
            new NodeDto
            {
                NodeId = -21,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 0,
                Path = "-1,-21",
                SortOrder = 0,
                UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"),
                Text = "Recycle Bin",
                NodeObjectType = Constants.ObjectTypes.MediaRecycleBin,
                CreateDate = DateTime.Now,
            });

        InsertDataTypeNodeDto(Constants.DataTypes.LabelString, 35, Constants.DataTypes.Guids.LabelString,
            "Label (string)");
        InsertDataTypeNodeDto(Constants.DataTypes.LabelInt, 36, Constants.DataTypes.Guids.LabelInt, "Label (integer)");
        InsertDataTypeNodeDto(Constants.DataTypes.LabelBigint, 36, Constants.DataTypes.Guids.LabelBigInt,
            "Label (bigint)");
        InsertDataTypeNodeDto(Constants.DataTypes.LabelDateTime, 37, Constants.DataTypes.Guids.LabelDateTime,
            "Label (datetime)");
        InsertDataTypeNodeDto(Constants.DataTypes.LabelTime, 38, Constants.DataTypes.Guids.LabelTime, "Label (time)");
        InsertDataTypeNodeDto(Constants.DataTypes.LabelDecimal, 39, Constants.DataTypes.Guids.LabelDecimal,
            "Label (decimal)");

        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Upload,
            new NodeDto
            {
                NodeId = Constants.DataTypes.Upload,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Upload}",
                SortOrder = 34,
                UniqueId = Constants.DataTypes.Guids.UploadGuid,
                Text = "Upload File",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadVideo,
            new NodeDto
            {
                NodeId = Constants.DataTypes.UploadVideo,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadVideo}",
                SortOrder = 35,
                UniqueId = Constants.DataTypes.Guids.UploadVideoGuid,
                Text = "Upload Video",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadAudio,
            new NodeDto
            {
                NodeId = Constants.DataTypes.UploadAudio,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadAudio}",
                SortOrder = 36,
                UniqueId = Constants.DataTypes.Guids.UploadAudioGuid,
                Text = "Upload Audio",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadArticle,
            new NodeDto
            {
                NodeId = Constants.DataTypes.UploadArticle,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadArticle}",
                SortOrder = 37,
                UniqueId = Constants.DataTypes.Guids.UploadArticleGuid,
                Text = "Upload Article",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadVectorGraphics,
            new NodeDto
            {
                NodeId = Constants.DataTypes.UploadVectorGraphics,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadVectorGraphics}",
                SortOrder = 38,
                UniqueId = Constants.DataTypes.Guids.UploadVectorGraphicsGuid,
                Text = "Upload Vector Graphics",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Textarea,
            new NodeDto
            {
                NodeId = Constants.DataTypes.Textarea,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Textarea}",
                SortOrder = 33,
                UniqueId = Constants.DataTypes.Guids.TextareaGuid,
                Text = "Textarea",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Textstring,
            new NodeDto
            {
                NodeId = Constants.DataTypes.Textbox,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Textbox}",
                SortOrder = 32,
                UniqueId = Constants.DataTypes.Guids.TextstringGuid,
                Text = "Textstring",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.RichtextEditor,
            new NodeDto
            {
                NodeId = Constants.DataTypes.RichtextEditor,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.RichtextEditor}",
                SortOrder = 4,
                UniqueId = Constants.DataTypes.Guids.RichtextEditorGuid,
                Text = "Richtext editor",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Numeric,
            new NodeDto
            {
                NodeId = -51,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,-51",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.NumericGuid,
                Text = "Numeric",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Checkbox,
            new NodeDto
            {
                NodeId = Constants.DataTypes.Boolean,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Boolean}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.CheckboxGuid,
                Text = "True/false",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.CheckboxList,
            new NodeDto
            {
                NodeId = -43,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,-43",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.CheckboxListGuid,
                Text = "Checkbox list",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Dropdown,
            new NodeDto
            {
                NodeId = Constants.DataTypes.DropDownSingle,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DropDownSingle}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DropdownGuid,
                Text = "Dropdown",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.DatePicker,
            new NodeDto
            {
                NodeId = -41,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,-41",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DatePickerGuid,
                Text = "Date Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Radiobox,
            new NodeDto
            {
                NodeId = -40,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,-40",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.RadioboxGuid,
                Text = "Radiobox",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.DropdownMultiple,
            new NodeDto
            {
                NodeId = Constants.DataTypes.DropDownMultiple,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DropDownMultiple}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DropdownMultipleGuid,
                Text = "Dropdown multiple",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ApprovedColor,
            new NodeDto
            {
                NodeId = -37,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,-37",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ApprovedColorGuid,
                Text = "Approved Color",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.DatePickerWithTime,
            new NodeDto
            {
                NodeId = Constants.DataTypes.DateTime,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DateTime}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DatePickerWithTimeGuid,
                Text = "Date Picker with time",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ListViewContent,
            new NodeDto
            {
                NodeId = Constants.DataTypes.DefaultContentListView,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DefaultContentListView}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ListViewContentGuid,
                Text = Constants.Conventions.DataTypes.ListViewPrefix + "Content",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ListViewMedia,
            new NodeDto
            {
                NodeId = Constants.DataTypes.DefaultMediaListView,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DefaultMediaListView}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ListViewMediaGuid,
                Text = Constants.Conventions.DataTypes.ListViewPrefix + "Media",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ListViewMembers,
            new NodeDto
            {
                NodeId = Constants.DataTypes.DefaultMembersListView,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DefaultMembersListView}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ListViewMembersGuid,
                Text = Constants.Conventions.DataTypes.ListViewPrefix + "Members",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Tags,
            new NodeDto
            {
                NodeId = Constants.DataTypes.Tags,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Tags}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.TagsGuid,
                Text = "Tags",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ImageCropper,
            new NodeDto
            {
                NodeId = Constants.DataTypes.ImageCropper,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.ImageCropper}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ImageCropperGuid,
                Text = "Image Cropper",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        // New UDI pickers with newer Ids
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ContentPicker,
            new NodeDto
            {
                NodeId = 1046,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1046",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ContentPickerGuid,
                Text = "Content Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MemberPicker,
            new NodeDto
            {
                NodeId = 1047,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1047",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MemberPickerGuid,
                Text = "Member Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker,
            new NodeDto
            {
                NodeId = 1048,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1048",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPickerGuid,
                Text = "Media Picker (legacy)",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MultipleMediaPicker,
            new NodeDto
            {
                NodeId = 1049,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1049",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MultipleMediaPickerGuid,
                Text = "Multiple Media Picker (legacy)",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.RelatedLinks,
            new NodeDto
            {
                NodeId = 1050,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1050",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.RelatedLinksGuid,
                Text = "Multi URL Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3,
            new NodeDto
            {
                NodeId = 1051,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1051",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3Guid,
                Text = "Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3Multiple,
            new NodeDto
            {
                NodeId = 1052,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1052",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3MultipleGuid,
                Text = "Multiple Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3SingleImage,
            new NodeDto
            {
                NodeId = 1053,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1053",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3SingleImageGuid,
                Text = "Image Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3MultipleImages,
            new NodeDto
            {
                NodeId = 1054,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1054",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3MultipleImagesGuid,
                Text = "Multiple Image Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
    }

    private void CreateNodeDataForMediaTypes()
    {
        var folderUniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            folderUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1031,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1031",
                SortOrder = 2,
                UniqueId = folderUniqueId,
                Text = Constants.Conventions.MediaTypes.Folder,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        var imageUniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            imageUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1032,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1032",
                SortOrder = 2,
                UniqueId = imageUniqueId,
                Text = Constants.Conventions.MediaTypes.Image,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        var fileUniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            fileUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1033,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1033",
                SortOrder = 2,
                UniqueId = fileUniqueId,
                Text = Constants.Conventions.MediaTypes.File,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        var videoUniqueId = new Guid("f6c515bb-653c-4bdc-821c-987729ebe327");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            videoUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1034,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1034",
                SortOrder = 2,
                UniqueId = videoUniqueId,
                Text = Constants.Conventions.MediaTypes.Video,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        var audioUniqueId = new Guid("a5ddeee0-8fd8-4cee-a658-6f1fcdb00de3");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            audioUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1035,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1035",
                SortOrder = 2,
                UniqueId = audioUniqueId,
                Text = Constants.Conventions.MediaTypes.Audio,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        var articleUniqueId = new Guid("a43e3414-9599-4230-a7d3-943a21b20122");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            articleUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1036,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1036",
                SortOrder = 2,
                UniqueId = articleUniqueId,
                Text = Constants.Conventions.MediaTypes.Article,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");

        var svgUniqueId = new Guid("c4b1efcf-a9d5-41c4-9621-e9d273b52a9c");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            svgUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1037,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1037",
                SortOrder = 2,
                UniqueId = svgUniqueId,
                Text = "Vector Graphics (SVG)",
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
    }

    private void CreateNodeDataForMemberTypes()
    {
        var memberUniqueId = new Guid("d59be02f-1df9-4228-aa1e-01917d806cda");
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.MemberTypes,
            memberUniqueId.ToString(),
            new NodeDto
            {
                NodeId = 1044,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,1044",
                SortOrder = 0,
                UniqueId = memberUniqueId,
                Text = Constants.Conventions.MemberTypes.DefaultAlias,
                NodeObjectType = Constants.ObjectTypes.MemberType,
                CreateDate = DateTime.Now,
            },
            Constants.DatabaseSchema.Tables.Node,
            "id");
    }

    private void CreateLockData()
    {
        // all lock objects
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.Servers, Name = "Servers" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.ContentTypes, Name = "ContentTypes" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.ContentTree, Name = "ContentTree" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.MediaTypes, Name = "MediaTypes" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.MediaTree, Name = "MediaTree" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.MemberTypes, Name = "MemberTypes" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.MemberTree, Name = "MemberTree" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.Domains, Name = "Domains" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.KeyValues, Name = "KeyValues" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.Languages, Name = "Languages" });
        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.ScheduledPublishing, Name = "ScheduledPublishing" });

        _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
            new LockDto { Id = Constants.Locks.MainDom, Name = "MainDom" });
    }

    private void CreateContentTypeData()
    {
        // Insert content types only if the corresponding Node record exists (which may or may not have been created depending on configuration
        // of media or member types to create).

        // Media types.
        if (_database.Exists<NodeDto>(1031))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 532,
                    NodeId = 1031,
                    Alias = Constants.Conventions.MediaTypes.Folder,
                    Icon = Constants.Icons.MediaFolder,
                    Thumbnail = Constants.Icons.MediaFolder,
                    IsContainer = false,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<NodeDto>(1032))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 533,
                    NodeId = 1032,
                    Alias = Constants.Conventions.MediaTypes.Image,
                    Icon = Constants.Icons.MediaImage,
                    Thumbnail = Constants.Icons.MediaImage,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<NodeDto>(1033))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 534,
                    NodeId = 1033,
                    Alias = Constants.Conventions.MediaTypes.File,
                    Icon = Constants.Icons.MediaFile,
                    Thumbnail = Constants.Icons.MediaFile,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<NodeDto>(1034))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 540,
                    NodeId = 1034,
                    Alias = Constants.Conventions.MediaTypes.VideoAlias,
                    Icon = Constants.Icons.MediaVideo,
                    Thumbnail = Constants.Icons.MediaVideo,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<NodeDto>(1035))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 541,
                    NodeId = 1035,
                    Alias = Constants.Conventions.MediaTypes.AudioAlias,
                    Icon = Constants.Icons.MediaAudio,
                    Thumbnail = Constants.Icons.MediaAudio,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<NodeDto>(1036))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 542,
                    NodeId = 1036,
                    Alias = Constants.Conventions.MediaTypes.ArticleAlias,
                    Icon = Constants.Icons.MediaArticle,
                    Thumbnail = Constants.Icons.MediaArticle,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<NodeDto>(1037))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 543,
                    NodeId = 1037,
                    Alias = Constants.Conventions.MediaTypes.VectorGraphicsAlias,
                    Icon = Constants.Icons.MediaVectorGraphics,
                    Thumbnail = Constants.Icons.MediaVectorGraphics,
                    AllowAtRoot = true,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        // Member type.
        if (_database.Exists<NodeDto>(1044))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false,
                new ContentTypeDto
                {
                    PrimaryKey = 531,
                    NodeId = 1044,
                    Alias = Constants.Conventions.MemberTypes.DefaultAlias,
                    Icon = Constants.Icons.Member,
                    Thumbnail = Constants.Icons.Member,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }
    }

    private void CreateUserData() => _database.Insert(Constants.DatabaseSchema.Tables.User, "id", false,
        new UserDto
        {
            Id = Constants.Security.SuperUserId,
            Disabled = false,
            NoConsole = false,
            UserName = "Administrator",
            Login = "admin",
            Password = "default",
            Email = string.Empty,
            UserLanguage = "en-US",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
        });

    private void CreateUserGroupData()
    {
        _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false,
            new UserGroupDto
            {
                Id = 1,
                StartMediaId = -1,
                StartContentId = -1,
                Alias = Constants.Security.AdminGroupAlias,
                Name = "Administrators",
                DefaultPermissions = "CADMOSKTPIURZ:5F7ïN",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-medal",
                HasAccessToAllLanguages = true,
            });
        _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false,
            new UserGroupDto
            {
                Id = 2,
                StartMediaId = -1,
                StartContentId = -1,
                Alias = Constants.Security.WriterGroupAlias,
                Name = "Writers",
                DefaultPermissions = "CAH:FN",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-edit",
                HasAccessToAllLanguages = true,
            });
        _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false,
            new UserGroupDto
            {
                Id = 3,
                StartMediaId = -1,
                StartContentId = -1,
                Alias = Constants.Security.EditorGroupAlias,
                Name = "Editors",
                DefaultPermissions = "CADMOSKTPUZ:5FïN",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-tools",
                HasAccessToAllLanguages = true,
            });
        _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false,
            new UserGroupDto
            {
                Id = 4,
                StartMediaId = -1,
                StartContentId = -1,
                Alias = Constants.Security.TranslatorGroupAlias,
                Name = "Translators",
                DefaultPermissions = "AF",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-globe",
                HasAccessToAllLanguages = true,
            });
        _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false,
            new UserGroupDto
            {
                Id = 5,
                Alias = Constants.Security.SensitiveDataGroupAlias,
                Name = "Sensitive data",
                DefaultPermissions = string.Empty,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-lock",
                HasAccessToAllLanguages = false,
            });
    }

    private void CreateUser2UserGroupData()
    {
        _database.Insert(new User2UserGroupDto
        {
            UserGroupId = 1,
            UserId = Constants.Security.SuperUserId,
        }); // add super to admins
        _database.Insert(new User2UserGroupDto
        {
            UserGroupId = 5,
            UserId = Constants.Security.SuperUserId,
        }); // add super to sensitive data
    }

    private void CreateUserGroup2AppData()
    {
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Content });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Packages });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Media });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Members });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Settings });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Users });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Forms });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Translation });

        _database.Insert(new UserGroup2AppDto { UserGroupId = 2, AppAlias = Constants.Applications.Content });

        _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Constants.Applications.Content });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Constants.Applications.Media });
        _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Constants.Applications.Forms });

        _database.Insert(new UserGroup2AppDto { UserGroupId = 4, AppAlias = Constants.Applications.Translation });
    }

    private void CreatePropertyTypeGroupData()
    {
        // Insert property groups only if the corresponding content type node record exists (which may or may not have been created depending on configuration
        // of media or member types to create).

        // Media property groups.
        if (_database.Exists<NodeDto>(1032))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 3,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Image),
                    ContentTypeNodeId = 1032,
                    Text = "Image",
                    Alias = "image",
                    SortOrder = 1,
                });
        }

        if (_database.Exists<NodeDto>(1033))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 4,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.File),
                    ContentTypeNodeId = 1033,
                    Text = "File",
                    Alias = "file",
                    SortOrder = 1,
                });
        }

        if (_database.Exists<NodeDto>(1034))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 52,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Video),
                    ContentTypeNodeId = 1034,
                    Text = "Video",
                    Alias = "video",
                    SortOrder = 1,
                });
        }

        if (_database.Exists<NodeDto>(1035))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 53,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Audio),
                    ContentTypeNodeId = 1035,
                    Text = "Audio",
                    Alias = "audio",
                    SortOrder = 1,
                });
        }

        if (_database.Exists<NodeDto>(1036))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 54,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Article),
                    ContentTypeNodeId = 1036,
                    Text = "Article",
                    Alias = "article",
                    SortOrder = 1,
                });
        }

        if (_database.Exists<NodeDto>(1037))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 55,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.VectorGraphics),
                    ContentTypeNodeId = 1037,
                    Text = "Vector Graphics",
                    Alias = "vectorGraphics",
                    SortOrder = 1,
                });
        }

        // Membership property group.
        if (_database.Exists<NodeDto>(1044))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false,
                new PropertyTypeGroupDto
                {
                    Id = 11,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Membership),
                    ContentTypeNodeId = 1044,
                    Text = Constants.Conventions.Member.StandardPropertiesGroupName,
                    Alias = Constants.Conventions.Member.StandardPropertiesGroupAlias,
                    SortOrder = 1,
                });
        }
    }

    private void CreatePropertyTypeData()
    {
        // Insert property types only if the corresponding property group record exists (which may or may not have been created depending on configuration
        // of media or member types to create).

        // Media property types.
        if (_database.Exists<PropertyTypeGroupDto>(3))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 6,
                    UniqueId = 6.ToGuid(),
                    DataTypeId = Constants.DataTypes.ImageCropper,
                    ContentTypeId = 1032,
                    PropertyTypeGroupId = 3,
                    Alias = Constants.Conventions.Media.File,
                    Name = "Image",
                    SortOrder = 0,
                    Mandatory = true,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 7,
                    UniqueId = 7.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelInt,
                    ContentTypeId = 1032,
                    PropertyTypeGroupId = 3,
                    Alias = Constants.Conventions.Media.Width,
                    Name = "Width",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in pixels",
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 8,
                    UniqueId = 8.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelInt,
                    ContentTypeId = 1032,
                    PropertyTypeGroupId = 3,
                    Alias = Constants.Conventions.Media.Height,
                    Name = "Height",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in pixels",
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 9,
                    UniqueId = 9.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelBigint,
                    ContentTypeId = 1032,
                    PropertyTypeGroupId = 3,
                    Alias = Constants.Conventions.Media.Bytes,
                    Name = "Size",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in bytes",
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 10,
                    UniqueId = 10.ToGuid(),
                    DataTypeId = -92,
                    ContentTypeId = 1032,
                    PropertyTypeGroupId = 3,
                    Alias = Constants.Conventions.Media.Extension,
                    Name = "Type",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<PropertyTypeGroupDto>(4))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 24,
                    UniqueId = 24.ToGuid(),
                    DataTypeId = Constants.DataTypes.Upload,
                    ContentTypeId = 1033,
                    PropertyTypeGroupId = 4,
                    Alias = Constants.Conventions.Media.File,
                    Name = "File",
                    SortOrder = 0,
                    Mandatory = true,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 25,
                    UniqueId = 25.ToGuid(),
                    DataTypeId = -92,
                    ContentTypeId = 1033,
                    PropertyTypeGroupId = 4,
                    Alias = Constants.Conventions.Media.Extension,
                    Name = "Type",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 26,
                    UniqueId = 26.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelBigint,
                    ContentTypeId = 1033,
                    PropertyTypeGroupId = 4,
                    Alias = Constants.Conventions.Media.Bytes,
                    Name = "Size",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in bytes",
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<PropertyTypeGroupDto>(52))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 40,
                    UniqueId = 40.ToGuid(),
                    DataTypeId = Constants.DataTypes.UploadVideo,
                    ContentTypeId = 1034,
                    PropertyTypeGroupId = 52,
                    Alias = Constants.Conventions.Media.File,
                    Name = "Video",
                    SortOrder = 0,
                    Mandatory = true,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 41,
                    UniqueId = 41.ToGuid(),
                    DataTypeId = -92,
                    ContentTypeId = 1034,
                    PropertyTypeGroupId = 52,
                    Alias = Constants.Conventions.Media.Extension,
                    Name = "Type",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 42,
                    UniqueId = 42.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelBigint,
                    ContentTypeId = 1034,
                    PropertyTypeGroupId = 52,
                    Alias = Constants.Conventions.Media.Bytes,
                    Name = "Size",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in bytes",
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<PropertyTypeGroupDto>(53))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 43,
                    UniqueId = 43.ToGuid(),
                    DataTypeId = Constants.DataTypes.UploadAudio,
                    ContentTypeId = 1035,
                    PropertyTypeGroupId = 53,
                    Alias = Constants.Conventions.Media.File,
                    Name = "Audio",
                    SortOrder = 0,
                    Mandatory = true,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 44,
                    UniqueId = 44.ToGuid(),
                    DataTypeId = -92,
                    ContentTypeId = 1035,
                    PropertyTypeGroupId = 53,
                    Alias = Constants.Conventions.Media.Extension,
                    Name = "Type",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 45,
                    UniqueId = 45.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelBigint,
                    ContentTypeId = 1035,
                    PropertyTypeGroupId = 53,
                    Alias = Constants.Conventions.Media.Bytes,
                    Name = "Size",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in bytes",
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<PropertyTypeGroupDto>(54))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 46,
                    UniqueId = 46.ToGuid(),
                    DataTypeId = Constants.DataTypes.UploadArticle,
                    ContentTypeId = 1036,
                    PropertyTypeGroupId = 54,
                    Alias = Constants.Conventions.Media.File,
                    Name = "Article",
                    SortOrder = 0,
                    Mandatory = true,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 47,
                    UniqueId = 47.ToGuid(),
                    DataTypeId = -92,
                    ContentTypeId = 1036,
                    PropertyTypeGroupId = 54,
                    Alias = Constants.Conventions.Media.Extension,
                    Name = "Type",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 48,
                    UniqueId = 48.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelBigint,
                    ContentTypeId = 1036,
                    PropertyTypeGroupId = 54,
                    Alias = Constants.Conventions.Media.Bytes,
                    Name = "Size",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in bytes",
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        if (_database.Exists<PropertyTypeGroupDto>(55))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 49,
                    UniqueId = 49.ToGuid(),
                    DataTypeId = Constants.DataTypes.UploadVectorGraphics,
                    ContentTypeId = 1037,
                    PropertyTypeGroupId = 55,
                    Alias = Constants.Conventions.Media.File,
                    Name = "Vector Graphics",
                    SortOrder = 0,
                    Mandatory = true,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 50,
                    UniqueId = 50.ToGuid(),
                    DataTypeId = -92,
                    ContentTypeId = 1037,
                    PropertyTypeGroupId = 55,
                    Alias = Constants.Conventions.Media.Extension,
                    Name = "Type",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 51,
                    UniqueId = 51.ToGuid(),
                    DataTypeId = Constants.DataTypes.LabelBigint,
                    ContentTypeId = 1037,
                    PropertyTypeGroupId = 55,
                    Alias = Constants.Conventions.Media.Bytes,
                    Name = "Size",
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = "in bytes",
                    Variations = (byte)ContentVariation.Nothing,
                });
        }

        // Membership property types.
        if (_database.Exists<PropertyTypeGroupDto>(11))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false,
                new PropertyTypeDto
                {
                    Id = 28,
                    UniqueId = 28.ToGuid(),
                    DataTypeId = Constants.DataTypes.Textarea,
                    ContentTypeId = 1044,
                    PropertyTypeGroupId = 11,
                    Alias = Constants.Conventions.Member.Comments,
                    Name = Constants.Conventions.Member.CommentsLabel,
                    SortOrder = 0,
                    Mandatory = false,
                    ValidationRegExp = null,
                    Description = null,
                    Variations = (byte)ContentVariation.Nothing,
                });
        }
    }

    private void CreateLanguageData() =>
        ConditionalInsert(
            Constants.Configuration.NamedOptions.InstallDefaultData.Languages,
            "en-us",
            new LanguageDto { Id = 1, IsoCode = "en-US", CultureName = "English (United States)", IsDefault = true },
            Constants.DatabaseSchema.Tables.Language,
            "id");

    private void CreateContentChildTypeData()
    {
        // Insert data if the corresponding Node records exist (which may or may not have been created depending on configuration
        // of media types to create).
        if (!_database.Exists<NodeDto>(1031))
        {
            return;
        }

        _database.Insert(Constants.DatabaseSchema.Tables.ContentChildType, "Id", false,
            new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1031 });

        for (var i = 1032; i <= 1037; i++)
        {
            if (_database.Exists<NodeDto>(i))
            {
                _database.Insert(Constants.DatabaseSchema.Tables.ContentChildType, "Id", false,
                    new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = i });
            }
        }
    }

    private void CreateDataTypeData()
    {
        void InsertDataTypeDto(int id, string editorAlias, string dbType, string? configuration = null)
        {
            var dataTypeDto = new DataTypeDto { NodeId = id, EditorAlias = editorAlias, DbType = dbType };

            if (configuration != null)
            {
                dataTypeDto.Configuration = configuration;
            }

            if (_database.Exists<NodeDto>(id))
            {
                _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, dataTypeDto);
            }
        }

        // layouts for the list view
        const string cardLayout =
            "{\"name\": \"Grid\",\"path\": \"views/propertyeditors/listview/layouts/grid/grid.html\", \"icon\": \"icon-thumbnails-small\", \"isSystem\": 1, \"selected\": true}";
        const string listLayout =
            "{\"name\": \"List\",\"path\": \"views/propertyeditors/listview/layouts/list/list.html\",\"icon\": \"icon-list\", \"isSystem\": 1,\"selected\": true}";
        const string layouts = "[" + cardLayout + "," + listLayout + "]";

        // Insert data types only if the corresponding Node record exists (which may or may not have been created depending on configuration
        // of data types to create).
        if (_database.Exists<NodeDto>(Constants.DataTypes.Boolean))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.Boolean,
                    EditorAlias = Constants.PropertyEditors.Aliases.Boolean,
                    DbType = "Integer",
                });
        }

        if (_database.Exists<NodeDto>(-51))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = -51,
                    EditorAlias = Constants.PropertyEditors.Aliases.Integer,
                    DbType = "Integer",
                });
        }

        if (_database.Exists<NodeDto>(-87))
        {
            _database.Insert(
                Constants.DatabaseSchema.Tables.DataType,
                "pk",
                false,
                new DataTypeDto
                {
                    NodeId = -87,
                    EditorAlias = Constants.PropertyEditors.Aliases.TinyMce,
                    DbType = "Ntext",
                    Configuration =
                        "{\"value\":\",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|\"}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.Textbox))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.Textbox,
                    EditorAlias = Constants.PropertyEditors.Aliases.TextBox,
                    DbType = "Nvarchar",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.Textarea))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.Textarea,
                    EditorAlias = Constants.PropertyEditors.Aliases.TextArea,
                    DbType = "Ntext",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.Upload))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.Upload,
                    EditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                });
        }

        InsertDataTypeDto(Constants.DataTypes.LabelString, Constants.PropertyEditors.Aliases.Label, "Nvarchar",
            "{\"umbracoDataValueType\":\"STRING\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelInt, Constants.PropertyEditors.Aliases.Label, "Integer",
            "{\"umbracoDataValueType\":\"INT\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelBigint, Constants.PropertyEditors.Aliases.Label, "Nvarchar",
            "{\"umbracoDataValueType\":\"BIGINT\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelDateTime, Constants.PropertyEditors.Aliases.Label, "Date",
            "{\"umbracoDataValueType\":\"DATETIME\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelDecimal, Constants.PropertyEditors.Aliases.Label, "Decimal",
            "{\"umbracoDataValueType\":\"DECIMAL\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelTime, Constants.PropertyEditors.Aliases.Label, "Date",
            "{\"umbracoDataValueType\":\"TIME\"}");

        if (_database.Exists<NodeDto>(Constants.DataTypes.DateTime))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.DateTime,
                    EditorAlias = Constants.PropertyEditors.Aliases.DateTime,
                    DbType = "Date",
                });
        }

        if (_database.Exists<NodeDto>(-37))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = -37,
                    EditorAlias = Constants.PropertyEditors.Aliases.ColorPicker,
                    DbType = "Nvarchar",
                });
        }

        InsertDataTypeDto(Constants.DataTypes.DropDownSingle, Constants.PropertyEditors.Aliases.DropDownListFlexible,
            "Nvarchar", "{\"multiple\":false}");

        if (_database.Exists<NodeDto>(-40))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = -40,
                    EditorAlias = Constants.PropertyEditors.Aliases.RadioButtonList,
                    DbType = "Nvarchar",
                });
        }

        if (_database.Exists<NodeDto>(-41))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = -41,
                    EditorAlias = "Umbraco.DateTime",
                    DbType = "Date",
                    Configuration = "{\"format\":\"YYYY-MM-DD\"}",
                });
        }

        InsertDataTypeDto(Constants.DataTypes.DropDownMultiple, Constants.PropertyEditors.Aliases.DropDownListFlexible,
            "Nvarchar", "{\"multiple\":true}");

        if (_database.Exists<NodeDto>(-43))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = -43,
                    EditorAlias = Constants.PropertyEditors.Aliases.CheckBoxList,
                    DbType = "Nvarchar",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.Tags))
        {
            _database.Insert(
                Constants.DatabaseSchema.Tables.DataType,
                "pk",
                false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.Tags,
                    EditorAlias = Constants.PropertyEditors.Aliases.Tags,
                    DbType = "Ntext",
                    Configuration = "{\"group\":\"default\", \"storageType\":\"Json\"}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.ImageCropper))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.ImageCropper,
                    EditorAlias = Constants.PropertyEditors.Aliases.ImageCropper,
                    DbType = "Ntext",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.DefaultContentListView))
        {
            _database.Insert(
                Constants.DatabaseSchema.Tables.DataType,
                "pk",
                false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.DefaultContentListView,
                    EditorAlias = Constants.PropertyEditors.Aliases.ListView,
                    DbType = "Nvarchar",
                    Configuration =
                        "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" +
                        layouts +
                        ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.DefaultMediaListView))
        {
            _database.Insert(
                Constants.DatabaseSchema.Tables.DataType,
                "pk",
                false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.DefaultMediaListView,
                    EditorAlias = Constants.PropertyEditors.Aliases.ListView,
                    DbType = "Nvarchar",
                    Configuration =
                        "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" +
                        layouts +
                        ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.DefaultMembersListView))
        {
            _database.Insert(
                Constants.DatabaseSchema.Tables.DataType,
                "pk",
                false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.DefaultMembersListView,
                    EditorAlias = Constants.PropertyEditors.Aliases.ListView,
                    DbType = "Nvarchar",
                    Configuration =
                        "{\"pageSize\":10, \"orderBy\":\"username\", \"orderDirection\":\"asc\", \"includeProperties\":[{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]}",
                });
        }

        // New UDI pickers with newer Ids
        if (_database.Exists<NodeDto>(1046))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1046,
                    EditorAlias = Constants.PropertyEditors.Aliases.ContentPicker,
                    DbType = "Nvarchar",
                });
        }

        if (_database.Exists<NodeDto>(1047))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1047,
                    EditorAlias = Constants.PropertyEditors.Aliases.MemberPicker,
                    DbType = "Nvarchar",
                });
        }

        if (_database.Exists<NodeDto>(1048))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1048,
                    EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker,
                    DbType = "Ntext",
                });
        }

        if (_database.Exists<NodeDto>(1049))
        {
            _database.Insert(
                Constants.DatabaseSchema.Tables.DataType,
                "pk",
                false,
                new DataTypeDto
                {
                    NodeId = 1049,
                    EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker,
                    DbType = "Ntext",
                    Configuration = "{\"multiPicker\":1}",
                });
        }

        if (_database.Exists<NodeDto>(1050))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1050,
                    EditorAlias = Constants.PropertyEditors.Aliases.MultiUrlPicker,
                    DbType = "Ntext",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.UploadVideo))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.UploadVideo,
                    EditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration =
                        "{\"fileExtensions\":[{\"id\":0, \"value\":\"mp4\"}, {\"id\":1, \"value\":\"webm\"}, {\"id\":2, \"value\":\"ogv\"}]}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.UploadAudio))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.UploadAudio,
                    EditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration =
                        "{\"fileExtensions\":[{\"id\":0, \"value\":\"mp3\"}, {\"id\":1, \"value\":\"weba\"}, {\"id\":2, \"value\":\"oga\"}, {\"id\":3, \"value\":\"opus\"}]}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.UploadArticle))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.UploadArticle,
                    EditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration =
                        "{\"fileExtensions\":[{\"id\":0, \"value\":\"pdf\"}, {\"id\":1, \"value\":\"docx\"}, {\"id\":2, \"value\":\"doc\"}]}",
                });
        }

        if (_database.Exists<NodeDto>(Constants.DataTypes.UploadVectorGraphics))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = Constants.DataTypes.UploadVectorGraphics,
                    EditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Configuration = "{\"fileExtensions\":[{\"id\":0, \"value\":\"svg\"}]}",
                });
        }

        if (_database.Exists<NodeDto>(1051))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1051,
                    EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"multiple\": false, \"validationLimit\":{\"min\":0,\"max\":1}}",
                });
        }

        if (_database.Exists<NodeDto>(1052))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1052,
                    EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"multiple\": true}",
                });
        }

        if (_database.Exists<NodeDto>(1053))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1053,
                    EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"filter\":\"" + Constants.Conventions.MediaTypes.Image +
                                    "\", \"multiple\": false, \"validationLimit\":{\"min\":0,\"max\":1}}",
                });
        }

        if (_database.Exists<NodeDto>(1054))
        {
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false,
                new DataTypeDto
                {
                    NodeId = 1054,
                    EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Configuration = "{\"filter\":\"" + Constants.Conventions.MediaTypes.Image +
                                    "\", \"multiple\": true}",
                });
        }
    }

    private void CreateRelationTypeData()
    {
        CreateRelationTypeData(1, Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias,
            Constants.Conventions.RelationTypes.RelateDocumentOnCopyName, Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Document, true, false);
        CreateRelationTypeData(2, Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName, Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Document, false, false);
        CreateRelationTypeData(3, Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName, Constants.ObjectTypes.Media,
            Constants.ObjectTypes.Media, false, false);
        CreateRelationTypeData(4, Constants.Conventions.RelationTypes.RelatedMediaAlias,
            Constants.Conventions.RelationTypes.RelatedMediaName, null, null, false, true);
        CreateRelationTypeData(5, Constants.Conventions.RelationTypes.RelatedDocumentAlias,
            Constants.Conventions.RelationTypes.RelatedDocumentName, null, null, false, true);
    }

    private void CreateRelationTypeData(int id, string alias, string name, Guid? parentObjectType,
        Guid? childObjectType, bool dual, bool isDependency)
    {
        var relationType = new RelationTypeDto
        {
            Id = id,
            Alias = alias,
            ChildObjectType = childObjectType,
            ParentObjectType = parentObjectType,
            Dual = dual,
            Name = name,
            IsDependency = isDependency,
        };
        relationType.UniqueId = CreateUniqueRelationTypeId(relationType.Alias, relationType.Name);

        _database.Insert(Constants.DatabaseSchema.Tables.RelationType, "id", false, relationType);
    }

    private void CreateKeyValueData()
    {
        // On install, initialize the umbraco migration plan with the final state.
        var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion));
        var stateValueKey = upgrader.StateValueKey;
        var finalState = upgrader.Plan.FinalState;

        _database.Insert(Constants.DatabaseSchema.Tables.KeyValue, "key", false,
            new KeyValueDto { Key = stateValueKey, Value = finalState, UpdateDate = DateTime.Now });
    }

    private void CreateLogViewerQueryData()
    {
        LogViewerQueryDto[] defaultData = MigrateLogViewerQueriesFromFileToDb._defaultLogQueries.ToArray();

        for (var i = 0; i < defaultData.Length; i++)
        {
            LogViewerQueryDto dto = defaultData[i];
            dto.Id = i + 1;
            _database.Insert(Constants.DatabaseSchema.Tables.LogViewerQuery, "id", false, dto);
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
                           _entitiesToAlwaysCreate[configKey].InvariantContains(id);

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

        if (!alwaysInsert && installDefaultDataSettings?.InstallData == InstallDefaultDataOption.Values &&
            !installDefaultDataSettings.Values.InvariantContains(id))
        {
            return;
        }

        if (!alwaysInsert && installDefaultDataSettings?.InstallData == InstallDefaultDataOption.ExceptValues &&
            installDefaultDataSettings.Values.InvariantContains(id))
        {
            return;
        }

        _database.Insert(tableName, primaryKeyName, autoIncrement, dto);
    }
}
