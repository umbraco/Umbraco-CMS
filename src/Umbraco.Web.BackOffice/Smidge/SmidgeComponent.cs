using System;
using Umbraco.Core.Composing;

namespace Umbraco.Web.BackOffice.Smidge
{
    [ComposeAfter(typeof(IComponent))]
    public sealed class SmidgeComponent : IComponent
    {
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }
    }
}
