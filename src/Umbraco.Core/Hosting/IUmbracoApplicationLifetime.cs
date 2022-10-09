namespace Umbraco.Cms.Core.Hosting;

public interface IUmbracoApplicationLifetime
{
    /// <summary>
    ///     A value indicating whether the application is restarting after the current request.
    /// </summary>
    bool IsRestarting { get; }

    /// <summary>
    ///     Terminates the current application. The application restarts the next time a request is received for it.
    /// </summary>
    void Restart();
}
