// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class DataTypeBuilder
    : BuilderBase<DataType>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreatorIdBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithDeleteDateBuilder,
        IWithNameBuilder,
        IWithParentIdBuilder,
        IWithTrashedBuilder,
        IWithLevelBuilder,
        IWithPathBuilder,
        IWithSortOrderBuilder
{
    private readonly DataEditorBuilder<DataTypeBuilder> _dataEditorBuilder;
    private DateTime? _createDate;
    private int? _creatorId;
    private ValueStorageType? _databaseType;
    private DateTime? _deleteDate;
    private int? _id;
    private Guid? _key;
    private int? _level;
    private string _name;
    private int? _parentId;
    private string _path;
    private int? _sortOrder;
    private bool? _trashed;
    private DateTime? _updateDate;

    public DataTypeBuilder() => _dataEditorBuilder = new DataEditorBuilder<DataTypeBuilder>(this);

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    int? IWithCreatorIdBuilder.CreatorId
    {
        get => _creatorId;
        set => _creatorId = value;
    }

    DateTime? IWithDeleteDateBuilder.DeleteDate
    {
        get => _deleteDate;
        set => _deleteDate = value;
    }

    int? IWithIdBuilder.Id
    {
        get => _id;
        set => _id = value;
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    int? IWithLevelBuilder.Level
    {
        get => _level;
        set => _level = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    int? IWithParentIdBuilder.ParentId
    {
        get => _parentId;
        set => _parentId = value;
    }

    string IWithPathBuilder.Path
    {
        get => _path;
        set => _path = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    bool? IWithTrashedBuilder.Trashed
    {
        get => _trashed;
        set => _trashed = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public DataTypeBuilder WithDatabaseType(ValueStorageType databaseType)
    {
        _databaseType = databaseType;
        return this;
    }

    public DataEditorBuilder<DataTypeBuilder> AddEditor() => _dataEditorBuilder;

    public override DataType Build()
    {
        var editor = _dataEditorBuilder.Build();
        var parentId = _parentId ?? -1;
        var id = _id ?? 1;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var deleteDate = _deleteDate;
        var name = _name ?? Guid.NewGuid().ToString();
        var level = _level ?? 0;
        var path = _path ?? $"-1,{id}";
        var creatorId = _creatorId ?? 1;
        var databaseType = _databaseType ?? ValueStorageType.Ntext;
        var sortOrder = _sortOrder ?? 0;
        var serializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var dataType = new DataType(editor, serializer, parentId)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            DeleteDate = deleteDate,
            Name = name,
            Trashed = _trashed ?? false,
            Level = level,
            Path = path,
            CreatorId = creatorId,
            DatabaseType = databaseType,
            SortOrder = sortOrder
        };

        return dataType;
    }

    public static DataType CreateSimpleElementDataType(
        IIOHelper ioHelper,
        string editorAlias,
        Guid elementKey,
        Guid? elementSettingKey)
    {
        Dictionary<string, object> configuration = editorAlias switch
        {
            Constants.PropertyEditors.Aliases.BlockGrid => GetBlockGridBaseConfiguration(),
            Constants.PropertyEditors.Aliases.RichText => GetRteBaseConfiguration(),
            _ => [],
        };

        SetBlockConfiguration(
            configuration,
            elementKey,
            elementSettingKey,
            editorAlias == Constants.PropertyEditors.Aliases.BlockGrid ? true : null);

        var dataTypeBuilder = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Ntext)
            .AddEditor()
            .WithAlias(editorAlias);

        switch (editorAlias)
        {
            case Constants.PropertyEditors.Aliases.BlockGrid:
                dataTypeBuilder.WithConfigurationEditor(
                    new BlockGridConfigurationEditor(ioHelper) { DefaultConfiguration = configuration });
                break;
            case Constants.PropertyEditors.Aliases.BlockList:
                dataTypeBuilder.WithConfigurationEditor(
                    new BlockListConfigurationEditor(ioHelper) { DefaultConfiguration = configuration });
                break;
            case Constants.PropertyEditors.Aliases.SingleBlock:
                dataTypeBuilder.WithConfigurationEditor(
                    new SingleBlockConfigurationEditor(ioHelper) { DefaultConfiguration = configuration });
                break;
            case Constants.PropertyEditors.Aliases.RichText:
                dataTypeBuilder.WithConfigurationEditor(
                    new RichTextConfigurationEditor(ioHelper) { DefaultConfiguration = configuration });
                break;
        }

        return dataTypeBuilder.Done().Build();
    }

    private static void SetBlockConfiguration(
        Dictionary<string, object> dictionary,
        Guid? elementKey,
        Guid? elementSettingKey,
        bool? allowAtRoot)
    {
        if (elementKey is null)
        {
            return;
        }

        dictionary["blocks"] = new[] { BuildBlockConfiguration(elementKey.Value, elementSettingKey, allowAtRoot) };
    }

    private static Dictionary<string, object> GetBlockGridBaseConfiguration() => new() { ["gridColumns"] = 12 };

    private static Dictionary<string, object> GetRteBaseConfiguration()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["maxImageSize"] = 500,
            ["mode"] = "Classic",
            ["toolbar"] = new[]
            {
                "styles", "bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "outdent",
                "indent", "sourcecode", "link", "umbmediapicker", "umbembeddialog"
            },
        };
        return dictionary;
    }

    private static Dictionary<string, object> BuildBlockConfiguration(
        Guid? elementKey,
        Guid? elementSettingKey,
        bool? allowAtRoot)
    {
        var dictionary = new Dictionary<string, object>();
        if (allowAtRoot is not null)
        {
            dictionary.Add("allowAtRoot", allowAtRoot.Value);
        }

        dictionary.Add("contentElementTypeKey", elementKey.ToString());
        if (elementSettingKey is not null)
        {
            dictionary.Add("settingsElementTypeKey", elementSettingKey.ToString());
        }

        return dictionary;
    }
}
