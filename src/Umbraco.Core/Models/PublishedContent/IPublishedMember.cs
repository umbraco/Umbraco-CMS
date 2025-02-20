namespace Umbraco.Cms.Core.Models.PublishedContent;

public interface IPublishedMember : IPublishedContent
{
    public string Email { get; }

    public string UserName { get; }

    public string? Comments { get; }

    public bool IsApproved { get; }

    public bool IsLockedOut { get; }

    public DateTime? LastLockoutDate { get; }

    public DateTime CreationDate { get; }

    public DateTime? LastLoginDate { get; }

    public DateTime? LastPasswordChangedDate { get; }
}
