namespace Umbraco.Cms.Core.Notifications;

public class MemberTwoFactorRequestedNotification : INotification
{
    public MemberTwoFactorRequestedNotification(Guid? memberKey) => MemberKey = memberKey;

    public Guid? MemberKey { get; }
}
