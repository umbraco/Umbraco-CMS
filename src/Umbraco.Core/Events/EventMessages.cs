using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Default transient event messages collection
    /// </summary>
    public sealed class EventMessages
    {
        private readonly List<EventMessage> _msgs = new List<EventMessage>();

        public void Add(EventMessage msg)
        {
            _msgs.Add(msg);
        }

        public IEnumerable<EventMessage> GetAll()
        {
            return _msgs;
        }
    }
}