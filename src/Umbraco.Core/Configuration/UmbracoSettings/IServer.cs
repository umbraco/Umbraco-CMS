namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IServer
    {
        string ForcePortnumber { get; }
        string ForceProtocol { get; }
        string ServerAddress { get; }

        string AppId { get; }
        string ServerName { get; }
    }
}