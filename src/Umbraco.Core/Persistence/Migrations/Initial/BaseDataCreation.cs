using System;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    /// <summary>
    /// Represents the initial data creation by running Insert for the base data.
    /// </summary>
    internal class BaseDataCreation
    {
        private readonly Database _database;

        public BaseDataCreation(Database database)
        {
            _database = database;
        } 

        /// <summary>
        /// Initialize the base data creation by inserting the data foundation for umbraco
        /// specific to a table
        /// </summary>
        /// <param name="tableName">Name of the table to create base data for</param>
        public void InitializeBaseData(string tableName)
        {
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

            if (tableName.Equals("cmsMacroPropertyType"))
            {
                CreateCmsMacroPropertyTypeData();
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
        }

        private void CreateUmbracNodeData()
        {
            using (var transaction = _database.GetTransaction())
            {
                _database.Execute(new Sql("SET IDENTITY_INSERT [umbracoNode] ON "));
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -1, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1", SortOrder = 0, UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"), Text = "SYSTEM DATA: umbraco master root", NodeObjectType = new Guid("ea7d8624-4cfe-4578-a871-24aa946bf34d"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -20, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,-20", SortOrder = 0, UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"), Text = "Recycle Bin", NodeObjectType = new Guid("01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -21, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,-21", SortOrder = 0, UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"), Text = "Recycle Bin", NodeObjectType = new Guid("CF3D8E34-1C1C-41e9-AE56-878B57B32113"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -92, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-92", SortOrder = 35, UniqueId = new Guid("f0bc4bfb-b499-40d6-ba86-058885a5178c"), Text = "Label", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -90, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-90", SortOrder = 34, UniqueId = new Guid("84c6b441-31df-4ffe-b67e-67d5bc3ae65a"), Text = "Upload", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -89, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-89", SortOrder = 33, UniqueId = new Guid("c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3"), Text = "Textbox multiple", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -88, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-88", SortOrder = 32, UniqueId = new Guid("0cc0eba1-9960-42c9-bf9b-60e150b429ae"), Text = "Textstring", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -87, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-87", SortOrder = 4, UniqueId = new Guid("ca90c950-0aff-4e72-b976-a30b1ac57dad"), Text = "Richtext editor", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -51, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-51", SortOrder = 2, UniqueId = new Guid("2e6d3631-066e-44b8-aec4-96f09099b2b5"), Text = "Numeric", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -49, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-49", SortOrder = 2, UniqueId = new Guid("92897bc6-a5f3-4ffe-ae27-f2e7e33dda49"), Text = "True/false", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -43, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-43", SortOrder = 2, UniqueId = new Guid("fbaf13a8-4036-41f2-93a3-974f678c312a"), Text = "Checkbox list", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -42, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-42", SortOrder = 2, UniqueId = new Guid("0b6a45e7-44ba-430d-9da5-4e46060b9e03"), Text = "Dropdown", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -41, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-41", SortOrder = 2, UniqueId = new Guid("5046194e-4237-453c-a547-15db3a07c4e1"), Text = "Date Picker", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -40, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-40", SortOrder = 2, UniqueId = new Guid("bb5f57c9-ce2b-4bb9-b697-4caca783a805"), Text = "Radiobox", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -39, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-39", SortOrder = 2, UniqueId = new Guid("f38f0ac7-1d27-439c-9f3f-089cd8825a53"), Text = "Dropdown multiple", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -38, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-38", SortOrder = 2, UniqueId = new Guid("fd9f1447-6c61-4a7c-9595-5aa39147d318"), Text = "Folder Browser", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -37, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-37", SortOrder = 2, UniqueId = new Guid("0225af17-b302-49cb-9176-b9f35cab9c17"), Text = "Approved Color", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = -36, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-36", SortOrder = 2, UniqueId = new Guid("e4d66c0f-b935-4200-81f0-025f7256b89a"), Text = "Date Picker with time", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1031, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1031", SortOrder = 2, UniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d"), Text = "Folder", NodeObjectType = new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1032, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1032", SortOrder = 2, UniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8"), Text = "Image", NodeObjectType = new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1033, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1033", SortOrder = 2, UniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d"), Text = "File", NodeObjectType = new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1034, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1034", SortOrder = 2, UniqueId = new Guid("a6857c73-d6e9-480c-b6e6-f15f6ad11125"), Text = "Content Picker", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1035, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1035", SortOrder = 2, UniqueId = new Guid("93929b9a-93a2-4e2a-b239-d99334440a59"), Text = "Media Picker", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1036, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1036", SortOrder = 2, UniqueId = new Guid("2b24165f-9782-4aa3-b459-1de4a4d21f60"), Text = "Member Picker", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1038, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1038", SortOrder = 2, UniqueId = new Guid("1251c96c-185c-4e9b-93f4-b48205573cbd"), Text = "Simple Editor", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1039, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1039", SortOrder = 2, UniqueId = new Guid("06f349a9-c949-4b6a-8660-59c10451af42"), Text = "Ultimate Picker", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1040, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1040", SortOrder = 2, UniqueId = new Guid("21e798da-e06e-4eda-a511-ed257f78d4fa"), Text = "Related Links", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1041, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1041", SortOrder = 2, UniqueId = new Guid("b6b73142-b9c1-4bf8-a16d-e1c23320b549"), Text = "Tags", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1042, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1042", SortOrder = 2, UniqueId = new Guid("0a452bd5-83f9-4bc3-8403-1286e13fb77e"), Text = "Macro Container", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 1043, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,1043", SortOrder = 2, UniqueId = new Guid("1df9f033-e6d4-451f-b8d2-e0cbc50a836f"), Text = "Image Cropper", NodeObjectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"), CreateDate = DateTime.UtcNow });
                _database.Execute(new Sql("SET IDENTITY_INSERT [umbracoNode] OFF;"));

                transaction.Complete();
            }
        }

        private void CreateCmsContentTypeData()
        {
            _database.Insert(new ContentTypeDto { PrimaryKey = 532, NodeId = 1031, Alias = "Folder", Icon = "folder.gif", Thumbnail = "folder.png", IsContainer = true, AllowAtRoot = true });
            _database.Insert(new ContentTypeDto { PrimaryKey = 533, NodeId = 1032, Alias = "Image", Icon = "mediaPhoto.gif", Thumbnail = "mediaPhoto.png" });
            _database.Insert(new ContentTypeDto { PrimaryKey = 534, NodeId = 1033, Alias = "File", Icon = "mediaFile.gif", Thumbnail = "mediaFile.png" });
        }

        private void CreateUmbracoUserData()
        {
            using (var transaction = _database.GetTransaction())
            {
                _database.Execute(new Sql("SET IDENTITY_INSERT [umbracoUser] ON "));
                _database.Insert("umbracoUser", "id", false, new UserDto { Id = 0, Disabled = false, NoConsole = false, Type = 1, ContentStartId = -1, MediaStartId = -1, UserName = "Administrator", Login = "admin", Password = "default", Email = "", UserLanguage = "en", DefaultPermissions = null, DefaultToLiveEditing = false });
                _database.Execute(new Sql("SET IDENTITY_INSERT [umbracoUser] OFF;"));

                transaction.Complete();
            }
        }
        
        private void CreateUmbracoUserTypeData()
        {
            _database.Insert(new UserTypeDto { Id = 1, Alias = "admin", Name = "Administrators", DefaultPermissions = "CADMOSKTPIURZ:5F" });
            _database.Insert(new UserTypeDto { Id = 2, Alias = "writer", Name = "Writer", DefaultPermissions = "CAH:F" });
            _database.Insert(new UserTypeDto { Id = 3, Alias = "editor", Name = "Editors", DefaultPermissions = "CADMOSKTPUZ:5F" });
            _database.Insert(new UserTypeDto { Id = 4, Alias = "translator", Name = "Translator", DefaultPermissions = "AF" });
        }

        private void CreateUmbracoUser2AppData()
        {
            _database.Insert(new User2AppDto { UserId = 0, AppAlias = "content" });
            _database.Insert(new User2AppDto { UserId = 0, AppAlias = "developer" });
            _database.Insert(new User2AppDto { UserId = 0, AppAlias = "media" });
            _database.Insert(new User2AppDto { UserId = 0, AppAlias = "member" });
            _database.Insert(new User2AppDto { UserId = 0, AppAlias = "settings" });
            _database.Insert(new User2AppDto { UserId = 0, AppAlias = "users" });
        }

        private void CreateCmsMacroPropertyTypeData()
        {
            _database.Insert(new MacroPropertyTypeDto { Id = 3, Alias = "mediaCurrent", RenderAssembly = "umbraco.macroRenderings", RenderType = "media", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 4, Alias = "contentSubs", RenderAssembly = "umbraco.macroRenderings", RenderType = "content", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 5, Alias = "contentRandom", RenderAssembly = "umbraco.macroRenderings", RenderType = "content", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 6, Alias = "contentPicker", RenderAssembly = "umbraco.macroRenderings", RenderType = "content", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 13, Alias = "number", RenderAssembly = "umbraco.macroRenderings", RenderType = "numeric", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 14, Alias = "bool", RenderAssembly = "umbraco.macroRenderings", RenderType = "yesNo", BaseType = "Boolean" });
            _database.Insert(new MacroPropertyTypeDto { Id = 16, Alias = "text", RenderAssembly = "umbraco.macroRenderings", RenderType = "text", BaseType = "String" });
            _database.Insert(new MacroPropertyTypeDto { Id = 17, Alias = "contentTree", RenderAssembly = "umbraco.macroRenderings", RenderType = "content", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 18, Alias = "contentType", RenderAssembly = "umbraco.macroRenderings", RenderType = "contentTypeSingle", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 19, Alias = "contentTypeMultiple", RenderAssembly = "umbraco.macroRenderings", RenderType = "contentTypeMultiple", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 20, Alias = "contentAll", RenderAssembly = "umbraco.macroRenderings", RenderType = "content", BaseType = "Int32" });
            _database.Insert(new MacroPropertyTypeDto { Id = 21, Alias = "tabPicker", RenderAssembly = "umbraco.macroRenderings", RenderType = "tabPicker", BaseType = "String" });
            _database.Insert(new MacroPropertyTypeDto { Id = 22, Alias = "tabPickerMultiple", RenderAssembly = "umbraco.macroRenderings", RenderType = "tabPickerMultiple", BaseType = "String" });
            _database.Insert(new MacroPropertyTypeDto { Id = 23, Alias = "propertyTypePicker", RenderAssembly = "umbraco.macroRenderings", RenderType = "propertyTypePicker", BaseType = "String" });
            _database.Insert(new MacroPropertyTypeDto { Id = 24, Alias = "propertyTypePickerMultiple", RenderAssembly = "umbraco.macroRenderings", RenderType = "propertyTypePickerMultiple", BaseType = "String" });
            _database.Insert(new MacroPropertyTypeDto { Id = 25, Alias = "textMultiLine", RenderAssembly = "umbraco.macroRenderings", RenderType = "textMultiple", BaseType = "String" });

        }

        private void CreateCmsPropertyTypeGroupData()
        {
            using (var transaction = _database.GetTransaction())
            {
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsPropertyTypeGroup] ON "));
                _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 3, ContentTypeNodeId = 1032, Text = "Image", SortOrder = 1 });
                _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 4, ContentTypeNodeId = 1033, Text = "File", SortOrder = 1 });
                _database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 5, ContentTypeNodeId = 1031, Text = "Contents", SortOrder = 1 });
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsPropertyTypeGroup] OFF;"));

                transaction.Complete();
            }
        }

        private void CreateCmsPropertyTypeData()
        {
            using (var transaction = _database.GetTransaction())
            {
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsPropertyType] ON "));
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 6, DataTypeId = -90, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = "umbracoFile", Name = "Upload image", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 7, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = "umbracoWidth", Name = "Width", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 8, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = "umbracoHeight", Name = "Height", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 9, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = "umbracoBytes", Name = "Size", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 10, DataTypeId = -92, ContentTypeId = 1032, PropertyTypeGroupId = 3, Alias = "umbracoExtension", Name = "Type", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 24, DataTypeId = -90, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = "umbracoFile", Name = "Upload file", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 25, DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = "umbracoExtension", Name = "Type", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 26, DataTypeId = -92, ContentTypeId = 1033, PropertyTypeGroupId = 4, Alias = "umbracoBytes", Name = "Size", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 27, DataTypeId = -38, ContentTypeId = 1031, PropertyTypeGroupId = 5, Alias = "contents", Name = "Contents:", HelpText = null, SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsPropertyType] OFF;"));

                transaction.Complete();
            }
        }

        private void CreateUmbracoLanguageData()
        {
            _database.Insert(new LanguageDto { Id = 1, IsoCode = "en-US", CultureName = "en-US" });
        }

        private void CreateCmsContentTypeAllowedContentTypeData()
        {
            _database.Insert(new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1031 });
            _database.Insert(new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1032 });
            _database.Insert(new ContentTypeAllowedContentTypeDto { Id = 1031, AllowedId = 1033 });
        }

        private void CreateCmsDataTypeData()
        {
            using (var transaction = _database.GetTransaction())
            {
                //TODO Check which of the DataTypeIds below doesn't exist in umbracoNode, which results in a foreign key constraint errors.
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsDataType] ON "));
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 1, DataTypeId = -49, ControlId = new Guid("38b352c1-e9f8-4fd8-9324-9a2eab06d97a"), DbType = "Integer" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 2, DataTypeId = -51, ControlId = new Guid("1413afcb-d19a-4173-8e9a-68288d2a73b8"), DbType = "Integer" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 3, DataTypeId = -87, ControlId = new Guid("5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 4, DataTypeId = -88, ControlId = new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 5, DataTypeId = -89, ControlId = new Guid("67db8357-ef57-493e-91ac-936d305e0f2a"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 6, DataTypeId = -90, ControlId = new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"), DbType = "Nvarchar" });
                //Dropdown list
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 12, DataTypeId = -91, ControlId = new Guid("a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 7, DataTypeId = -92, ControlId = new Guid("6c738306-4c17-4d88-b9bd-6546f3771597"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 8, DataTypeId = -36, ControlId = new Guid("b6fb1622-afa5-4bbf-a3cc-d9672a442222"), DbType = "Date" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 9, DataTypeId = -37, ControlId = new Guid("f8d60f68-ec59-4974-b43b-c46eb5677985"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 10, DataTypeId = -38, ControlId = new Guid("cccd4ae9-f399-4ed2-8038-2e88d19e810c"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 11, DataTypeId = -39, ControlId = new Guid("928639ed-9c73-4028-920c-1e55dbb68783"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 12, DataTypeId = -40, ControlId = new Guid("a52c7c1c-c330-476e-8605-d63d3b84b6a6"), DbType = "Nvarchar" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 13, DataTypeId = -41, ControlId = new Guid("23e93522-3200-44e2-9f29-e61a6fcbb79a"), DbType = "Date" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 14, DataTypeId = -42, ControlId = new Guid("a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6"), DbType = "Integer" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 15, DataTypeId = -43, ControlId = new Guid("b4471851-82b6-4c75-afa4-39fa9c6a75e9"), DbType = "Nvarchar" });
                // Fix for rich text editor backwards compatibility -> 83722133-f80c-4273-bdb6-1befaa04a612 TinyMCE DataType
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 22, DataTypeId = -44, ControlId = new Guid("a3776494-0574-4d93-b7de-efdfdec6f2d1"), DbType = "Ntext" });
                //Radiobutton list
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 23, DataTypeId = -128, ControlId = new Guid("a52c7c1c-c330-476e-8605-d63d3b84b6a6"), DbType = "Nvarchar" });
                //Dropdown list multiple
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 24, DataTypeId = -129, ControlId = new Guid("928639ed-9c73-4028-920c-1e55dbb68783"), DbType = "Nvarchar" });
                //Dropdown list
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 25, DataTypeId = -130, ControlId = new Guid("a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6"), DbType = "Nvarchar" });
                //Dropdown list
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 26, DataTypeId = -131, ControlId = new Guid("a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6"), DbType = "Nvarchar" });
                //Dropdown list
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 27, DataTypeId = -132, ControlId = new Guid("a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6"), DbType = "Nvarchar" });
                //No edit
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 28, DataTypeId = -133, ControlId = new Guid("6c738306-4c17-4d88-b9bd-6546f3771597"), DbType = "Ntext" });
                //Dropdown list multiple
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 29, DataTypeId = -134, ControlId = new Guid("928639ed-9c73-4028-920c-1e55dbb68783"), DbType = "Nvarchar" });
                //Not found - maybe a legacy thing?
                //_database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 30, DataTypeId = -50, ControlId = new Guid("aaf99bb2-dbbe-444d-a296-185076bf0484"), DbType = "Date" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 16, DataTypeId = 1034, ControlId = new Guid("158aa029-24ed-4948-939e-c3da209e5fba"), DbType = "Integer" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 17, DataTypeId = 1035, ControlId = new Guid("ead69342-f06d-4253-83ac-28000225583b"), DbType = "Integer" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 18, DataTypeId = 1036, ControlId = new Guid("39f533e4-0551-4505-a64b-e0425c5ce775"), DbType = "Integer" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 19, DataTypeId = 1038, ControlId = new Guid("60b7dabf-99cd-41eb-b8e9-4d2e669bbde9"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 20, DataTypeId = 1039, ControlId = new Guid("cdbf0b5d-5cb2-445f-bc12-fcaaec07cf2c"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 21, DataTypeId = 1040, ControlId = new Guid("71b8ad1a-8dc2-425c-b6b8-faa158075e63"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 22, DataTypeId = 1041, ControlId = new Guid("4023e540-92f5-11dd-ad8b-0800200c9a66"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 23, DataTypeId = 1042, ControlId = new Guid("474FCFF8-9D2D-11DE-ABC6-AD7A56D89593"), DbType = "Ntext" });
                _database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 24, DataTypeId = 1043, ControlId = new Guid("7A2D436C-34C2-410F-898F-4A23B3D79F54"), DbType = "Ntext" });
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsDataType] OFF;"));

                transaction.Complete();
            }
        }

        private void CreateCmsDataTypePreValuesData()
        {
            using (var transaction = _database.GetTransaction())
            {
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsDataTypePreValues] ON "));
                _database.Insert("cmsDataTypePreValues", "id", false,
                                 new DataTypePreValueDto
                                 {
                                     Id = 3,
                                     Alias = "",
                                     SortOrder = 0,
                                     DataTypeNodeId = -87,
                                     Value = ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,' + char(124) + '1' + char(124) + '1,2,3,' + char(124) + '0' + char(124) + '500,400' + char(124) + '1049,' + char(124) + 'true' + char(124) + '"
                                 });
                _database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto
                                 {
                                     Id = 4,
                                     Alias = "group",
                                     SortOrder = 0,
                                     DataTypeNodeId = 1041,
                                     Value = "default"
                                 });
                _database.Execute(new Sql("SET IDENTITY_INSERT [cmsDataTypePreValues] OFF;"));

                transaction.Complete();
            }
        }

        private void CreateUmbracoRelationTypeData()
        {
            using (var transaction = _database.GetTransaction())
            {
                _database.Execute(new Sql("SET IDENTITY_INSERT [umbracoRelationType] ON "));
                _database.Insert("umbracoRelationType", "id", false,
                                 new RelationTypeDto
                                     {
                                         Id = 1,
                                         Alias = "relateDocumentOnCopy",
                                         ChildObjectType = new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"),
                                         ParentObjectType = new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"),
                                         Dual = true,
                                         Name = "Relate Document On Copy"
                                     });
                _database.Execute(new Sql("SET IDENTITY_INSERT [umbracoRelationType] OFF;"));

                transaction.Complete();
            }
        }

        private void CreateCmsTaskTypeData()
        {
            //TODO: It tries to do an identity insert it seems, why? Error: The column cannot be modified. [ Column name = id ]
            //using (var transaction = _database.GetTransaction())
            //{
            //    _database.Insert("cmsTaskType", "id", false,
            //                     new TaskTypeDto
            //                     {
            //                         Alias = "toTranslate"
            //                     });

            //    transaction.Complete();
            //}            
        }
    }
}