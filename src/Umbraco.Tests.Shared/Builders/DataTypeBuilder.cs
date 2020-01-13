using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Shared.Builders.Interfaces;

namespace Umbraco.Tests.Shared.Builders
{
    public class DataTypeBuilder
        : BuilderBase<DataType>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithDeleteDateBuilder,
            IWithNameBuilder
    {
        private readonly DataEditorBuilder<DataTypeBuilder> _dataEditorBuilder;
        private int? _id;
        private int? _parentId;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private DateTime? _deleteDate;
        private string _name;
        private bool? _trashed;
      //  private object _configuration;
        private int? _level;
        private string _path;
        private int? _creatorId;
        private ValueStorageType? _databaseType;
        private int? _sortOrder;

        public DataTypeBuilder()
        {
            _dataEditorBuilder = new DataEditorBuilder<DataTypeBuilder>(this);
        }

        public DataTypeBuilder WithParentId(int parentId)
        {
            _parentId = parentId;
            return this;
        }

        public DataTypeBuilder WithTrashed(bool trashed)
        {
            _trashed = trashed;
            return this;
        }

        // public DataTypeBuilder WithConfiguration(object configuration)
        // {
        //     _configuration = configuration;
        //     return this;
        // }

        public DataTypeBuilder WithLevel(int level)
        {
            _level = level;
            return this;
        }

        public DataTypeBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public DataTypeBuilder WithCreatorId(int creatorId)
        {
            _creatorId = creatorId;
            return this;
        }

        public DataTypeBuilder WithDatabaseType(ValueStorageType databaseType)
        {
            _databaseType = databaseType;
            return this;
        }

        public DataTypeBuilder WithSortOrder(int sortOrder)
        {
            _sortOrder = sortOrder;
            return this;
        }

        public DataEditorBuilder<DataTypeBuilder> AddEditor()
        {
            return _dataEditorBuilder;
        }

        public override DataType Build()
        {
            var editor = _dataEditorBuilder.Build();
            var parentId = _parentId ?? -1;
            var id = _id ?? 1;
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var deleteDate = _deleteDate ?? null;
            var name = _name ?? Guid.NewGuid().ToString();
         //   var configuration = _configuration ?? editor.GetConfigurationEditor().DefaultConfigurationObject;
            var level = _level ?? 0;
            var path = _path ?? string.Empty;
            var creatorId = _creatorId ?? 1;
            var databaseType = _databaseType ?? ValueStorageType.Ntext;
            var sortOrder = _sortOrder ?? 0;

            return new DataType(editor, parentId)
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
                SortOrder = sortOrder,
            };
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

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        DateTime? IWithDeleteDateBuilder.DeleteDate
        {
            get => _deleteDate;
            set => _deleteDate = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }
    }
}
