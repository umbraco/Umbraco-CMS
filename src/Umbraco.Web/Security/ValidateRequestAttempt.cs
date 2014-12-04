namespace Umbraco.Web.Security
{
    internal enum ValidateRequestAttempt
    {
        Success,
        FailedNoPrivileges,

        //FailedTimedOut, 

        FailedNoContextId,
        FailedNoSsl
    }
}