using System;

namespace Umbraco.Core.Events
{
    public interface IEventDefinition
    {
        Guid EventId { get; }

        void RaiseEvent();
    }
}