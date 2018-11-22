using System;

namespace Umbraco.Core.Logging
{
    internal class VoidProfiler : IProfiler
    {
        private readonly VoidDisposable _disposable = new VoidDisposable();

        public string Render()
        {
            return string.Empty;
        }

        public IDisposable Step(string name)
        {
            return _disposable;
        }

        public void Start()
        { }

        public void Stop(bool discardResults = false)
        { }

        private class VoidDisposable : DisposableObject
        {
            protected override void DisposeResources()
            { }
        }
    }
}
