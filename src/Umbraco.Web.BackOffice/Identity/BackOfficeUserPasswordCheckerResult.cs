namespace Umbraco.Web.BackOffice.Identity
{
    /// <summary>
    /// The result returned from the IBackOfficeUserPasswordChecker
    /// </summary>
    public enum BackOfficeUserPasswordCheckerResult
    {
        ValidCredentials,
        InvalidCredentials,
        FallbackToDefaultChecker
    }
}
