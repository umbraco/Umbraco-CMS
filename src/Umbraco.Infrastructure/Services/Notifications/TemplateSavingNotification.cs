// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class TemplateSavingNotification : SavingNotification<ITemplate>
    {
        public TemplateSavingNotification(ITemplate target, EventMessages messages) : base(target, messages)
        {
        }

        public TemplateSavingNotification(IEnumerable<ITemplate> target, EventMessages messages) : base(target, messages)
        {
        }

        public TemplateSavingNotification(ITemplate target, EventMessages messages,
            Dictionary<string, object> additionalData) : base(target, messages) => AdditionalData = additionalData;

        public TemplateSavingNotification(IEnumerable<ITemplate> target, EventMessages messages,
            Dictionary<string, object> additionalData) : base(target, messages) => AdditionalData = additionalData;

        public Dictionary<string, object> AdditionalData;
    }
}
