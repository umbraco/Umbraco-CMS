using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class ParameterEditorCollection : BuilderCollectionBase<IParameterEditor>
    {
        public ParameterEditorCollection(IEnumerable<IParameterEditor> items)
            : base(items)
        { }

        public IParameterEditor this[string alias]
        {
            get
            {
                var editor = this.SingleOrDefault(x => x.Alias == alias);
                if (editor != null) return editor;

                var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(alias);
                return mapped == null ? null : this.SingleOrDefault(x => x.Alias == mapped);
            }
        }
    }
}
