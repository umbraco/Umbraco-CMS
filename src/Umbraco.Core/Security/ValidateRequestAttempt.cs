namespace Umbraco.Core.Security
{
    public enum ValidateRequestAttempt
    {
        Success = 0,

        FailedNoPrivileges = 100,

        //FailedTimedOut,

        FailedNoContextId = 101,
        FailedNoSsl = 102
    }
}
