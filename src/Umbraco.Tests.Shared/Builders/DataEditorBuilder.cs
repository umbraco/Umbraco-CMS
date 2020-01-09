using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Shared.Builders
{
    public class DataEditorBuilder<TParent> : ChildBuilderBase<TParent, IDataEditor>
    {
        public DataEditorBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public override IDataEditor Build()
        {
            var result = new DataEditor(
                Mock.Of<ILogger>(),
                Mock.Of<IDataTypeService>(),
                Mock.Of<ILocalizationService>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>()
                );

            return result;
        }
    }
}
