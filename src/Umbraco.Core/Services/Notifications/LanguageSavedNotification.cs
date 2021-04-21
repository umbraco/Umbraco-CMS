// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class LanguageSavedNotification : SavedNotification<ILanguage>
    {
        public LanguageSavedNotification(ILanguage target, EventMessages messages) : base(target, messages)
        {
        }

        public LanguageSavedNotification(IEnumerable<ILanguage> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
