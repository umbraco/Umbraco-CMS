using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFCoreDatabaseDataCreator : IDatabaseDataCreator
{
    private readonly UmbracoDbContextFactory _dbContextFactory;

    private readonly IDictionary<string, IList<string>> _entitiesToAlwaysCreate = new Dictionary<string, IList<string>>
    {
        {
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            new List<string> { Constants.DataTypes.Guids.LabelString }
        }
    };

    private readonly IOptionsMonitor<InstallDefaultDataSettings> _installDefaultDataSettings;

    public EFCoreDatabaseDataCreator(UmbracoDbContextFactory dbContextFactory,
        IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
    {
        _dbContextFactory = dbContextFactory;
        _installDefaultDataSettings = installDefaultDataSettings;
    }

    public async Task SeedDataAsync() =>
        await _dbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {

            try
            {
                using (IDbContextTransaction transaction = await db.Database.BeginTransactionAsync())
                {
                    await CreateLockData(db);
                    UmbracoUser adminUser = await CreateUserData(db);
                    await CreateLanguageData(db);
                    await CreateNodeData(db);
                    await CreateContentTypeData(db);
                    await CreateContentTypeChildTypeData(db);
                    await CreateUserGroupData(db, adminUser);
                    await CreateDataTypeData(db);
                    await CreatePropertyTypeGroupData(db);
                    await CreatePropertyTypeData(db);

                    await CreateRelationTypeData(db);
                    await CreateLogViewerQueryData(db);

                    await transaction.CommitAsync();
                }
            }
            catch (Exception e)
            {
                throw;
            }

        });

    private async Task CreateContentTypeChildTypeData(UmbracoEFContext db)
    {

        var contentTypes = (await db.CmsContentTypes.ToListAsync()).ToDictionary(x=>x.NodeId);

        if (!contentTypes.ContainsKey(1031))
        {
            return;
        }

        for (var i = 1031; i <= 1037; i++)
        {
            if (contentTypes.ContainsKey(i))
            {
                db.Add(new CmsContentTypeAllowedContentType() { Id = 1031, AllowedId = i });
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task CreateDataTypeData(UmbracoEFContext db)
    {
        void InsertDataTypeDto(int id, string editorAlias, string dbType, HashSet<int> existingnodeids, string? configuration = null)
        {
            var dataTypeDto = new UmbracoDataType() { NodeId = id, PropertyEditorAlias = editorAlias, DbType = dbType };

            if (configuration != null)
            {
                dataTypeDto.Config = configuration;
            }

            if (existingnodeids.Contains(id))
            {
                db.Add(dataTypeDto);
            }
        }

        var existingNodeIds = new HashSet<int>(await db.UmbracoNodes.Select(x => x.Id).ToArrayAsync());

        // layouts for the list view
        const string cardLayout =
            "{\"name\": \"Grid\",\"path\": \"views/propertyeditors/listview/layouts/grid/grid.html\", \"icon\": \"icon-thumbnails-small\", \"isSystem\": 1, \"selected\": true}";
        const string listLayout =
            "{\"name\": \"List\",\"path\": \"views/propertyeditors/listview/layouts/list/list.html\",\"icon\": \"icon-list\", \"isSystem\": 1,\"selected\": true}";
        const string layouts = "[" + cardLayout + "," + listLayout + "]";

        await db.EnableIdentityInsert<UmbracoPropertyDatum>();

        // Insert data types only if the corresponding Node record exists (which may or may not have been created depending on configuration
        // of data types to create).
        if (existingNodeIds.Contains(Constants.DataTypes.Boolean))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.Boolean,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Boolean,
                    DbType = "Integer",
                });
        }

        if (existingNodeIds.Contains(-51))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = -51,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Integer,
                    DbType = "Integer",
                });
        }

        if (existingNodeIds.Contains(-87))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = -87,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.TinyMce,
                    DbType = "Ntext",
                    Config =
                        "{\"value\":\",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|\"}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.Textbox))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.Textbox,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.TextBox,
                    DbType = "Nvarchar",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.Textarea))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.Textarea,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.TextArea,
                    DbType = "Ntext",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.Upload))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.Upload,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                });
        }

        InsertDataTypeDto(Constants.DataTypes.LabelString, Constants.PropertyEditors.Aliases.Label, "Nvarchar", existingNodeIds,
            "{\"umbracoDataValueType\":\"STRING\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelInt, Constants.PropertyEditors.Aliases.Label, "Integer", existingNodeIds,
            "{\"umbracoDataValueType\":\"INT\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelBigint, Constants.PropertyEditors.Aliases.Label, "Nvarchar", existingNodeIds,
            "{\"umbracoDataValueType\":\"BIGINT\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelDateTime, Constants.PropertyEditors.Aliases.Label, "Date", existingNodeIds,
            "{\"umbracoDataValueType\":\"DATETIME\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelDecimal, Constants.PropertyEditors.Aliases.Label, "Decimal", existingNodeIds,
            "{\"umbracoDataValueType\":\"DECIMAL\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelTime, Constants.PropertyEditors.Aliases.Label, "Date", existingNodeIds,
            "{\"umbracoDataValueType\":\"TIME\"}");

        if (existingNodeIds.Contains(Constants.DataTypes.DateTime))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.DateTime,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.DateTime,
                    DbType = "Date",
                });
        }

        if (existingNodeIds.Contains(-37))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = -37,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ColorPicker,
                    DbType = "Nvarchar",
                });
        }

        InsertDataTypeDto(Constants.DataTypes.DropDownSingle, Constants.PropertyEditors.Aliases.DropDownListFlexible,
            "Nvarchar", existingNodeIds, "{\"multiple\":false}");

        if (existingNodeIds.Contains(-40))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = -40,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.RadioButtonList,
                    DbType = "Nvarchar",
                });
        }

        if (existingNodeIds.Contains(-41))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = -41,
                    PropertyEditorAlias = "Umbraco.DateTime",
                    DbType = "Date",
                    Config = "{\"format\":\"YYYY-MM-DD\"}",
                });
        }

        InsertDataTypeDto(Constants.DataTypes.DropDownMultiple, Constants.PropertyEditors.Aliases.DropDownListFlexible,
            "Nvarchar", existingNodeIds, "{\"multiple\":true}");

        if (existingNodeIds.Contains(-43))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = -43,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.CheckBoxList,
                    DbType = "Nvarchar",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.Tags))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.Tags,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.Tags,
                    DbType = "Ntext",
                    Config = "{\"group\":\"default\", \"storageType\":\"Json\"}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.ImageCropper))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.ImageCropper,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ImageCropper,
                    DbType = "Ntext",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.DefaultContentListView))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.DefaultContentListView,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ListView,
                    DbType = "Nvarchar",
                    Config =
                        "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" +
                        layouts +
                        ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.DefaultMediaListView))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.DefaultMediaListView,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ListView,
                    DbType = "Nvarchar",
                    Config =
                        "{\"pageSize\":100, \"orderBy\":\"updateDate\", \"orderDirection\":\"desc\", \"layouts\":" +
                        layouts +
                        ", \"includeProperties\":[{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1},{\"alias\":\"owner\",\"header\":\"Updated by\",\"isSystem\":1}]}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.DefaultMembersListView))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.DefaultMembersListView,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ListView,
                    DbType = "Nvarchar",
                    Config =
                        "{\"pageSize\":10, \"orderBy\":\"username\", \"orderDirection\":\"asc\", \"includeProperties\":[{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]}",
                });
        }

        // New UDI pickers with newer Ids
        if (existingNodeIds.Contains(1046))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1046,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.ContentPicker,
                    DbType = "Nvarchar",
                });
        }

        if (existingNodeIds.Contains(1047))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1047,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MemberPicker,
                    DbType = "Nvarchar",
                });
        }

        if (existingNodeIds.Contains(1048))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1048,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MediaPicker,
                    DbType = "Ntext",
                });
        }

        if (existingNodeIds.Contains(1049))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1049,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MediaPicker,
                    DbType = "Ntext",
                    Config = "{\"multiPicker\":1}",
                });
        }

        if (existingNodeIds.Contains(1050))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1050,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MultiUrlPicker,
                    DbType = "Ntext",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.UploadVideo))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.UploadVideo,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Config =
                        "{\"fileExtensions\":[{\"id\":0, \"value\":\"mp4\"}, {\"id\":1, \"value\":\"webm\"}, {\"id\":2, \"value\":\"ogv\"}]}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.UploadAudio))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.UploadAudio,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Config =
                        "{\"fileExtensions\":[{\"id\":0, \"value\":\"mp3\"}, {\"id\":1, \"value\":\"weba\"}, {\"id\":2, \"value\":\"oga\"}, {\"id\":3, \"value\":\"opus\"}]}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.UploadArticle))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.UploadArticle,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Config =
                        "{\"fileExtensions\":[{\"id\":0, \"value\":\"pdf\"}, {\"id\":1, \"value\":\"docx\"}, {\"id\":2, \"value\":\"doc\"}]}",
                });
        }

        if (existingNodeIds.Contains(Constants.DataTypes.UploadVectorGraphics))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = Constants.DataTypes.UploadVectorGraphics,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.UploadField,
                    DbType = "Nvarchar",
                    Config = "{\"fileExtensions\":[{\"id\":0, \"value\":\"svg\"}]}",
                });
        }

        if (existingNodeIds.Contains(1051))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1051,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Config = "{\"multiple\": false, \"validationLimit\":{\"min\":0,\"max\":1}}",
                });
        }

        if (existingNodeIds.Contains(1052))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1052,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Config = "{\"multiple\": true}",
                });
        }

        if (existingNodeIds.Contains(1053))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1053,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Config = "{\"filter\":\"" + Constants.Conventions.MediaTypes.Image +
                             "\", \"multiple\": false, \"validationLimit\":{\"min\":0,\"max\":1}}",
                });
        }

        if (existingNodeIds.Contains(1054))
        {
            db.Add(
                new UmbracoDataType()
                {
                    NodeId = 1054,
                    PropertyEditorAlias = Constants.PropertyEditors.Aliases.MediaPicker3,
                    DbType = "Ntext",
                    Config = "{\"filter\":\"" + Constants.Conventions.MediaTypes.Image +
                                    "\", \"multiple\": true}",
                });
        }

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<UmbracoPropertyDatum>();
    }

    private async Task CreateLogViewerQueryData(UmbracoEFContext db)
    {
        db.AddRange(new UmbracoLogViewerQuery[]
        {
            new()
            {
                Name = "Find all logs where the Level is NOT Verbose and NOT Debug",
                Query = "Not(@Level='Verbose') and Not(@Level='Debug')",
            },
            new()
            {
                Name = "Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)",
                Query = "Has(@Exception)",
            },
            new() { Name = "Find all logs that have the property 'Duration'", Query = "Has(Duration)" },
            new()
            {
                Name = "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
                Query = "Has(Duration) and Duration > 1000",
            },
            new()
            {
                Name = "Find all logs that are from the namespace 'Umbraco.Core'",
                Query = "StartsWith(SourceContext, 'Umbraco.Core')",
            },
            new()
            {
                Name = "Find all logs that use a specific log message template",
                Query = "@MessageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'",
            },
            new()
            {
                Name = "Find logs where one of the items in the SortedComponentTypes property array is equal to",
                Query = "SortedComponentTypes[?] = 'Umbraco.Web.Search.ExamineComponent'",
            },
            new()
            {
                Name = "Find logs where one of the items in the SortedComponentTypes property array contains",
                Query = "Contains(SortedComponentTypes[?], 'DatabaseServer')",
            },
            new()
            {
                Name = "Find all logs that the message has localhost in it with SQL like",
                Query = "@Message like '%localhost%'",
            },
            new()
            {
                Name = "Find all logs that the message that starts with 'end' in it with SQL like",
                Query = "@Message like 'end%'"
            },
        });

        await db.SaveChangesAsync();
    }


    internal static Guid CreateUniqueRelationTypeId(string alias, string name) => (alias + "____" + name).ToGuid();


    private async Task CreateRelationTypeData(UmbracoEFContext db)
    {
        void CreateRelationTypeData(int id, string alias, string name, Guid? parentObjectType,
            Guid? childObjectType, bool dual, bool isDependency)
        {
            var relationType = new UmbracoRelationType()
            {
                Id = id,
                Alias = alias,
                ChildObjectType = childObjectType,
                ParentObjectType = parentObjectType,
                Dual = dual,
                Name = name,
                IsDependency = isDependency,
            };
            relationType.TypeUniqueId = CreateUniqueRelationTypeId(relationType.Alias, relationType.Name);

            db.Add(relationType);
        }

        await db.EnableIdentityInsert<UmbracoRelationType>();

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

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<UmbracoRelationType>();
    }


    private async Task CreatePropertyTypeData(UmbracoEFContext db)
    {
        var existingGroupIds = new HashSet<int>(await db.CmsPropertyTypeGroups.Select(x => x.Id).ToArrayAsync());

        await db.EnableIdentityInsert<CmsPropertyType>();

          // Media property types.
        if (existingGroupIds.Contains(3))
        {
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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

        if (existingGroupIds.Contains(4))
        {
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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

        if (existingGroupIds.Contains(52))
        {
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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

        if (existingGroupIds.Contains(53))
        {
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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

        if (existingGroupIds.Contains(54))
        {
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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

        if (existingGroupIds.Contains(55))
        {
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
            db.Add(
                new CmsPropertyType()
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
        if (existingGroupIds.Contains(11))
        {
            db.Add(
                new CmsPropertyType()
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

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<CmsPropertyType>();
    }

    private async Task CreatePropertyTypeGroupData(UmbracoEFContext db)
    {
        var existingNodeIds = new HashSet<int>(await db.UmbracoNodes.Select(x => x.Id).ToArrayAsync());

        await db.EnableIdentityInsert<CmsPropertyTypeGroup>();

        if (existingNodeIds.Contains(1032))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 3,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Image),
                    ContenttypeNodeId = 1032,
                    Text = "Image",
                    Alias = "image",
                    Sortorder = 1,
                });
        }

        if (existingNodeIds.Contains(1033))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 4,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.File),
                    ContenttypeNodeId = 1033,
                    Text = "File",
                    Alias = "file",
                    Sortorder = 1,
                });
        }

        if (existingNodeIds.Contains(1034))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 52,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Video),
                    ContenttypeNodeId = 1034,
                    Text = "Video",
                    Alias = "video",
                    Sortorder = 1,
                });
        }

        if (existingNodeIds.Contains(1035))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 53,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Audio),
                    ContenttypeNodeId = 1035,
                    Text = "Audio",
                    Alias = "audio",
                    Sortorder = 1,
                });
        }

        if (existingNodeIds.Contains(1036))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 54,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Article),
                    ContenttypeNodeId = 1036,
                    Text = "Article",
                    Alias = "article",
                    Sortorder = 1,
                });
        }

        if (existingNodeIds.Contains(1037))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 55,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.VectorGraphics),
                    ContenttypeNodeId = 1037,
                    Text = "Vector Graphics",
                    Alias = "vectorGraphics",
                    Sortorder = 1,
                });
        }

        // Membership property group.
        if (existingNodeIds.Contains(1044))
        {
            db.Add(
                new CmsPropertyTypeGroup()
                {
                    Id = 11,
                    UniqueId = new Guid(Constants.PropertyTypeGroups.Membership),
                    ContenttypeNodeId = 1044,
                    Text = Constants.Conventions.Member.StandardPropertiesGroupName,
                    Alias = Constants.Conventions.Member.StandardPropertiesGroupAlias,
                    Sortorder = 1,
                });
        }

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<CmsPropertyTypeGroup>();
    }


    private async Task CreateUserGroupData(UmbracoEFContext db, UmbracoUser adminUser)
    {
        await db.EnableIdentityInsert<UmbracoUserGroup>();

        db.Add(
            new UmbracoUserGroup
            {
                Id = 1,
                StartMediaId = -1,
                StartContentId = -1,
                UserGroupAlias = Constants.Security.AdminGroupAlias,
                UserGroupName = "Administrators",
                UserGroupDefaultPermissions = "CADMOSKTPIURZ:5F7ïN",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-medal",
                HasAccessToAllLanguages = true,
                Users = new List<UmbracoUser> { adminUser },
                UmbracoUserGroup2Apps = new List<UmbracoUserGroup2App>
                {
                    new() { App = Constants.Applications.Content },
                    new() { App = Constants.Applications.Packages },
                    new() { App = Constants.Applications.Media },
                    new() { App = Constants.Applications.Members },
                    new() { App = Constants.Applications.Settings },
                    new() { App = Constants.Applications.Users },
                    new() { App = Constants.Applications.Forms },
                    new() { App = Constants.Applications.Translation }
                },
            });
        db.Add(
            new UmbracoUserGroup
            {
                Id = 2,
                StartMediaId = -1,
                StartContentId = -1,
                UserGroupAlias = Constants.Security.WriterGroupAlias,
                UserGroupName = "Writers",
                UserGroupDefaultPermissions = "CAH:FN",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-edit",
                HasAccessToAllLanguages = true,
                UmbracoUserGroup2Apps = new List<UmbracoUserGroup2App>
                {
                    new() { App = Constants.Applications.Content },
                },
            });
        db.Add(
            new UmbracoUserGroup
            {
                Id = 3,
                StartMediaId = -1,
                StartContentId = -1,
                UserGroupAlias = Constants.Security.EditorGroupAlias,
                UserGroupName = "Editors",
                UserGroupDefaultPermissions = "CADMOSKTPUZ:5FïN",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-tools",
                HasAccessToAllLanguages = true,
                UmbracoUserGroup2Apps = new List<UmbracoUserGroup2App>
                {
                    new() { App = Constants.Applications.Content },
                    new() { App = Constants.Applications.Media },
                    new() { App = Constants.Applications.Forms },
                }
            });
        db.Add(
            new UmbracoUserGroup
            {
                Id = 4,
                StartMediaId = -1,
                StartContentId = -1,
                UserGroupAlias = Constants.Security.TranslatorGroupAlias,
                UserGroupName = "Translators",
                UserGroupDefaultPermissions = "AF",
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-globe",
                HasAccessToAllLanguages = true,
                UmbracoUserGroup2Apps = new List<UmbracoUserGroup2App>
                {
                    new() { App = Constants.Applications.Translation }
                },
            });
        db.Add(
            new UmbracoUserGroup
            {
                Id = 5,
                UserGroupAlias = Constants.Security.SensitiveDataGroupAlias,
                UserGroupName = "Sensitive data",
                UserGroupDefaultPermissions = string.Empty,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Icon = "icon-lock",
                HasAccessToAllLanguages = false,
                Users = new List<UmbracoUser> { adminUser }
            });

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<UmbracoUserGroup>(); //TODO using dispose?
    }

    private async Task CreateContentTypeData(UmbracoEFContext db)
    {
        // Insert content types only if the corresponding Node record exists (which may or may not have been created depending on configuration
        // of media or member types to create).

        // Media types.
        var existingNodeIds = new HashSet<int>(await db.UmbracoNodes.Select(x => x.Id).ToArrayAsync());

        await db.EnableIdentityInsert<CmsContentType>();


        var folder = new CmsContentType
        {
            Pk = 532,
            NodeId = 1031,
            Alias = Constants.Conventions.MediaTypes.Folder,
            Icon = Constants.Icons.MediaFolder,
            Thumbnail = Constants.Icons.MediaFolder,
            IsContainer = false,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing,
        };
        var image = new CmsContentType
        {
            Pk = 533,
            NodeId = 1032,
            Alias = Constants.Conventions.MediaTypes.Image,
            Icon = Constants.Icons.MediaImage,
            Thumbnail = Constants.Icons.MediaImage,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing
        };
        var file = new CmsContentType
        {
            Pk = 534,
            NodeId = 1033,
            Alias = Constants.Conventions.MediaTypes.File,
            Icon = Constants.Icons.MediaFile,
            Thumbnail = Constants.Icons.MediaFile,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing
        };
        var video = new CmsContentType
        {
            Pk = 540,
            NodeId = 1034,
            Alias = Constants.Conventions.MediaTypes.VideoAlias,
            Icon = Constants.Icons.MediaVideo,
            Thumbnail = Constants.Icons.MediaVideo,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing
        };
        var audio = new CmsContentType
        {
            Pk = 541,
            NodeId = 1035,
            Alias = Constants.Conventions.MediaTypes.AudioAlias,
            Icon = Constants.Icons.MediaAudio,
            Thumbnail = Constants.Icons.MediaAudio,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing
        };
        var article = new CmsContentType
        {
            Pk = 542,
            NodeId = 1036,
            Alias = Constants.Conventions.MediaTypes.ArticleAlias,
            Icon = Constants.Icons.MediaArticle,
            Thumbnail = Constants.Icons.MediaArticle,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing
        };
        var vectorGraphics = new CmsContentType
        {
            Pk = 543,
            NodeId = 1037,
            Alias = Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            Icon = Constants.Icons.MediaVectorGraphics,
            Thumbnail = Constants.Icons.MediaVectorGraphics,
            AllowAtRoot = true,
            Variations = (byte)ContentVariation.Nothing
        };
        var member = new CmsContentType
        {
            Pk = 531,
            NodeId = 1044,
            Alias = Constants.Conventions.MemberTypes.DefaultAlias,
            Icon = Constants.Icons.Member,
            Thumbnail = Constants.Icons.Member,
            Variations = (byte)ContentVariation.Nothing
        };

        if (existingNodeIds.Contains(folder.NodeId))
        {
            db.Add(folder);
        }

        if (existingNodeIds.Contains(image.NodeId))
        {
            db.Add(image);
        }

        if (existingNodeIds.Contains(file.NodeId))
        {
            db.Add(file);
        }

        if (existingNodeIds.Contains(video.NodeId))
        {
            db.Add(video);
        }

        if (existingNodeIds.Contains(audio.NodeId))
        {
            db.Add(audio);
        }

        if (existingNodeIds.Contains(article.NodeId))
        {
            db.Add(article);
        }

        if (existingNodeIds.Contains(vectorGraphics.NodeId))
        {
            db.Add(vectorGraphics);
        }

        // Member type.
        if (existingNodeIds.Contains(member.NodeId))
        {
            db.Add(member);
        }
        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<CmsContentType>();
    }

    private async Task CreateNodeData(UmbracoEFContext db)
    {
        await db.EnableIdentityInsert<UmbracoNode>();

        await CreateNodeDataForDataTypes(db);
        await CreateNodeDataForMediaTypes(db);
        await CreateNodeDataForMemberTypes(db);

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<UmbracoNode>();
    }

    private async Task CreateNodeDataForMemberTypes(UmbracoEFContext db)
    {
        var memberUniqueId = new Guid("d59be02f-1df9-4228-aa1e-01917d806cda");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MemberTypes,
            memberUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1044,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1044",
                SortOrder = 0,
                UniqueId = memberUniqueId,
                Text = Constants.Conventions.MemberTypes.DefaultAlias,
                NodeObjectType = Constants.ObjectTypes.MemberType,
                CreateDate = DateTime.Now
            });
    }

    private async Task CreateNodeDataForMediaTypes(UmbracoEFContext db)
    {
        var folderUniqueId = new Guid("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            folderUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1031,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1031",
                SortOrder = 2,
                UniqueId = folderUniqueId,
                Text = Constants.Conventions.MediaTypes.Folder,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });

        var imageUniqueId = new Guid("cc07b313-0843-4aa8-bbda-871c8da728c8");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            imageUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1032,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1032",
                SortOrder = 2,
                UniqueId = imageUniqueId,
                Text = Constants.Conventions.MediaTypes.Image,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });

        var fileUniqueId = new Guid("4c52d8ab-54e6-40cd-999c-7a5f24903e4d");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            fileUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1033,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1033",
                SortOrder = 2,
                UniqueId = fileUniqueId,
                Text = Constants.Conventions.MediaTypes.File,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });

        var videoUniqueId = new Guid("f6c515bb-653c-4bdc-821c-987729ebe327");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            videoUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1034,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1034",
                SortOrder = 2,
                UniqueId = videoUniqueId,
                Text = Constants.Conventions.MediaTypes.Video,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });

        var audioUniqueId = new Guid("a5ddeee0-8fd8-4cee-a658-6f1fcdb00de3");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            audioUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1035,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1035",
                SortOrder = 2,
                UniqueId = audioUniqueId,
                Text = Constants.Conventions.MediaTypes.Audio,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });

        var articleUniqueId = new Guid("a43e3414-9599-4230-a7d3-943a21b20122");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            articleUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1036,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1036",
                SortOrder = 2,
                UniqueId = articleUniqueId,
                Text = Constants.Conventions.MediaTypes.Article,
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });

        var svgUniqueId = new Guid("c4b1efcf-a9d5-41c4-9621-e9d273b52a9c");
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            svgUniqueId.ToString(),
            new UmbracoNode
            {
                Id = 1037,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1037",
                SortOrder = 2,
                UniqueId = svgUniqueId,
                Text = "Vector Graphics (SVG)",
                NodeObjectType = Constants.ObjectTypes.MediaType,
                CreateDate = DateTime.Now
            });
    }

    private async Task CreateNodeDataForDataTypes(UmbracoEFContext db)
    {
        void InsertDataTypeUmbracoNode(int id, int sortOrder, string uniqueId, string text)
        {
            var UmbracoNode = new UmbracoNode
            {
                Id = id,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1," + id,
                SortOrder = sortOrder,
                UniqueId = new Guid(uniqueId),
                Text = text,
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            };

            ConditionalInsert(db,
                Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
                uniqueId,
                UmbracoNode);
        }

        db.Add(new UmbracoNode
        {
            Id = -1,
            Trashed = false,
            ParentId = -1,
            NodeUser = -1,
            Level = 0,
            Path = "-1",
            SortOrder = 0,
            UniqueId = new Guid("916724a5-173d-4619-b97e-b9de133dd6f5"),
            Text = "SYSTEM DATA: umbraco master root",
            NodeObjectType = Constants.ObjectTypes.SystemRoot,
            CreateDate = DateTime.Now
        });
        db.Add(new UmbracoNode
        {
            Id = -20,
            Trashed = false,
            ParentId = -1,
            NodeUser = -1,
            Level = 0,
            Path = "-1,-20",
            SortOrder = 0,
            UniqueId = new Guid("0F582A79-1E41-4CF0-BFA0-76340651891A"),
            Text = "Recycle Bin",
            NodeObjectType = Constants.ObjectTypes.ContentRecycleBin,
            CreateDate = DateTime.Now
        });
        db.Add(new UmbracoNode
        {
            Id = -21,
            Trashed = false,
            ParentId = -1,
            NodeUser = -1,
            Level = 0,
            Path = "-1,-21",
            SortOrder = 0,
            UniqueId = new Guid("BF7C7CBC-952F-4518-97A2-69E9C7B33842"),
            Text = "Recycle Bin",
            NodeObjectType = Constants.ObjectTypes.MediaRecycleBin,
            CreateDate = DateTime.Now
        });

        InsertDataTypeUmbracoNode(Constants.DataTypes.LabelString, 35, Constants.DataTypes.Guids.LabelString,
            "Label (string)");
        InsertDataTypeUmbracoNode(Constants.DataTypes.LabelInt, 36, Constants.DataTypes.Guids.LabelInt,
            "Label (integer)");
        InsertDataTypeUmbracoNode(Constants.DataTypes.LabelBigint, 36, Constants.DataTypes.Guids.LabelBigInt,
            "Label (bigint)");
        InsertDataTypeUmbracoNode(Constants.DataTypes.LabelDateTime, 37, Constants.DataTypes.Guids.LabelDateTime,
            "Label (datetime)");
        InsertDataTypeUmbracoNode(Constants.DataTypes.LabelTime, 38, Constants.DataTypes.Guids.LabelTime,
            "Label (time)");
        InsertDataTypeUmbracoNode(Constants.DataTypes.LabelDecimal, 39, Constants.DataTypes.Guids.LabelDecimal,
            "Label (decimal)");


        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Upload,
            new UmbracoNode
            {
                Id = Constants.DataTypes.Upload,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Upload}",
                SortOrder = 34,
                UniqueId = Constants.DataTypes.Guids.UploadGuid,
                Text = "Upload File",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadVideo,
            new UmbracoNode
            {
                Id = Constants.DataTypes.UploadVideo,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadVideo}",
                SortOrder = 35,
                UniqueId = Constants.DataTypes.Guids.UploadVideoGuid,
                Text = "Upload Video",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadAudio,
            new UmbracoNode
            {
                Id = Constants.DataTypes.UploadAudio,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadAudio}",
                SortOrder = 36,
                UniqueId = Constants.DataTypes.Guids.UploadAudioGuid,
                Text = "Upload Audio",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadArticle,
            new UmbracoNode
            {
                Id = Constants.DataTypes.UploadArticle,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadArticle}",
                SortOrder = 37,
                UniqueId = Constants.DataTypes.Guids.UploadArticleGuid,
                Text = "Upload Article",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.UploadVectorGraphics,
            new UmbracoNode
            {
                Id = Constants.DataTypes.UploadVectorGraphics,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.UploadVectorGraphics}",
                SortOrder = 38,
                UniqueId = Constants.DataTypes.Guids.UploadVectorGraphicsGuid,
                Text = "Upload Vector Graphics",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Textarea,
            new UmbracoNode
            {
                Id = Constants.DataTypes.Textarea,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Textarea}",
                SortOrder = 33,
                UniqueId = Constants.DataTypes.Guids.TextareaGuid,
                Text = "Textarea",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Textstring,
            new UmbracoNode
            {
                Id = Constants.DataTypes.Textbox,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Textbox}",
                SortOrder = 32,
                UniqueId = Constants.DataTypes.Guids.TextstringGuid,
                Text = "Textstring",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.RichtextEditor,
            new UmbracoNode
            {
                Id = Constants.DataTypes.RichtextEditor,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.RichtextEditor}",
                SortOrder = 4,
                UniqueId = Constants.DataTypes.Guids.RichtextEditorGuid,
                Text = "Richtext editor",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Numeric,
            new UmbracoNode
            {
                Id = -51,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,-51",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.NumericGuid,
                Text = "Numeric",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Checkbox,
            new UmbracoNode
            {
                Id = Constants.DataTypes.Boolean,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Boolean}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.CheckboxGuid,
                Text = "True/false",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.CheckboxList,
            new UmbracoNode
            {
                Id = -43,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,-43",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.CheckboxListGuid,
                Text = "Checkbox list",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Dropdown,
            new UmbracoNode
            {
                Id = Constants.DataTypes.DropDownSingle,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DropDownSingle}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DropdownGuid,
                Text = "Dropdown",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.DatePicker,
            new UmbracoNode
            {
                Id = -41,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,-41",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DatePickerGuid,
                Text = "Date Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Radiobox,
            new UmbracoNode
            {
                Id = -40,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,-40",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.RadioboxGuid,
                Text = "Radiobox",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.DropdownMultiple,
            new UmbracoNode
            {
                Id = Constants.DataTypes.DropDownMultiple,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DropDownMultiple}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DropdownMultipleGuid,
                Text = "Dropdown multiple",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ApprovedColor,
            new UmbracoNode
            {
                Id = -37,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,-37",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ApprovedColorGuid,
                Text = "Approved Color",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.DatePickerWithTime,
            new UmbracoNode
            {
                Id = Constants.DataTypes.DateTime,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DateTime}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.DatePickerWithTimeGuid,
                Text = "Date Picker with time",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ListViewContent,
            new UmbracoNode
            {
                Id = Constants.DataTypes.DefaultContentListView,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DefaultContentListView}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ListViewContentGuid,
                Text = Constants.Conventions.DataTypes.ListViewPrefix + "Content",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ListViewMedia,
            new UmbracoNode
            {
                Id = Constants.DataTypes.DefaultMediaListView,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DefaultMediaListView}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ListViewMediaGuid,
                Text = Constants.Conventions.DataTypes.ListViewPrefix + "Media",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ListViewMembers,
            new UmbracoNode
            {
                Id = Constants.DataTypes.DefaultMembersListView,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.DefaultMembersListView}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ListViewMembersGuid,
                Text = Constants.Conventions.DataTypes.ListViewPrefix + "Members",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.Tags,
            new UmbracoNode
            {
                Id = Constants.DataTypes.Tags,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.Tags}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.TagsGuid,
                Text = "Tags",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ImageCropper,
            new UmbracoNode
            {
                Id = Constants.DataTypes.ImageCropper,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = $"-1,{Constants.DataTypes.ImageCropper}",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ImageCropperGuid,
                Text = "Image Cropper",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });

        // New UDI pickers with newer Ids
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.ContentPicker,
            new UmbracoNode
            {
                Id = 1046,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1046",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.ContentPickerGuid,
                Text = "Content Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MemberPicker,
            new UmbracoNode
            {
                Id = 1047,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1047",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MemberPickerGuid,
                Text = "Member Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker,
            new UmbracoNode
            {
                Id = 1048,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1048",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPickerGuid,
                Text = "Media Picker (legacy)",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MultipleMediaPicker,
            new UmbracoNode
            {
                Id = 1049,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1049",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MultipleMediaPickerGuid,
                Text = "Multiple Media Picker (legacy)",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.RelatedLinks,
            new UmbracoNode
            {
                Id = 1050,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1050",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.RelatedLinksGuid,
                Text = "Multi URL Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });

        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3,
            new UmbracoNode
            {
                Id = 1051,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1051",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3Guid,
                Text = "Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3Multiple,
            new UmbracoNode
            {
                Id = 1052,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1052",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3MultipleGuid,
                Text = "Multiple Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3SingleImage,
            new UmbracoNode
            {
                Id = 1053,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1053",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3SingleImageGuid,
                Text = "Image Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
        ConditionalInsert(db,
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            Constants.DataTypes.Guids.MediaPicker3MultipleImages,
            new UmbracoNode
            {
                Id = 1054,
                Trashed = false,
                ParentId = -1,
                NodeUser = -1,
                Level = 1,
                Path = "-1,1054",
                SortOrder = 2,
                UniqueId = Constants.DataTypes.Guids.MediaPicker3MultipleImagesGuid,
                Text = "Multiple Image Media Picker",
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now
            });
    }


    private void ConditionalInsert<T>(UmbracoEFContext db, string configKey, string id, [DisallowNull] T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var alwaysInsert = _entitiesToAlwaysCreate.ContainsKey(configKey) &&
                           _entitiesToAlwaysCreate[configKey].InvariantContains(id);

        InstallDefaultDataSettings installDefaultDataSettings = _installDefaultDataSettings.Get(configKey);

        // If there's no configuration, we assume to create.
        if (installDefaultDataSettings is null)
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

        db.Add(entity);
    }

    private async Task CreateLockData(UmbracoEFContext db)
    {
        db.UmbracoLocks.Add(new UmbracoLock { Id = Constants.Locks.Servers, Name = nameof(Constants.Locks.Servers) });
        db.UmbracoLocks.Add(new UmbracoLock
        {
            Id = Constants.Locks.ContentTypes, Name = nameof(Constants.Locks.ContentTypes)
        });
        db.UmbracoLocks.Add(new UmbracoLock
        {
            Id = Constants.Locks.ContentTree, Name = nameof(Constants.Locks.ContentTree)
        });
        db.UmbracoLocks.Add(new UmbracoLock
        {
            Id = Constants.Locks.MediaTypes, Name = nameof(Constants.Locks.MediaTypes)
        });
        db.UmbracoLocks.Add(
            new UmbracoLock { Id = Constants.Locks.MediaTree, Name = nameof(Constants.Locks.MediaTree) });
        db.UmbracoLocks.Add(new UmbracoLock
        {
            Id = Constants.Locks.MemberTypes, Name = nameof(Constants.Locks.MemberTypes)
        });
        db.UmbracoLocks.Add(new UmbracoLock
        {
            Id = Constants.Locks.MemberTree, Name = nameof(Constants.Locks.MemberTree)
        });
        db.UmbracoLocks.Add(new UmbracoLock { Id = Constants.Locks.Domains, Name = nameof(Constants.Locks.Domains) });
        db.UmbracoLocks.Add(
            new UmbracoLock { Id = Constants.Locks.KeyValues, Name = nameof(Constants.Locks.KeyValues) });
        db.UmbracoLocks.Add(
            new UmbracoLock { Id = Constants.Locks.Languages, Name = nameof(Constants.Locks.Languages) });
        db.UmbracoLocks.Add(new UmbracoLock
        {
            Id = Constants.Locks.ScheduledPublishing, Name = nameof(Constants.Locks.ScheduledPublishing)
        });
        db.UmbracoLocks.Add(new UmbracoLock { Id = Constants.Locks.MainDom, Name = nameof(Constants.Locks.MainDom) });

        await db.SaveChangesAsync();
    }

    private async Task CreateLanguageData(UmbracoEFContext db)
    {
        await db.EnableIdentityInsert<UmbracoLanguage>();

        ConditionalInsert(db, Constants.Configuration.NamedOptions.InstallDefaultData.Languages,
            "en-us",
            new UmbracoLanguage
            {
                Id = 1,
                LanguageIsocode = "en-US",
                LanguageCultureName = "English (United States)",
                IsDefaultVariantLang = true
            });

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<UmbracoLanguage>();
    }

    private async Task<UmbracoUser> CreateUserData(UmbracoEFContext db)
    {
        await db.EnableIdentityInsert<UmbracoUser>();

        var adminUser = new UmbracoUser
        {
            Id = Constants.Security.SuperUserId,
            UserDisabled
                = false,
            UserNoConsole = false,
            UserName = "Administrator",
            UserLogin = "admin",
            UserPassword = "default",
            UserEmail = string.Empty,
            UserLanguage = "en-US",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now
        };
        db.UmbracoUsers.Add(adminUser);

        await db.SaveChangesAsync();

        await db.DisableIdentityInsert<UmbracoUser>();

        return adminUser;
    }
}
