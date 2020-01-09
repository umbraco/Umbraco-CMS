using Umbraco.Core.Models;

namespace Umbraco.Tests.Shared.Builders
{
    public class DataTypeBuilder : BuilderBase<DataType>, IWithIdBuilder
    {
        private readonly DataEditorBuilder<DataTypeBuilder> _dataEditorBuilder;
        private int? _id;

        public DataTypeBuilder()
        {
            _dataEditorBuilder = new DataEditorBuilder<DataTypeBuilder>(this);
        }

        public override DataType Build()
        {
            var editor = _dataEditorBuilder.Build();
            var id = _id ?? 1;
            var result = new DataType(editor)
            {
                Id = id
            };

            return result;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
