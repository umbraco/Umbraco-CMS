using System.Collections.Generic;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services
{
    // TODO split this file into several small classes and move to another namespace

    public interface ICancelableNotification
    {
        bool Cancel { get; set; }
    }

    public abstract class ObjectNotification<T> : INotification where T : class
    {
        protected ObjectNotification(T target, EventMessages messages)
        {
            Messages = messages;
            Target = target;
        }

        public EventMessages Messages { get; }

        protected T Target { get; }
    }

    public abstract class EnumerableObjectNotification<T> : ObjectNotification<IEnumerable<T>>
    {
        protected EnumerableObjectNotification(T target, EventMessages messages) : base(new [] {target}, messages)
        {
        }

        protected EnumerableObjectNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }
    }

    public abstract class CancelableObjectNotification<T> : ObjectNotification<T> where T : class
    {
        protected CancelableObjectNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public bool Cancel { get; set; }

        public void CancelOperation(EventMessage cancelationMessage)
        {
            Cancel = true;
            cancelationMessage.IsDefaultEventMessage = true;
            Messages.Add(cancelationMessage);
        }
    }

    public abstract class CancelableEnumerableObjectNotification<T> : CancelableObjectNotification<IEnumerable<T>>
    {
        protected CancelableEnumerableObjectNotification(T target, EventMessages messages) : base(new [] {target}, messages)
        {
        }
        protected CancelableEnumerableObjectNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }
    }

    public class DeletingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public DeletingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }

    public class DeletedNotification<T> : EnumerableObjectNotification<T>
    {
        public DeletedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> DeletedEntities => Target;
    }

    public class SortingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public SortingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SortedEntities => Target;
    }

    public class SortedNotification<T> : EnumerableObjectNotification<T>
    {
        public SortedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SortedEntities => Target;
    }

    public class SavingNotification<T> : CancelableEnumerableObjectNotification<T>
    {
        public SavingNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public SavingNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }

    public class SavedNotification<T> : EnumerableObjectNotification<T>
    {
        public SavedNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public SavedNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> SavedEntities => Target;
    }
}
