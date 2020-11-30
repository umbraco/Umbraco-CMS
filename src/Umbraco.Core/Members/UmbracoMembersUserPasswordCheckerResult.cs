namespace Umbraco.Core.Members
{
    /// <summary>
    /// The result returned from the IUmbracoMembersUserPasswordChecker
    /// </summary>
    public enum UmbracoMembersUserPasswordCheckerResult
    {
        ValidCredentials,
        InvalidCredentials,
        FallbackToDefaultChecker
    }
}
