// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
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
