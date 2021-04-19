using System;

namespace Umbraco.Cms.Core.Models.PublishedContent
{
    public interface IPublishedMember : IPublishedContent
    {
        string Email { get; }
        string UserName { get; }
        string Comments { get; }
        bool IsApproved { get; }
        bool IsLockedOut { get; }
        DateTime LastLockoutDate { get; }
        DateTime CreationDate { get; }
        DateTime LastLoginDate { get; }
        DateTime LastPasswordChangedDate { get; }
    }
}
