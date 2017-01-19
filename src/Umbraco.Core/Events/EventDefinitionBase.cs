using System;

namespace Umbraco.Core.Events
{
    public abstract class EventDefinitionBase : IEventDefinition
    {
        protected EventDefinitionBase()
        {
            EventId = Guid.NewGuid();
        }
        public Guid EventId { get; private set; }
        public abstract void RaiseEvent();
    }
}