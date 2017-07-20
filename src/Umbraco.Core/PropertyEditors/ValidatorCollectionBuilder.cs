using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    internal class ValidatorCollectionBuilder : LazyCollectionBuilderBase<ValidatorCollectionBuilder, ValidatorCollection, ManifestValueValidator>
    {
        public ValidatorCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ValidatorCollectionBuilder This => this;
    }
}
