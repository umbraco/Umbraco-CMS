using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Composing;
using Umbraco.Web.Runtime;

namespace Umbraco.Web.BackOffice.Smidge
{
    [ComposeAfter(typeof(WebInitialComponent))]
    public sealed class SmidgeComponent : IComponent
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}
