// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Events
{
    public abstract class CancelableObjectNotification<T> : ObjectNotification<T>, ICancelableNotification where T : class
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
}
