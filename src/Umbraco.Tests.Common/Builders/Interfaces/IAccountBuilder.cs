namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IAccountBuilder : IWithLoginBuilder,
                                       IWithEmailBuilder,
                                       IWithFailedPasswordAttemptsBuilder,
                                       IWithIsApprovedBuilder,
                                       IWithIsLockedOutBuilder,
                                       IWithLastLoginDateBuilder,
                                       IWithLastPasswordChangeDateBuilder
    {
    }
}
