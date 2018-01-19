using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class ParameterEditorCollectionBuilder : LazyCollectionBuilderBase<ParameterEditorCollectionBuilder, ParameterEditorCollection, IParameterEditor>
    {
        private readonly ManifestParser _manifestParser;

        public ParameterEditorCollectionBuilder(IServiceContainer container, ManifestParser manifestParser)
            : base(container)
        {
            _manifestParser = manifestParser;
        }

        protected override ParameterEditorCollectionBuilder This => this;

        protected override IEnumerable<IParameterEditor> CreateItems(params object[] args)
        {
            //return base.CreateItems(args).Union(_manifestBuilder.PropertyEditors);

            // the buider's producer returns all IParameterEditor implementations
            // this includes classes inheriting from both PropertyEditor and ParameterEditor
            // but only some PropertyEditor inheritors are also parameter editors
            //
            // return items,
            // that are NOT PropertyEditor OR that also have IsParameterEditor set to true
            // union all manifest's parameter editors
            // union all manifest's property editors that are ALSO parameter editors

            return base.CreateItems(args)
                .Where(x => (x is PropertyEditor) == false || ((PropertyEditor) x).IsParameterEditor)
                .Union(_manifestParser.Manifest.ParameterEditors)
                .Union(_manifestParser.Manifest.PropertyEditors.Where(x => x.IsParameterEditor))
                .ToList();
        }
    }
}
