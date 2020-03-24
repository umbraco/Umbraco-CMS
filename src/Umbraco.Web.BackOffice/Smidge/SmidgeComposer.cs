using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.BackOffice.Smidge
{
    public sealed class SmidgeComposer : ComponentComposer<SmidgeComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.RegisterUnique<IRuntimeMinifier, SmidgeRuntimeMinifier>();
        }
    }
}
