using System;
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

            if(tableName.Equals("cmsContentType"))
            {
                CreateCmsContentTypeData();
            }

            if (tableName.Equals("umbracoUser"))
            {
                CreateUmbracoUserData();
            }

            if (tableName.Equals("umbracoUserType"))
            {
                CreateUmbracoUserTypeData();
            }

            if (tableName.Equals("umbracoUser2app"))
            {
                CreateUmbracoUser2AppData();
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

            _logger.Info<BaseDataCreation>(string.Format("Done creating data in table {0}", tableName));
        }

        private void CreateUmbracNodeData()
        {
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -1, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1", SortOrder = 0, UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"), Text = "SYSTEM DATA: umbraco master root", NodeObjectType = new Guid(Constants.ObjectTypes.SystemRoot), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -20, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,-20", SortOrder = 0, UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"), Text = "Recycle Bin", NodeObjectType = new Guid(Constants.ObjectTypes.ContentRecycleBin), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -21, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,-21", SortOrder = 0, UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"), Text = "Recycle Bin", NodeObjectType = new Guid(Constants.ObjectTypes.MediaRecycleBin), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -92, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-92", SortOrder = 35, UniqueId = new Guid("f0bc4bfb-b499-40d6-ba86-058885a5178c"), Text = "Label", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -90, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-90", SortOrder = 34, UniqueId = new Guid("84c6b441-31df-4ffe-b67e-67d5bc3ae65a"), Text = "Upload", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -89, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-89", SortOrder = 33, UniqueId = new Guid("c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3"), Text = "Textbox multiple", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -88, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-88", SortOrder = 32, UniqueId = new Guid("0cc0eba1-9960-42c9-bf9b-60e150b429ae"), Text = "Textstring", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -87, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-87", SortOrder = 4, UniqueId = new Guid("ca90c950-0aff-4e72-b976-a30b1ac57dad"), Text = "Richtext editor", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -51, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-51", SortOrder = 2, UniqueId = new Guid("2e6d3631-066e-44b8-aec4-96f09099b2b5"), Text = "Numeric", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -49, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-49", SortOrder = 2, UniqueId = new Guid("92897bc6-a5f3-4ffe-ae27-f2e7e33dda49"), Text = "True/false", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -43, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-43", SortOrder = 2, UniqueId = new Guid("fbaf13a8-4036-41f2-93a3-974f678c312a"), Text = "Checkbox list", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -42, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-42", SortOrder = 2, UniqueId = new Guid("0b6a45e7-44ba-430d-9da5-4e46060b9e03"), Text = "Dropdown", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -41, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-41", SortOrder = 2, UniqueId = new Guid("5046194e-4237-453c-a547-15db3a07c4e1"), Text = "Date Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -40, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-40", SortOrder = 2, UniqueId = new Guid("bb5f57c9-ce2b-4bb9-b697-4caca783a805"), Text = "Radiobox", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -39, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-39", SortOrder = 2, UniqueId = new Guid("f38f0ac7-1d27-439c-9f3f-089cd8825a53"), Text = "Dropdown multiple", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -38, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-38", SortOrder = 2, UniqueId = new Guid("fd9f1447-6c61-4a7c-9595-5aa39147d318"), Text = "Folder Browser", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -37, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-37", SortOrder = 2, UniqueId = new Guid("0225af17-b302-49cb-9176-b9f35cab9c17"), Text = "Approved Color", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -36, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-36", SortOrder = 2, UniqueId = new Guid("e4d66c0f-b935-4200-81f0-025f7256b89a"), Text = "Date Picker with time", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.System.DefaultContentListViewDataTypeId, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-95", SortOrder = 2, UniqueId = new Guid("C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Content", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.System.DefaultMediaListViewDataTypeId, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-96", SortOrder = 2, UniqueId = new Guid("3A0156C4-3B8C-4803-BDC1-6871FAA83FFF"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Media", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.System.DefaultMembersListViewDataTypeId, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-97", SortOrder = 2, UniqueId = new Guid("AA2C52A0-CE87-4E65-A47C-7DF09358585D"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Members", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });                                    
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1031, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1031", SortOrder = 2, UniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d"), Text = Constants.Conventions.MediaTypes.Folder, NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1032, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1032", SortOrder = 2, UniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8"), Text = Constants.Conventions.MediaTypes.Image, NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1033, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1033", SortOrder = 2, UniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d"), Text = Constants.Conventions.MediaTypes.File, NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1034, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1034", SortOrder = 2, UniqueId = new Guid("a6857c73-d6e9-480c-b6e6-f15f6ad11125"), Text = "Content Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1035, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1035", SortOrder = 2, UniqueId = new Guid("93929b9a-93a2-4e2a-b239-d99334440a59"), Text = "Media Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1036, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1036", SortOrder = 2, UniqueId = new Guid("2b24165f-9782-4aa3-b459-1de4a4d21f60"), Text = "Member Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });            
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1040, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1040", SortOrder = 2, UniqueId = new Guid("21e798da-e06e-4eda-a511-ed257f78d4fa"), Text = "Related Links", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1041, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1041", SortOrder = 2, UniqueId = new Guid("b6b73142-b9c1-4bf8-a16d-e1c23320b549"), Text = "Tags", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1043, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1043", SortOrder = 2, UniqueId = new Guid("1df9f033-e6d4-451f-b8d2-e0cbc50a836f"), Text = "Image Cropper", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1044, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1044", SortOrder = 0, UniqueId = new Guid("d59be02f-1df9-4228-aa1e-01917d806cda"), Text = Constants.Conventions.MemberTypes.DefaultAlias, NodeObjectType = new Guid(Constants.ObjectTypes.MemberType), CreateDate = DateTime.Now });
            _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1045, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1045", SortOrder = 2, UniqueId = new Guid("7E3962CC-CE20-4FFC-B661-5897A894BA7E"), Text = "Multiple Media Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            

            //TODO: We're not creating these for 7.0
            //_database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1039, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1039", SortOrder = 2, UniqueId = new Guid("06f349a9-c949-4b6a-8660-59c10451af42"), Text = "Ultimate Picker", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            //_database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1038, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1038", SortOrder = 2, UniqueId = new Guid("1251c96c-185c-4e9b-93f4-b48205573cbd"), Text = "Simple Editor", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
            
            //_database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1042, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1042", SortOrder = 2, UniqueId = new Guid("0a452bd5-83f9-4bc3-8403-1286e13fb77e"), Text = "Macro Container", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });            
        }

        private void CreateCmsContentTypeData()
        {
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 532, NodeId = 1031, Alias = Constants.Conventions.MediaTypes.Folder, Icon = "icon-folder", Thumbnail = "icon-folder", IsContainer = false, AllowAtRoot = true });
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 533, NodeId = 1032, Alias = Constants.Conventions.MediaTypes.Image, Icon = "icon-picture", Thumbnail = "icon-picture" });
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 534, NodeId = 1033, Alias = Constants.Conventions.MediaTypes.File, Icon = "icon-document", Thumbnail = "icon-document" });
            _database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 531, NodeId = 1044, Alias = Constants.Conventions.MemberTypes.DefaultAlias, Icon = "icon-user", Thumbnail = "icon-user" });
        }

        private void CreateUmbracoUserData()
        {
            _database.Insert("umbracoUser", "id", false, new UserDto { Id = 0, Disabled = false, NoConsole = false, Type = 1, ContentStartId = -1, MediaStartId = -1, UserName = "Administrator", Login = "admin", Password = "default", Email = "", UserLanguage = "en" });
            //_database.Update<UserDto>("SET id = @IdAfter WHERE id = @IdBefore AND userLogin = @Login", new { IdAfter = 0, IdBefore = 1, Login = "admin" });
        }
        
        private void CreateUmbracoUserTypeData()
        {
            _database.Insert("umbracoUserType", "id", false, new UserTypeDto { Id = 1, Alias = "admin", Name = "Administrators", DefaultPermissions = "CADMOSKTPIURZ:5F7" });
            _database.Insert("umbracoUserType", "id", false, new UserTypeDto { Id = 2, Alias = "writer", Name = "Writer", DefaultPermissions = "CAH:F" });
            _database.Insert("umbracoUserType", "id", false, new UserTypeDto { Id = 3, Alias = "editor", Name = "Editors", DefaultPermissions = "CADMOSKTPUZ:5F" });
            _database.Insert("umbracoUserType", "id", false, new UserTypeDto { Id = 4, Alias = "translator", Name = "Translator", DefaultPermissions = "AF" });
        }

        private void CreateUmbracoUser2AppData()
        {
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Content });
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Developer });
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Media });
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Members });
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Settings });
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Users });
            _database.Insert("umbracoUser2app", "user", false, new User2AppDto { UserId = 0, AppAlias = Constants.Applications.Forms });
        }
        
        private void CreateCmsPropertyTypeGroupData()
        {          
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 3, ContentTypeNodeId = 1032, Text = "Image", SortOrder = 1 });
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 4, ContentTypeNodeId = 1033, Text = "File", SortOrder = 1 });
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 5, ContentTypeNodeId = 1031, Text = "Contents", SortOrder = 1 });
            //membership property group
            _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 11, ContentTypeNodeId = 1044, Text = "Membership", SortOrder = 1 });
        }

        private void CreateCmsPropertyTypeData()
        {
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 6, DataTypeId = -90, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.File, Name = "Upload image", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 7, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Width, Name = "Width", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 8, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Height, Name = "Height", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 9, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 10, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 24, DataTypeId = -90, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.File, Name = "Upload file", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 25, DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.Extension, Name = "Type", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 26, DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = Constants.Conventions.Media.Bytes, Name = "Size", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 27, DataTypeId = -38, ContentTypeId = 1031, PropertyTypeGroupId = 5, Alias = "contents", Name = "Contents:", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            //membership property types
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 28, DataTypeId = -89, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.Comments, Name = Constants.Conventions.Member.CommentsLabel, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 29, DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.FailedPasswordAttempts, Name = Constants.Conventions.Member.FailedPasswordAttemptsLabel, SortOrder = 1, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 30, DataTypeId = -49, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.IsApproved, Name = Constants.Conventions.Member.IsApprovedLabel, SortOrder = 2, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 31, DataTypeId = -49, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.IsLockedOut, Name = Constants.Conventions.Member.IsLockedOutLabel, SortOrder = 3, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 32, DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastLockoutDate, Name = Constants.Conventions.Member.LastLockoutDateLabel, SortOrder = 4, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 33, DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastLoginDate, Name = Constants.Conventions.Member.LastLoginDateLabel, SortOrder = 5, Mandatory = false, ValidationRegExp = null, Description = null });
            _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 34, DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.LastPasswordChangeDate, Name = Constants.Conventions.Member.LastPasswordChangeDateLabel, SortOrder = 6, Mandatory = false, ValidationRegExp = null, Description = null });
            
            //TODO: The member editor doesn't currently support providers that have question/answer so we'll leave these out for now.
            // Also, it's worth noting that the built in ASP.Net providers encrypt the answer so that admins cannot see it for added security which is something we should actually do!
            //_database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 35, DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.PasswordAnswer, Name = Constants.Conventions.Member.PasswordAnswerLabel, HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
            //_database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 36, DataTypeId = -92, ContentTypeId = 1044, PropertyTypeGroupId = 11, Alias = Constants.Conventions.Member.PasswordQuestion, Name = Constants.Conventions.Member.PasswordQuestionLabel, HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
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
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 10, DataTypeId = -38, PropertyEditorAlias = Constants.PropertyEditors.FolderBrowserAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 11, DataTypeId = -39, PropertyEditorAlias = Constants.PropertyEditors.DropDownListMultipleAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 12, DataTypeId = -40, PropertyEditorAlias = Constants.PropertyEditors.RadioButtonListAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 13, DataTypeId = -41, PropertyEditorAlias = Constants.PropertyEditors.DateAlias, DbType = "Date" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 14, DataTypeId = -42, PropertyEditorAlias = Constants.PropertyEditors.DropDownListAlias, DbType = "Integer" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 15, DataTypeId = -43, PropertyEditorAlias = Constants.PropertyEditors.CheckBoxListAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 16, DataTypeId = 1034, PropertyEditorAlias = Constants.PropertyEditors.ContentPickerAlias, DbType = "Integer" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 17, DataTypeId = 1035, PropertyEditorAlias = Constants.PropertyEditors.MediaPickerAlias, DbType = "Integer" });            
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 18, DataTypeId = 1036, PropertyEditorAlias = Constants.PropertyEditors.MemberPickerAlias, DbType = "Integer" });                        
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 21, DataTypeId = 1040, PropertyEditorAlias = Constants.PropertyEditors.RelatedLinksAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 22, DataTypeId = 1041, PropertyEditorAlias = Constants.PropertyEditors.TagsAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 24, DataTypeId = 1043, PropertyEditorAlias = Constants.PropertyEditors.ImageCropperAlias, DbType = "Ntext" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 25, DataTypeId = 1045, PropertyEditorAlias = Constants.PropertyEditors.MultipleMediaPickerAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = -26, DataTypeId = Constants.System.DefaultContentListViewDataTypeId, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = -27, DataTypeId = Constants.System.DefaultMediaListViewDataTypeId, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
            _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = -28, DataTypeId = Constants.System.DefaultMembersListViewDataTypeId, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });            

            //TODO: We're not creating these for 7.0
            //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 19, DataTypeId = 1038, PropertyEditorAlias = Constants.PropertyEditors.MarkdownEditorAlias, DbType = "Ntext" });            
            //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 20, DataTypeId = 1039, PropertyEditorAlias = Constants.PropertyEditors.UltimatePickerAlias, DbType = "Ntext" });
            //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 23, DataTypeId = 1042, PropertyEditorAlias = Constants.PropertyEditors.MacroContainerAlias, DbType = "Ntext" });
            
        }

        private void CreateCmsDataTypePreValuesData()
        {
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 3, Alias = "", SortOrder = 0, DataTypeNodeId = -87, Value = ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 4, Alias = "group", SortOrder = 0, DataTypeNodeId = 1041, Value = "default" });

            //default's for MultipleMediaPickerAlias picker
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = 5, Alias = "multiPicker", SortOrder = 0, DataTypeNodeId = 1045, Value = "1" });

            //defaults for the member list
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -1, Alias = "pageSize", SortOrder = 1, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "10" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -2, Alias = "orderBy", SortOrder = 2, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "Name" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -3, Alias = "orderDirection", SortOrder = 3, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "asc" });
            _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -4, Alias = "includeProperties", SortOrder = 4, DataTypeNodeId = Constants.System.DefaultMembersListViewDataTypeId, Value = "[{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]" });
        }

        private void CreateUmbracoRelationTypeData()
        {
            _database.Insert("umbracoRelationType", "id", false, new RelationTypeDto { Id = 1, Alias = Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias, ChildObjectType = new Guid(Constants.ObjectTypes.Document), ParentObjectType = new Guid(Constants.ObjectTypes.Document), Dual = true, Name = Constants.Conventions.RelationTypes.RelateDocumentOnCopyName });
            _database.Insert("umbracoRelationType", "id", false, new RelationTypeDto { Id = 2, Alias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias, ChildObjectType = new Guid(Constants.ObjectTypes.Document), ParentObjectType = new Guid(Constants.ObjectTypes.Document), Dual = false, Name = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName });
        }

        private void CreateCmsTaskTypeData()
        {
            _database.Insert("cmsTaskType", "id", false, new TaskTypeDto { Id = 1, Alias = "toTranslate" });
        }
    }
}