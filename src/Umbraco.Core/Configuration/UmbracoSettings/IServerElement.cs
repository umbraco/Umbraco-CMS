namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IServerElement
    {
        string ForcePortnumber { get; }
        string ForceProtocol { get; }
        string ServerAddress { get; }
    }
}