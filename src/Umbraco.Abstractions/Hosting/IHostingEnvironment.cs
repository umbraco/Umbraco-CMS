namespace Umbraco.Core.Hosting
{
    public interface IHostingEnvironment
    {
        string SiteName { get; }
        string ApplicationId { get; }
        string ApplicationPhysicalPath { get; }

        string LocalTempPath { get; }
        string ApplicationVirtualPath { get; }

        bool IsDebugMode { get; }
        bool IsHosted { get; }
        string MapPath(string path);
    }
}
