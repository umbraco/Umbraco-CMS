using System;

namespace Umbraco.Cms.Core.Notifications
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class CannotSuppressNotificationAttribute : Attribute
    {
    }
}
