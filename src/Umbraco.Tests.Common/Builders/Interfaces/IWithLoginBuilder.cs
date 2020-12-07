namespace Umbraco.Tests.Common.Builders.Interfaces
{
    public interface IWithLoginBuilder
    {
        string Username { get; set; }

        string RawPasswordValue { get; set; }

        string PasswordConfig { get; set; }
    }
}
