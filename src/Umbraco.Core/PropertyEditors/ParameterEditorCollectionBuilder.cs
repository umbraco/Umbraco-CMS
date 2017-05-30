using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    internal class ParameterEditorCollectionBuilder : LazyCollectionBuilderBase<ParameterEditorCollectionBuilder, ParameterEditorCollection, IParameterEditor>
    {
        private readonly ManifestBuilder _manifestBuilder;

        public ParameterEditorCollectionBuilder(IServiceContainer container, ManifestBuilder manifestBuilder)
            : base(container)
        {
            _manifestBuilder = manifestBuilder;
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
                .Union(_manifestBuilder.ParameterEditors)
                .Union(_manifestBuilder.PropertyEditors.Where(x => x.IsParameterEditor));
        }
    }
}
