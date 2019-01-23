using System;
using NPoco;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Install
{
    /// <summary>
    /// Creates the initial database data during install.
    /// </summary>
    internal class DatabaseDataCreator
    {
        private readonly IDatabase _database;
        private readonly ILogger _logger;

        public DatabaseDataCreator(IDatabase database, ILogger logger)
        {
            _database = database;
            _logger = logger;
        }

        /// <summary>
        /// Initialize the base data creation by inserting the data foundation for umbraco
        /// specific to a table
        /// </summary>
        /// <param name="tableName">Name of the table to create base data for</param>
        public void InitializeBaseData(string tableName)
        {
            _logger.Info<DatabaseDataCreator>("Creating data in {TableName}", tableName);

            if (tableName.Equals(Constants.DatabaseSchema.Tables.Node))
                CreateNodeData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.Lock))
                CreateLockData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.ContentType))
                CreateContentTypeData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.User))
                CreateUserData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.UserGroup))
                CreateUserGroupData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.User2UserGroup))
                CreateUser2UserGroupData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.UserGroup2App))
                CreateUserGroup2AppData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.PropertyTypeGroup))
                CreatePropertyTypeGroupData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.PropertyType))
                CreatePropertyTypeData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.Language))
                CreateLanguageData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.ContentChildType))
                CreateContentChildTypeData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.DataType))
                CreateDataTypeData();

            if (tableName.Equals(Constants.DatabaseSchema.Tables.RelationType))
                CreateRelationTypeData();
            
            if (tableName.Equals(Constants.DatabaseSchema.Tables.KeyValue))
                CreateKeyValueData();

            _logger.Info<DatabaseDataCreator>("Done creating table {TableName} data.", tableName);
        }

        private void CreateNodeData()
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
                    CreateDate = DateTime.Now
                };

                _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, nodeDto);
            }

            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -1, Trashed = false, ParentId = -1, UserId = -1, Level = 0, Path = "-1", SortOrder = 0, UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"), Text = "SYSTEM DATA: umbraco master root", NodeObjectType = Constants.ObjectTypes.SystemRoot, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -20, Trashed = false, ParentId = -1, UserId = -1, Level = 0, Path = "-1,-20", SortOrder = 0, UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"), Text = "Recycle Bin", NodeObjectType = Constants.ObjectTypes.ContentRecycleBin, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -21, Trashed = false, ParentId = -1, UserId = -1, Level = 0, Path = "-1,-21", SortOrder = 0, UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"), Text = "Recycle Bin", NodeObjectType = Constants.ObjectTypes.MediaRecycleBin, CreateDate = DateTime.Now });
            InsertDataTypeNodeDto(Constants.DataTypes.LabelString, 35, "f0bc4bfb-b499-40d6-ba86-058885a5178c", "Label");
            InsertDataTypeNodeDto(Constants.DataTypes.LabelInt, 36, "8e7f995c-bd81-4627-9932-c40e568ec788", "Label (integer)");
            InsertDataTypeNodeDto(Constants.DataTypes.LabelBigint, 36, "930861bf-e262-4ead-a704-f99453565708", "Label (bigint)");
            InsertDataTypeNodeDto(Constants.DataTypes.LabelDateTime, 37, "0e9794eb-f9b5-4f20-a788-93acd233a7e4", "Label (datetime)");
            InsertDataTypeNodeDto(Constants.DataTypes.LabelTime, 38, "a97cec69-9b71-4c30-8b12-ec398860d7e8", "Label (time)");
            InsertDataTypeNodeDto(Constants.DataTypes.LabelDecimal, 39, "8f1ef1e1-9de4-40d3-a072-6673f631ca64", "Label (decimal)");
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -90, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-90", SortOrder = 34, UniqueId = new Guid("84c6b441-31df-4ffe-b67e-67d5bc3ae65a"), Text = "Upload", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -89, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-89", SortOrder = 33, UniqueId = new Guid("c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3"), Text = "Textarea", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -88, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-88", SortOrder = 32, UniqueId = new Guid("0cc0eba1-9960-42c9-bf9b-60e150b429ae"), Text = "Textstring", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -87, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-87", SortOrder = 4, UniqueId = new Guid("ca90c950-0aff-4e72-b976-a30b1ac57dad"), Text = "Richtext editor", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -51, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-51", SortOrder = 2, UniqueId = new Guid("2e6d3631-066e-44b8-aec4-96f09099b2b5"), Text = "Numeric", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -49, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-49", SortOrder = 2, UniqueId = new Guid("92897bc6-a5f3-4ffe-ae27-f2e7e33dda49"), Text = "True/false", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -43, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-43", SortOrder = 2, UniqueId = new Guid("fbaf13a8-4036-41f2-93a3-974f678c312a"), Text = "Checkbox list", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = Constants.DataTypes.DropDownMultiple, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Constants.DataTypes.DropDownMultiple}", SortOrder = 2, UniqueId = new Guid("0b6a45e7-44ba-430d-9da5-4e46060b9e03"), Text = "Dropdown", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -41, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-41", SortOrder = 2, UniqueId = new Guid("5046194e-4237-453c-a547-15db3a07c4e1"), Text = "Date Picker", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -40, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-40", SortOrder = 2, UniqueId = new Guid("bb5f57c9-ce2b-4bb9-b697-4caca783a805"), Text = "Radiobox", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = Constants.DataTypes.DropDownSingle, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Constants.DataTypes.DropDownSingle}", SortOrder = 2, UniqueId = new Guid("f38f0ac7-1d27-439c-9f3f-089cd8825a53"), Text = "Dropdown multiple", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -37, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-37", SortOrder = 2, UniqueId = new Guid("0225af17-b302-49cb-9176-b9f35cab9c17"), Text = "Approved Color", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = -36, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,-36", SortOrder = 2, UniqueId = new Guid("e4d66c0f-b935-4200-81f0-025f7256b89a"), Text = "Date Picker with time", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = Constants.DataTypes.DefaultContentListView, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Constants.DataTypes.DefaultContentListView}", SortOrder = 2, UniqueId = new Guid("C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Content", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = Constants.DataTypes.DefaultMediaListView, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Constants.DataTypes.DefaultMediaListView}", SortOrder = 2, UniqueId = new Guid("3A0156C4-3B8C-4803-BDC1-6871FAA83FFF"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Media", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = Constants.DataTypes.DefaultMembersListView, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = $"-1,{Constants.DataTypes.DefaultMembersListView}", SortOrder = 2, UniqueId = new Guid("AA2C52A0-CE87-4E65-A47C-7DF09358585D"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Members", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1031, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1031", SortOrder = 2, UniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d"), Text = Constants.Conventions.MediaTypes.Folder, NodeObjectType = Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1032, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1032", SortOrder = 2, UniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8"), Text = Constants.Conventions.MediaTypes.Image, NodeObjectType = Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1033, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1033", SortOrder = 2, UniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d"), Text = Constants.Conventions.MediaTypes.File, NodeObjectType = Constants.ObjectTypes.MediaType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1041, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1041", SortOrder = 2, UniqueId = new Guid("b6b73142-b9c1-4bf8-a16d-e1c23320b549"), Text = "Tags", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1043, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1043", SortOrder = 2, UniqueId = new Guid("1df9f033-e6d4-451f-b8d2-e0cbc50a836f"), Text = "Image Cropper", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1044, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1044", SortOrder = 0, UniqueId = new Guid("d59be02f-1df9-4228-aa1e-01917d806cda"), Text = Constants.Conventions.MemberTypes.DefaultAlias, NodeObjectType = Constants.ObjectTypes.MemberType, CreateDate = DateTime.Now });

            //New UDI pickers with newer Ids
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1046, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1046", SortOrder = 2, UniqueId = new Guid("FD1E0DA5-5606-4862-B679-5D0CF3A52A59"), Text = "Content Picker", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1047, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1047", SortOrder = 2, UniqueId = new Guid("1EA2E01F-EBD8-4CE1-8D71-6B1149E63548"), Text = "Member Picker", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1048, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1048", SortOrder = 2, UniqueId = new Guid("135D60E0-64D9-49ED-AB08-893C9BA44AE5"), Text = "Media Picker", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1049, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1049", SortOrder = 2, UniqueId = new Guid("9DBBCBBB-2327-434A-B355-AF1B84E5010A"), Text = "Multiple Media Picker", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            _database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, new NodeDto { NodeId = 1050, Trashed = false, ParentId = -1, UserId = -1, Level = 1, Path = "-1,1050", SortOrder = 2, UniqueId = new Guid("B4E3535A-1753-47E2-8568-602CF8CFEE6F"), Text = "Multi URL Picker", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
        }

        private void CreateLockData()
        {
            // all lock objects
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.Servers, Name = "Servers" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.ContentTypes, Name = "ContentTypes" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.ContentTree, Name = "ContentTree" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.MediaTypes, Name = "MediaTypes" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.MediaTree, Name = "MediaTree" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.MemberTypes, Name = "MemberTypes" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.MemberTree, Name = "MemberTree" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.Domains, Name = "Domains" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.KeyValues, Name = "KeyValues" });
            _database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.Languages, Name = "Languages" });
        }

        private void CreateContentTypeData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 532, NodeId = 1031, Alias = Constants.Conventions.MediaTypes.Folder, Icon = "icon-folder", Thumbnail = "icon-folder", IsContainer = false, AllowAtRoot = true, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 533, NodeId = 1032, Alias = Constants.Conventions.MediaTypes.Image, Icon = "icon-picture", Thumbnail = "icon-picture", AllowAtRoot = true, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 534, NodeId = 1033, Alias = Constants.Conventions.MediaTypes.File, Icon = "icon-document", Thumbnail = "icon-document", AllowAtRoot = true, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false, new ContentTypeDto { PrimaryKey = 531, NodeId = 1044, Alias = Constants.Conventions.MemberTypes.DefaultAlias, Icon = "icon-user", Thumbnail = "icon-user", Variations = (byte) ContentVariation.Nothing });
        }

        private void CreateUserData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.User, "id", false, new UserDto { Id = Constants.Security.SuperUserId, Disabled = false, NoConsole = false, UserName = "Administrator", Login = "admin", Password = "default", Email = "", UserLanguage = "en-US", CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
        }

        private void CreateUserGroupData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 1, StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.AdminGroupAlias, Name = "Administrators", DefaultPermissions = "CADMOSKTPIURZ:5F7ï", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-medal" });
            _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 2, StartMediaId = -1, StartContentId = -1, Alias = "writer", Name = "Writers", DefaultPermissions = "CAH:F", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-edit" });
            _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 3, StartMediaId = -1, StartContentId = -1, Alias = "editor", Name = "Editors", DefaultPermissions = "CADMOSKTPUZ:5Fï", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-tools" });
            _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 4, StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.TranslatorGroupAlias, Name = "Translators", DefaultPermissions = "AF", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-globe" });
            _database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", false, new UserGroupDto { Id = 5, StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.SensitiveDataGroupAlias, Name = "Sensitive data", DefaultPermissions = "", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-lock" });
        }

        private void CreateUser2UserGroupData()
        {
            _database.Insert(new User2UserGroupDto { UserGroupId = 1, UserId = Constants.Security.SuperUserId }); // add super to admins
            _database.Insert(new User2UserGroupDto { UserGroupId = 5, UserId = Constants.Security.SuperUserId }); // add super to sensitive data
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

            _database.Insert(new UserGroup2AppDto { UserGroupId = 2, AppAlias = Constants.Applications.Content });

            _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Constants.Applications.Content });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Constants.Applications.Media });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 3, AppAlias = Constants.Applications.Forms });

            _database.Insert(new UserGroup2AppDto { UserGroupId = 4, AppAlias = Constants.Applications.Translation });
        }

        private void CreatePropertyTypeGroupData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 3, ContentTypeNodeId = 1032, Text = "Image", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.Image) });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 4, ContentTypeNodeId = 1033, Text = "File", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.File) });
            //membership property group
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyTypeGroup, "id", false, new PropertyTypeGroupDto { Id = 11, ContentTypeNodeId = 1044, Text = "Membership", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.Membership) });
        }

        private void CreatePropertyTypeData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 6, UniqueId = 6.ToGuid(), DataTypeId = 1043, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.File, Name = "Upload image", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 7, UniqueId = 7.ToGuid(), DataTypeId = Constants.DataTypes.LabelInt, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Width, Name = "Width", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 8, UniqueId = 8.ToGuid(), DataTypeId = Constants.DataTypes.LabelInt, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Height, Name = "Height", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 9, UniqueId = 9.ToGuid(), DataTypeId = Constants.DataTypes.LabelBigint, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 10, UniqueId = 10.ToGuid(), DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 24, UniqueId = 24.ToGuid(), DataTypeId = -90, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.File, Name = "Upload file", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 25, UniqueId = 25.ToGuid(), DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 26, UniqueId = 26.ToGuid(), DataTypeId = Constants.DataTypes.LabelBigint, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            //membership property types
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 28, UniqueId = 28.ToGuid(), DataTypeId = -89, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.Comments, Name = Constants.Conventions.Member.CommentsLabel, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 29, UniqueId = 29.ToGuid(), DataTypeId = Constants.DataTypes.LabelInt, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.FailedPasswordAttempts, Name = Constants.Conventions.Member.FailedPasswordAttemptsLabel, SortOrder = 1, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 30, UniqueId = 30.ToGuid(), DataTypeId = -49, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.IsApproved, Name = Constants.Conventions.Member.IsApprovedLabel, SortOrder = 2, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 31, UniqueId = 31.ToGuid(), DataTypeId = -49, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.IsLockedOut, Name = Constants.Conventions.Member.IsLockedOutLabel, SortOrder = 3, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 32, UniqueId = 32.ToGuid(), DataTypeId = Constants.DataTypes.LabelDateTime, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastLockoutDate, Name = Constants.Conventions.Member.LastLockoutDateLabel, SortOrder = 4, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 33, UniqueId = 33.ToGuid(), DataTypeId = Constants.DataTypes.LabelDateTime, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastLoginDate, Name = Constants.Conventions.Member.LastLoginDateLabel, SortOrder = 5, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });
            _database.Insert(Constants.DatabaseSchema.Tables.PropertyType, "id", false, new PropertyTypeDto { Id = 34, UniqueId = 34.ToGuid(), DataTypeId = Constants.DataTypes.LabelDateTime, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastPasswordChangeDate, Name = Constants.Conventions.Member.LastPasswordChangeDateLabel, SortOrder = 6, Mandatory = false, ValidationRegExp = null, Description = null, Variations = (byte) ContentVariation.Nothing });

        }

        private void CreateLanguageData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.Language, "id", false, new LanguageDto { Id = 1, IsoCode = "en-US", CultureName = "English (United States)", IsDefault = true });
        }

        private void CreateContentChildTypeData()
        {
            _database.Insert(Constants.DatabaseSchema.Tables.ContentChildType, "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1031 });
            _database.Insert(Constants.DatabaseSchema.Tables.ContentChildType, "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1032 });
            _database.Insert(Constants.DatabaseSchema.Tables.ContentChildType, "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1033 });
        }

        private void CreateDataTypeData()
        {
            void InsertDataTypeDto(int id, string editorAlias, string dbType, string configuration = null)
            {
                var dataTypeDto = new DataTypeDto
                {
                    NodeId = id,
                    EditorAlias = editorAlias,
                    DbType = dbType
                };

                if (configuration != null)
                    dataTypeDto.Configuration = configuration;

                _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, dataTypeDto);
            }

            //layouts for the list view
            const string cardLayout = "{\"name\": \"Grid\",\"path\": \"views/propertyeditors/listview/layouts/grid/grid.html\", \"icon\": \"icon-thumbnails-small\", \"isSystem\": 1, \"selected\": true}";
            const string listLayout = "{\"name\": \"List\",\"path\": \"views/propertyeditors/listview/layouts/list/list.html\",\"icon\": \"icon-list\", \"isSystem\": 1,\"selected\": true}";
            const string layouts = "[" + cardLayout + "," + listLayout + "]";

            //TODO Check which of the DataTypeIds below doesn't exist in umbracoNode, which results in a foreign key constraint errors.
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -49, EditorAlias = Constants.PropertyEditors.Aliases.Boolean, DbType = "Integer" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -51, EditorAlias = Constants.PropertyEditors.Aliases.Integer, DbType = "Integer" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -87, EditorAlias = Constants.PropertyEditors.Aliases.TinyMce, DbType = "Ntext",
                Configuration = "{\"value\":\",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|\"}" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -88, EditorAlias = Constants.PropertyEditors.Aliases.TextBox, DbType = "Nvarchar" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -89, EditorAlias = Constants.PropertyEditors.Aliases.TextArea, DbType = "Ntext" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -90, EditorAlias = Constants.PropertyEditors.Aliases.UploadField, DbType = "Nvarchar" });
            InsertDataTypeDto(Constants.DataTypes.LabelString, Constants.PropertyEditors.Aliases.NoEdit, "Nvarchar", "{\"umbracoDataValueType\":\"STRING\"}");
            InsertDataTypeDto(Constants.DataTypes.LabelInt, Constants.PropertyEditors.Aliases.NoEdit, "Integer", "{\"umbracoDataValueType\":\"INT\"}");
            InsertDataTypeDto(Constants.DataTypes.LabelBigint, Constants.PropertyEditors.Aliases.NoEdit, "Nvarchar", "{\"umbracoDataValueType\":\"BIGINT\"}");
            InsertDataTypeDto(Constants.DataTypes.LabelDateTime, Constants.PropertyEditors.Aliases.NoEdit, "Date", "{\"umbracoDataValueType\":\"DATETIME\"}");
            InsertDataTypeDto(Constants.DataTypes.LabelDecimal, Constants.PropertyEditors.Aliases.NoEdit, "Decimal", "{\"umbracoDataValueType\":\"DECIMAL\"}");
            InsertDataTypeDto(Constants.DataTypes.LabelTime, Constants.PropertyEditors.Aliases.NoEdit, "Date", "{\"umbracoDataValueType\":\"TIME\"}");
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -36, EditorAlias = Constants.PropertyEditors.Aliases.DateTime, DbType = "Date" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -37, EditorAlias = Constants.PropertyEditors.Aliases.ColorPicker, DbType = "Nvarchar" });
            InsertDataTypeDto(Constants.DataTypes.DropDownSingle, Constants.PropertyEditors.Aliases.DropDownListFlexible, "Nvarchar", "{\"multiple\":false}");
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -40, EditorAlias = Constants.PropertyEditors.Aliases.RadioButtonList, DbType = "Nvarchar" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -41, EditorAlias = Constants.PropertyEditors.Aliases.Date, DbType = "Date" });
            InsertDataTypeDto(Constants.DataTypes.DropDownMultiple, Constants.PropertyEditors.Aliases.DropDownListFlexible, "Nvarchar", "{\"multiple\":true}");
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = -43, EditorAlias = Constants.PropertyEditors.Aliases.CheckBoxList, DbType = "Nvarchar" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1041, EditorAlias = Constants.PropertyEditors.Aliases.Tags, DbType = "Ntext",
                Configuration = "{\"group\":\"default\", \"storageType\":\"Json\"}"
            });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1043, EditorAlias = Constants.PropertyEditors.Aliases.ImageCropper, DbType = "Ntext" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Constants.DataTypes.DefaultContentListView, EditorAlias = Constants.PropertyEditors.Aliases.ListView, DbType = "Nvarchar",
                Configuration = "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" + layouts + ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Constants.DataTypes.DefaultMediaListView, EditorAlias = Constants.PropertyEditors.Aliases.ListView, DbType = "Nvarchar",
                Configuration = "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" + layouts + ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = Constants.DataTypes.DefaultMembersListView, EditorAlias = Constants.PropertyEditors.Aliases.ListView, DbType = "Nvarchar",
                Configuration = "{\"pageSize\":10, \"orderBy\":\"username\", \"orderDirection\":\"asc\", \"includeProperties\":[{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]}" });

            //New UDI pickers with newer Ids
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1046, EditorAlias = Constants.PropertyEditors.Aliases.ContentPicker, DbType = "Nvarchar" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1047, EditorAlias = Constants.PropertyEditors.Aliases.MemberPicker, DbType = "Nvarchar" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1048, EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker, DbType = "Ntext" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1049, EditorAlias = Constants.PropertyEditors.Aliases.MediaPicker, DbType = "Ntext",
                Configuration = "{\"multiPicker\":1}" });
            _database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { NodeId = 1050, EditorAlias = Constants.PropertyEditors.Aliases.MultiUrlPicker, DbType = "Ntext" });
        }

        private void CreateRelationTypeData()
        {
            var relationType = new RelationTypeDto { Id = 1, Alias = Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias, ChildObjectType = Constants.ObjectTypes.Document, ParentObjectType = Constants.ObjectTypes.Document, Dual = true, Name = Constants.Conventions.RelationTypes.RelateDocumentOnCopyName };
            relationType.UniqueId = (relationType.Alias + "____" + relationType.Name).ToGuid();
            _database.Insert(Constants.DatabaseSchema.Tables.RelationType, "id", false, relationType);
            relationType = new RelationTypeDto { Id = 2, Alias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias, ChildObjectType = Constants.ObjectTypes.Document, ParentObjectType = Constants.ObjectTypes.Document, Dual = false, Name = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName };
            relationType.UniqueId = (relationType.Alias + "____" + relationType.Name).ToGuid();
            _database.Insert(Constants.DatabaseSchema.Tables.RelationType, "id", false, relationType);
            relationType = new RelationTypeDto { Id = 3, Alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias, ChildObjectType = Constants.ObjectTypes.Media, ParentObjectType = Constants.ObjectTypes.Media, Dual = false, Name = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName };
            relationType.UniqueId = (relationType.Alias + "____" + relationType.Name).ToGuid();
            _database.Insert(Constants.DatabaseSchema.Tables.RelationType, "id", false, relationType);
        }

        private void CreateKeyValueData()
        {
            // on install, initialize the umbraco migration plan with the final state

            var upgrader = new UmbracoUpgrader();
            var stateValueKey = upgrader.StateValueKey;
            var finalState = upgrader.Plan.FinalState;

            _database.Insert(Constants.DatabaseSchema.Tables.KeyValue, "key", false, new KeyValueDto { Key = stateValueKey, Value = finalState, Updated = DateTime.Now });
        }
    }
}
