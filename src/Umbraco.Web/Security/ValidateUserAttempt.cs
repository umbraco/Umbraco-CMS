namespace Umbraco.Web.Security
{
    internal enum ValidateUserAttempt
    {
        Success,
        FailedNoPrivileges,
        FailedTimedOut,
        FailedNoContextId       
    }
}