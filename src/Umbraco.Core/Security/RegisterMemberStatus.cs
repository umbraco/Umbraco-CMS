namespace Umbraco.Cms.Core.Security
{
    public enum RegisterMemberStatus
    {
        Success,
        InvalidUserName,
        InvalidPassword,
        InvalidEmail,
        DuplicateUserName,
        DuplicateEmail,
        Error,
    }
}
