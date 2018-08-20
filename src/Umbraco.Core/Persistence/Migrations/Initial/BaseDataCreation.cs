using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    /// <summary>
    /// Represents the initial data creation by running Insert for the base data.
    /// </summary>
    internal class BaseDataCreation
    {
        private readonly Database _database;
        private readonly ILogger _logger;

        public BaseDataCreation(Database database, ILogger logger)
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
            _logger.Info<BaseDataCreation>(string.Format("Creating data in table {0}", tableName));

            if(tableName.Equals("umbracoNode"))
            {
                CreateUmbracNodeData();
            }

            if (tableName.Equals("umbracoLock"))
            {
                CreateUmbracoLockData();
            }

            if (tableName.Equals("cmsContentType"))
            {
                CreateCmsContentTypeData();
            }

            if (tableName.Equals("umbracoUser"))
            {
                CreateUmbracoUserData();
            }

            if (tableName.Equals("umbracoUserGroup"))
            {
                CreateUmbracoUserGroupData();
            }

            if (tableName.Equals("umbracoUser2UserGroup"))
            {
                CreateUmbracoUser2UserGroupData();
            }

            if (tableName.Equals("umbracoUserGroup2App"))
            {
                CreateUmbracoUserGroup2AppData();
            }

            if (tableName.Equals("cmsPropertyTypeGroup"))
            {
                CreateCmsPropertyTypeGroupData();
            }

            if (tableName.Equals("cmsPropertyType"))
            {
                CreateCmsPropertyTypeData();
            }

            if (tableName.Equals("umbracoLanguage"))
            {
                CreateUmbracoLanguageData();
            }

            if (tableName.Equals("cmsContentTypeAllowedContentType"))
            {
                CreateCmsContentTypeAllowedContentTypeData();
            }

            if(tableName.Equals("cmsDataType"))
            {
                CreateCmsDataTypeData();
            }

            if (tableName.Equals("cmsDataTypePreValues"))
            {
                CreateCmsDataTypePreValuesData();
            }

            if (tableName.Equals("umbracoRelationType"))
            {
                CreateUmbracoRelationTypeData();
            }

            if (tableName.Equals("cmsTaskType"))
            {
                CreateCmsTaskTypeData();
            }

            if (tableName.Equals("umbracoMigration"))
            {
                CreateUmbracoMigrationData();
            }

            _logger.Info<BaseDataCreation>(string.Format("Done creating data in table {0}", tableName));
        }

        private void CreateUmbracNodeData()
        {
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -1, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1", SortOrder = 0, UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"), Text = "SYSTEM DATA: umbraco master root", NodeObjectType = new Guid(Constants.ObjectTypes.SystemRoot), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -20, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,-20", SortOrder = 0, UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"), Text = "Recycle Bin", NodeObjectType = new Guid(Constants.ObjectTypes.ContentRecycleBin), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -21, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,-21", SortOrder = 0, UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"), Text = "Recycle Bin", NodeObjectType = new Guid(Constants.ObjectTypes.MediaRecycleBin), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -92, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-92", SortOrder = 35, UniqueId = new Guid("f0bc4bfb-b499-40d6-ba86-058885a5178c"), Text = "Label", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -90, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-90", SortOrder = 34, UniqueId = new Guid("84c6b441-31df-4ffe-b67e-67d5bc3ae65a"), Text = "Upload", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -89, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-89", SortOrder = 33, UniqueId = new Guid("c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3"), Text = "Textarea", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -88, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-88", SortOrder = 32, UniqueId = new Guid("0cc0eba1-9960-42c9-bf9b-60e150b429ae"), Text = "Textstring", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -87, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-87", SortOrder = 4, UniqueId = new Guid("ca90c950-0aff-4e72-b976-a30b1ac57dad"), Text = "Richtext editor", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -51, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-51", SortOrder = 2, UniqueId = new Guid("2e6d3631-066e-44b8-aec4-96f09099b2b5"), Text = "Numeric", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -49, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-49", SortOrder = 2, UniqueId = new Guid("92897bc6-a5f3-4ffe-ae27-f2e7e33dda49"), Text = "Checkbox", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -43, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-43", SortOrder = 2, UniqueId = new Guid("fbaf13a8-4036-41f2-93a3-974f678c312a"), Text = "Checkbox list", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -42, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-42", SortOrder = 2, UniqueId = new Guid("0b6a45e7-44ba-430d-9da5-4e46060b9e03"), Text = "Dropdown", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -41, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-41", SortOrder = 2, UniqueId = new Guid("5046194e-4237-453c-a547-15db3a07c4e1"), Text = "Date Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -40, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-40", SortOrder = 2, UniqueId = new Guid("bb5f57c9-ce2b-4bb9-b697-4caca783a805"), Text = "Radiobox", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -39, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-39", SortOrder = 2, UniqueId = new Guid("f38f0ac7-1d27-439c-9f3f-089cd8825a53"), Text = "Dropdown multiple", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });            
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -37, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-37", SortOrder = 2, UniqueId = new Guid("0225af17-b302-49cb-9176-b9f35cab9c17"), Text = "Approved Color", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -36, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-36", SortOrder = 2, UniqueId = new Guid("e4d66c0f-b935-4200-81f0-025f7256b89a"), Text = "Date Picker with time", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.System.DefaultContentListViewDataTypeId, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-95", SortOrder = 2, UniqueId = new Guid("C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Content", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.System.DefaultMediaListViewDataTypeId, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-96", SortOrder = 2, UniqueId = new Guid("3A0156C4-3B8C-4803-BDC1-6871FAA83FFF"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Media", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.System.DefaultMembersListViewDataTypeId, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-97", SortOrder = 2, UniqueId = new Guid("AA2C52A0-CE87-4E65-A47C-7DF09358585D"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Members", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });                                    
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1031, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1031", SortOrder = 2, UniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d"), Text = Constants.Conventions.MediaTypes.Folder, NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1032, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1032", SortOrder = 2, UniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8"), Text = Constants.Conventions.MediaTypes.Image, NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1033, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1033", SortOrder = 2, UniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d"), Text = Constants.Conventions.MediaTypes.File, NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });                        
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1041, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1041", SortOrder = 2, UniqueId = new Guid("b6b73142-b9c1-4bf8-a16d-e1c23320b549"), Text = "Tags", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1043, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1043", SortOrder = 2, UniqueId = new Guid("1df9f033-e6d4-451f-b8d2-e0cbc50a836f"), Text = "Image Cropper", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1044, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1044", SortOrder = 0, UniqueId = new Guid("d59be02f-1df9-4228-aa1e-01917d806cda"), Text = Constants.Conventions.MemberTypes.DefaultAlias, NodeObjectType = new Guid(Constants.ObjectTypes.MemberType), CreateDate = DateTime.Now });

            //New UDI pickers with newer Ids
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1046, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1046", SortOrder = 2, UniqueId = new Guid("FD1E0DA5-5606-4862-B679-5D0CF3A52A59"), Text = "Content Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1047, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1047", SortOrder = 2, UniqueId = new Guid("1EA2E01F-EBD8-4CE1-8D71-6B1149E63548"), Text = "Member Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1048, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1048", SortOrder = 2, UniqueId = new Guid("135D60E0-64D9-49ED-AB08-893C9BA44AE5"), Text = "Media Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1049, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1049", SortOrder = 2, UniqueId = new Guid("9DBBCBBB-2327-434A-B355-AF1B84E5010A"), Text = "Multiple Media Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1050, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1050", SortOrder = 2, UniqueId = new Guid("B4E3535A-1753-47E2-8568-602CF8CFEE6F"), Text = "Related Links", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
        }

        private void CreateUmbracoLockData()
        {
            // all lock objects
            _database.Insert("umbracoLock", "id", false, new LockDto { Id = Constants.Locks.Servers, Name = "Servers" });
        }

        private void CreateCmsContentTypeData()
        {
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 532, NodeId = 1031, Alias = Constants.Conventions.MediaTypes.Folder, Icon = "icon-folder", Thumbnail = "icon-folder", IsContainer = false, AllowAtRoot = true });
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 533, NodeId = 1032, Alias = Constants.Conventions.MediaTypes.Image, Icon = "icon-picture", Thumbnail = "icon-picture", AllowAtRoot = true });
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 534, NodeId = 1033, Alias = Constants.Conventions.MediaTypes.File, Icon = "icon-document", Thumbnail = "icon-document", AllowAtRoot = true });
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 531, NodeId = 1044, Alias = Constants.Conventions.MemberTypes.DefaultAlias, Icon = "icon-user", Thumbnail = "icon-user" });
        }

        private void CreateUmbracoUserData()
        {
            _database.Insert("umbracoUser", "id", false, new UserDto { Id = 0, Disabled = false, NoConsole = false, UserName = "Administrator", Login = "admin", Password = "default", Email = "", UserLanguage = "en-US", CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
        }
        
        private void CreateUmbracoUserGroupData()
        {
            _database.Insert("umbracoUserGroup", "id", false, new UserGroupDto { Id = 1, StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.AdminGroupAlias, Name = "Administrators", DefaultPermissions = "CADMOSKTPIURZ:5F7ï", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-medal" });
            _database.Insert("umbracoUserGroup", "id", false, new UserGroupDto { Id = 2, StartMediaId = -1, StartContentId = -1, Alias = "writer", Name = "Writers", DefaultPermissions = "CAH:F", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-edit" });
            _database.Insert("umbracoUserGroup", "id", false, new UserGroupDto { Id = 3, StartMediaId = -1, StartContentId = -1, Alias = "editor", Name = "Editors", DefaultPermissions = "CADMOSKTPUZ:5Fï", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-tools" });
            _database.Insert("umbracoUserGroup", "id", false, new UserGroupDto { Id = 4, StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.TranslatorGroupAlias, Name = "Translators", DefaultPermissions = "AF", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-globe" });
            _database.Insert("umbracoUserGroup", "id", false, new UserGroupDto { Id = 5, StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.SensitiveDataGroupAlias, Name = "Sensitive data", DefaultPermissions = "", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-lock" });
        }

        private void CreateUmbracoUser2UserGroupData()
        {
            _database.Insert(new User2UserGroupDto { UserGroupId = 1, UserId = 0 }); //add admin to admins
            _database.Insert(new User2UserGroupDto { UserGroupId = 5, UserId = 0 }); //add admin to sensitive data
        }

        private void CreateUmbracoUserGroup2AppData()
        {
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Content });
            _database.Insert(new UserGroup2AppDto { UserGroupId = 1, AppAlias = Constants.Applications.Developer });
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
        
        private void CreateCmsPropertyTypeGroupData()
        {          
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 3, ContentTypeNodeId = 1032, Text = "Image", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.Image) });
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 4, ContentTypeNodeId = 1033, Text = "File", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.File) });
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 5, ContentTypeNodeId = 1031, Text = "Contents", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.Contents) });
            //membership property group
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 11, ContentTypeNodeId = 1044, Text = "Membership", SortOrder = 1, UniqueId = new Guid(Constants.PropertyTypeGroups.Membership) });
        }

        private void CreateCmsPropertyTypeData()
        {
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 6, UniqueId = 6.ToGuid(), DataTypeId = 1043, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.File, Name = "Upload image", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 7, UniqueId = 7.ToGuid(), DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Width, Name = "Width", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 8, UniqueId = 8.ToGuid(), DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Height, Name = "Height", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 9, UniqueId = 9.ToGuid(), DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 10, UniqueId = 10.ToGuid(), DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 24, UniqueId = 24.ToGuid(), DataTypeId = -90, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.File, Name = "Upload file", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 25, UniqueId = 25.ToGuid(), DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 26, UniqueId = 26.ToGuid(), DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 27, UniqueId = 27.ToGuid(), DataTypeId = Constants.System.DefaultMediaListViewDataTypeId, ContentTypeId = 1031, PropertyTypeGroupId = 5, Alias = "contents", Name = "Contents:", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            //membership property types
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 28, UniqueId = 28.ToGuid(), DataTypeId = -89, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.Comments, Name = Constants.Conventions.Member.CommentsLabel, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 29, UniqueId = 29.ToGuid(), DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.FailedPasswordAttempts, Name = Constants.Conventions.Member.FailedPasswordAttemptsLabel, SortOrder = 1, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 30, UniqueId = 30.ToGuid(), DataTypeId = -49, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.IsApproved, Name = Constants.Conventions.Member.IsApprovedLabel, SortOrder = 2, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 31, UniqueId = 31.ToGuid(), DataTypeId = -49, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.IsLockedOut, Name = Constants.Conventions.Member.IsLockedOutLabel, SortOrder = 3, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 32, UniqueId = 32.ToGuid(), DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastLockoutDate, Name = Constants.Conventions.Member.LastLockoutDateLabel, SortOrder = 4, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 33, UniqueId = 33.ToGuid(), DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastLoginDate, Name = Constants.Conventions.Member.LastLoginDateLabel, SortOrder = 5, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 34, UniqueId = 34.ToGuid(), DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastPasswordChangeDate, Name = Constants.Conventions.Member.LastPasswordChangeDateLabel, SortOrder = 6, Mandatory = false, ValidationRegExp = null, Description = null });
            
        }

        private void CreateUmbracoLanguageData()
        {
            _database.Insert("umbracoLanguage", "id", false, new LanguageDto { Id = 1, IsoCode = "en-US", CultureName = "en-US" });
        }

        private void CreateCmsContentTypeAllowedContentTypeData()
        {
            _database.Insert("cmsContentTypeAllowedContentType", "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1031 });
            _database.Insert("cmsContentTypeAllowedContentType", "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1032 });
            _database.Insert("cmsContentTypeAllowedContentType", "Id", false, new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1033 });
        }

        private void CreateCmsDataTypeData()
        {
            //TODO Check which of the DataTypeIds below doesn't exist in umbracoNode, which results in a foreign key constraint errors.
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 1, DataTypeId = -49, PropertyEditorAlias = Constants.PropertyEditors.TrueFalseAlias, DbType = "Integer" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 2, DataTypeId = -51, PropertyEditorAlias = Constants.PropertyEditors.IntegerAlias, DbType = "Integer" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 3, DataTypeId = -87, PropertyEditorAlias = Constants.PropertyEditors.TinyMCEAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 4, DataTypeId = -88, PropertyEditorAlias = Constants.PropertyEditors.TextboxAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 5, DataTypeId = -89, PropertyEditorAlias = Constants.PropertyEditors.TextboxMultipleAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 6, DataTypeId = -90, PropertyEditorAlias = Constants.PropertyEditors.UploadFieldAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 7, DataTypeId = -92, PropertyEditorAlias = Constants.PropertyEditors.NoEditAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 8, DataTypeId = -36, PropertyEditorAlias = Constants.PropertyEditors.DateTimeAlias, DbType = "Date" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 9, DataTypeId = -37, PropertyEditorAlias = Constants.PropertyEditors.ColorPickerAlias, DbType = "Nvarchar" });            
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 11, DataTypeId = -39, PropertyEditorAlias = Constants.PropertyEditors.DropDownListMultipleAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 12, DataTypeId = -40, PropertyEditorAlias = Constants.PropertyEditors.RadioButtonListAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 13, DataTypeId = -41, PropertyEditorAlias = Constants.PropertyEditors.DateAlias, DbType = "Date" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 14, DataTypeId = -42, PropertyEditorAlias = Constants.PropertyEditors.DropDownListAlias, DbType = "Integer" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 15, DataTypeId = -43, PropertyEditorAlias = Constants.PropertyEditors.CheckBoxListAlias, DbType = "Nvarchar" });                        
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 22, DataTypeId = 1041, PropertyEditorAlias = Constants.PropertyEditors.TagsAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 24, DataTypeId = 1043, PropertyEditorAlias = Constants.PropertyEditors.ImageCropperAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = -26, DataTypeId = Constants.System.DefaultContentListViewDataTypeId, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = -27, DataTypeId = Constants.System.DefaultMediaListViewDataTypeId, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = -28, DataTypeId = Constants.System.DefaultMembersListViewDataTypeId, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });

            //New UDI pickers with newer Ids
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 26, DataTypeId = 1046, PropertyEditorAlias = Constants.PropertyEditors.ContentPicker2Alias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 27, DataTypeId = 1047, PropertyEditorAlias = Constants.PropertyEditors.MemberPicker2Alias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 28, DataTypeId = 1048, PropertyEditorAlias = Constants.PropertyEditors.MediaPicker2Alias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 29, DataTypeId = 1049, PropertyEditorAlias = Constants.PropertyEditors.MediaPicker2Alias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 30, DataTypeId = 1050, PropertyEditorAlias = Constants.PropertyEditors.RelatedLinks2Alias, DbType = "Ntext" });
        }

        private void CreateCmsDataTypePreValuesData()
        {
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 3, Alias = "", SortOrder = 0, DataTypeNodeId = -87, Value = ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 4, Alias = "group", SortOrder = 0, DataTypeNodeId = 1041, Value = "default" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 5, Alias = "storageType", SortOrder = 0, DataTypeNodeId = 1041, Value = "Json" });
            
            //defaults for the member list
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -1, Alias = "pageSize", SortOrder = 1, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "10" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -2, Alias = "orderBy", SortOrder = 2, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "username" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -3, Alias = "orderDirection", SortOrder = 3, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "asc" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -4, Alias = "includeProperties", SortOrder = 4, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "[{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]" });

            //layouts for the list view
            var cardLayout = "{\"name\": \"Grid\",\"path\": \"views/propertyeditors/listview/layouts/grid/grid.html\", \"icon\": \"icon-thumbnails-small\", \"isSystem\": 1, \"selected\": true}";
            var listLayout = "{\"name\": \"List\",\"path\": \"views/propertyeditors/listview/layouts/list/list.html\",\"icon\": \"icon-list\", \"isSystem\": 1,\"selected\": true}";

            //defaults for the media list
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -5, Alias = "pageSize", SortOrder = 1, DataTypeNodeId = Constants.System.DefaultMediaListViewDataTypeId, Value = "100" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -6, Alias = "orderBy", SortOrder = 2, DataTypeNodeId = Constants.System.DefaultMediaListViewDataTypeId, Value = "updateDate" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -7, Alias = "orderDirection", SortOrder = 3, DataTypeNodeId = Constants.System.DefaultMediaListViewDataTypeId, Value = "desc" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -8, Alias = "layouts", SortOrder = 4, DataTypeNodeId = Constants.System.DefaultMediaListViewDataTypeId, Value = "[" + cardLayout + "," + listLayout + "]" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -9, Alias = "includeProperties", SortOrder = 5, DataTypeNodeId = Constants.System.DefaultMediaListViewDataTypeId, Value = "[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]" });

            //default's for MultipleMediaPickerAlias picker
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 6, Alias = "multiPicker", SortOrder = 0, DataTypeNodeId = 1049, Value = "1" });
        }

        private void CreateUmbracoRelationTypeData()
        {
            var relationType = new RelationTypeDto { Id = 1, Alias = Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias, ChildObjectType = new Guid(Constants.ObjectTypes.Document), ParentObjectType = new Guid(Constants.ObjectTypes.Document), Dual = true, Name = Constants.Conventions.RelationTypes.RelateDocumentOnCopyName };
            relationType.UniqueId = (relationType.Alias + "____" + relationType.Name).ToGuid();
            _database.Insert("umbracoRelationType", "id", false, relationType);
            relationType = new RelationTypeDto { Id = 2, Alias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias, ChildObjectType = new Guid(Constants.ObjectTypes.Document), ParentObjectType = new Guid(Constants.ObjectTypes.Document), Dual = false, Name = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName };
            relationType.UniqueId = (relationType.Alias + "____" + relationType.Name).ToGuid();
            _database.Insert("umbracoRelationType", "id", false, relationType);
            relationType = new RelationTypeDto { Id = 3, Alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias, ChildObjectType = new Guid(Constants.ObjectTypes.Media), ParentObjectType = new Guid(Constants.ObjectTypes.Media), Dual = false, Name = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName };
            relationType.UniqueId = (relationType.Alias + "____" + relationType.Name).ToGuid();
            _database.Insert("umbracoRelationType", "id", false, relationType);
        }

        private void CreateCmsTaskTypeData()
        {
            _database.Insert("cmsTaskType", "id", false, new TaskTypeDto { Id = 1, Alias = "toTranslate" });
        }

        private void CreateUmbracoMigrationData()
        {
            var dto = new MigrationDto
            {
                Id = 1,
                Name = Constants.System.UmbracoMigrationName,
                Version = UmbracoVersion.GetSemanticVersion().ToString(),
                CreateDate = DateTime.Now
            };

            _database.Insert("umbracoMigration", "pk", false, dto);
        }
    }
}
