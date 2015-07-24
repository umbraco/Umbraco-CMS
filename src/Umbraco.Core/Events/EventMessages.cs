using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Event messages collection
    /// </summary>
    public sealed class EventMessages : DisposableObject
    {
        private readonly List<EventMessage> _msgs = new List<EventMessage>();

        public void Add(EventMessage msg)
        {
            _msgs.Add(msg);
        }

        public int Count
        {
            get { return _msgs.Count; }
        }

        public IEnumerable<EventMessage> GetAll()
        {
            return _msgs;
        }

        protected override void DisposeResources()
        {
            _msgs.Clear();
        }
    }
}