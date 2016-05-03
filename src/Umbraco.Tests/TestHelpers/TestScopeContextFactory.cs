using Umbraco.Core;

namespace Umbraco.Tests.TestHelpers
{
    internal class TestScopeContextFactory : IScopeContextFactory
    {
        private readonly bool _transient = false;
        private TestScopeContext _ctx;

        public TestScopeContextFactory(bool transient = false)
        {
            _transient = transient;
        }
        
        public IScopeContext GetContext()
        {
            return _transient ? new TestScopeContext() : (_ctx ?? (_ctx = new TestScopeContext()));
        }
    }
}