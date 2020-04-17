namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithFailedPasswordAttemptsBuilder
    {
        int? FailedPasswordAttempts { get; set; }
    }
}
