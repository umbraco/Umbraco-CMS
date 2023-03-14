// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

public interface ICancelableNotification : INotification
{
    bool Cancel { get; set; }
}
