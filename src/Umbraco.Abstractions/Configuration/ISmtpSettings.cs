namespace Umbraco.Core.Configuration
{
    public interface ISmtpSettings
    {
        string From { get; }
        string Host { get; }
        int Port{ get; }
        string PickupDirectoryLocation { get; }
    }
}
