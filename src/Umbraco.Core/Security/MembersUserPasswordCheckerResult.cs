namespace Umbraco.Core.Security
{
    /// <summary>
    /// The result returned from the IMembersUserPasswordChecker
    /// </summary>
    public enum MembersUserPasswordCheckerResult
    {
        ValidCredentials,
        InvalidCredentials,
        FallbackToDefaultChecker
    }
}
