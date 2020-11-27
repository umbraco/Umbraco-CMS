namespace Umbraco.Core.Security
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
