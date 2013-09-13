namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IScheduledTask
    {
        string Alias { get; }

        bool Log { get; }

        int Interval { get; }

        string Url { get; }
    }
}