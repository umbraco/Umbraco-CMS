using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class AddEditorUiToDataType : MigrationBase
{
    private readonly IKeyValueService _keyValueService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<AddEditorUiToDataType> _logger;

    public AddEditorUiToDataType(IMigrationContext context, IKeyValueService keyValueService, IJsonSerializer jsonSerializer, ILogger<AddEditorUiToDataType> logger)
        : base(context)
    {
        _keyValueService = keyValueService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    protected override void Migrate()
    {
        var dataEditorSplitCollectionData = _keyValueService.GetValue("migrateDataEditorSplitCollectionData");
        DataTypeEditorAliasMigrationData[]? migrationData = dataEditorSplitCollectionData.IsNullOrWhiteSpace() is false
            ? _jsonSerializer.Deserialize<DataTypeEditorAliasMigrationData[]>(dataEditorSplitCollectionData)
            : null;

        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .AndSelect<NodeDto>()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>()
            .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<DataTypeDto>(x => x.EditorUiAlias == null);

        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(sql);
        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            dataTypeDto.EditorUiAlias = dataTypeDto.EditorAlias switch
            {
                Constants.PropertyEditors.Aliases.BlockList => "Umb.PropertyEditorUi.BlockList",
                Constants.PropertyEditors.Aliases.BlockGrid => "Umb.PropertyEditorUi.BlockGrid",
                Constants.PropertyEditors.Aliases.CheckBoxList => "Umb.PropertyEditorUi.CheckBoxList",
                Constants.PropertyEditors.Aliases.ColorPicker => "Umb.PropertyEditorUi.ColorPicker",
                Constants.PropertyEditors.Aliases.ColorPickerEyeDropper => "Umb.PropertyEditorUi.EyeDropper",
                Constants.PropertyEditors.Aliases.ContentPicker => "Umb.PropertyEditorUi.DocumentPicker",
                Constants.PropertyEditors.Aliases.DateTime => "Umb.PropertyEditorUi.DatePicker",
                Constants.PropertyEditors.Aliases.DropDownListFlexible => "Umb.PropertyEditorUi.Dropdown",
                Constants.PropertyEditors.Aliases.ImageCropper => "Umb.PropertyEditorUi.ImageCropper",
                Constants.PropertyEditors.Aliases.Integer => "Umb.PropertyEditorUi.Integer",
                Constants.PropertyEditors.Aliases.Decimal => "Umb.PropertyEditorUi.Decimal",
                Constants.PropertyEditors.Aliases.ListView => "Umb.PropertyEditorUi.Collection",
                Constants.PropertyEditors.Aliases.MediaPicker3 => "Umb.PropertyEditorUi.MediaPicker",
                Constants.PropertyEditors.Aliases.MemberPicker => "Umb.PropertyEditorUi.MemberPicker",
                Constants.PropertyEditors.Aliases.MemberGroupPicker => "Umb.PropertyEditorUi.MemberGroupPicker",
                Constants.PropertyEditors.Aliases.MultiNodeTreePicker => "Umb.PropertyEditorUi.ContentPicker",
                Constants.PropertyEditors.Aliases.MultipleTextstring => "Umb.PropertyEditorUi.MultipleTextString",
                Constants.PropertyEditors.Aliases.Label => "Umb.PropertyEditorUi.Label",
                Constants.PropertyEditors.Aliases.RadioButtonList => "Umb.PropertyEditorUi.RadioButtonList",
                Constants.PropertyEditors.Aliases.Slider => "Umb.PropertyEditorUi.Slider",
                Constants.PropertyEditors.Aliases.Tags => "Umb.PropertyEditorUi.Tags",
                Constants.PropertyEditors.Aliases.TextBox => "Umb.PropertyEditorUi.TextBox",
                Constants.PropertyEditors.Aliases.TextArea => "Umb.PropertyEditorUi.TextArea",
                Constants.PropertyEditors.Aliases.RichText => "Umb.PropertyEditorUi.TinyMCE",
                Constants.PropertyEditors.Aliases.TinyMce => "Umb.PropertyEditorUi.TinyMCE",
                Constants.PropertyEditors.Aliases.Boolean => "Umb.PropertyEditorUi.Toggle",
                Constants.PropertyEditors.Aliases.MarkdownEditor => "Umb.PropertyEditorUi.MarkdownEditor",
                Constants.PropertyEditors.Aliases.UserPicker => "Umb.PropertyEditorUi.UserPicker",
                Constants.PropertyEditors.Aliases.UploadField => "Umb.PropertyEditorUi.UploadField",
                Constants.PropertyEditors.Aliases.EmailAddress => "Umb.PropertyEditorUi.EmailAddress",
                Constants.PropertyEditors.Aliases.MultiUrlPicker => "Umb.PropertyEditorUi.MultiUrlPicker",
                _ => null
            };

            if (dataTypeDto.EditorUiAlias is null)
            {
                DataTypeEditorAliasMigrationData? dataTypeMigrationData = migrationData?.FirstOrDefault(md => md.DataTypeId == dataTypeDto.NodeId);
                if (dataTypeMigrationData is not null)
                {
                    // the V13 "data type split data collector" works like this:
                    // - if .EditorUiAlias is set, the editor is based on manifests and should use one of the "Umbraco.Plain" options as .EditorAlias
                    // - if .EditorUiAlias is not set, the editor is based on code and should use its own alias as .EditorUiAlias
                    // unfortunately there is an issue with the migrator, in that it does not handle manifest based editors using valueType=TEXT,
                    // but with the above logic in mind, we can work around that :)
                    if (dataTypeMigrationData.EditorUiAlias.IsNullOrWhiteSpace())
                    {
                        // editor based on code
                        dataTypeDto.EditorUiAlias = dataTypeDto.EditorAlias;
                    }
                    else
                    {
                        // editor based on manifests
                        dataTypeDto.EditorUiAlias = dataTypeMigrationData.EditorUiAlias;
                        dataTypeDto.EditorAlias = dataTypeMigrationData.EditorAlias?.NullOrWhiteSpaceAsNull()
                                                  ?? "Umbraco.Plain.String";
                    }
                }
                else
                {
                    _logger.LogWarning("No migration data was found for the data editor split for data type {name} ({editorAlias} - {id}). Please make sure you're upgrading from the latest V13. The affected data type may not work correctly.", dataTypeDto.NodeDto.Text, dataTypeDto.EditorAlias, dataTypeDto.NodeId);

                    // we *need* an EditorUiAlias - default to the editor alias (let the client handle later)
                    dataTypeDto.EditorUiAlias = dataTypeDto.EditorAlias;
                }
            }

            Database.Update(dataTypeDto);
        }
    }

    private class DataTypeEditorAliasMigrationData
    {
        [JsonPropertyName("DataTypeId")]
        public int DataTypeId { get; set; }

        [JsonPropertyName("EditorUiAlias")]
        public string? EditorUiAlias { get; init; }

        [JsonPropertyName("EditorAlias")]
        public string? EditorAlias { get; init; }
    }
}
