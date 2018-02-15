using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class ParameterEditorCollectionBuilder : LazyCollectionBuilderBase<ParameterEditorCollectionBuilder, ParameterEditorCollection, IDataEditor>
    {
        private readonly ManifestParser _manifestParser;

        public ParameterEditorCollectionBuilder(IServiceContainer container, ManifestParser manifestParser)
            : base(container)
        {
            _manifestParser = manifestParser;
        }

        protected override ParameterEditorCollectionBuilder This => this;

        protected override IEnumerable<IDataEditor> CreateItems(params object[] args)
        {
            return base.CreateItems(args)
                .Where(x => (x.Type & EditorType.MacroParameter) > 0)
                .Union(_manifestParser.Manifest.ParameterEditors);
        }
    }
}
