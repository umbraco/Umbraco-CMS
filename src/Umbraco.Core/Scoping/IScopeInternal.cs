using System.Data;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    internal interface IScopeInternal : IScope
    {
        IScopeInternal ParentScope { get; }
        bool CallContext { get; }
        IsolationLevel IsolationLevel { get; }
        UmbracoDatabase DatabaseOrNull { get; }
        EventMessages MessagesOrNull { get; }
        bool ScopedFileSystems { get; }
        void ChildCompleted(bool? completed);
        void Reset();
    }
}
