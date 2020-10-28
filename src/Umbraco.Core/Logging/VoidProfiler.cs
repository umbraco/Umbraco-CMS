using System;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Logging
{
    [UmbracoVolatile]
    public class VoidProfiler : IProfiler
    {
        private readonly VoidDisposable _disposable = new VoidDisposable();

        public IDisposable Step(string name)
        {
            return _disposable;
        }

        public void Start()
        { }

        public void Stop(bool discardResults = false)
        { }

        private class VoidDisposable : DisposableObjectSlim
        {
            protected override void DisposeResources()
            { }
        }
    }
}
