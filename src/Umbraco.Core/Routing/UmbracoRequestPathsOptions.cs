namespace Umbraco.Cms.Core.Routing;

public class UmbracoRequestPathsOptions
{
    /// <summary>
    ///     Gets the delegate that allows us to handle additional URLs as back-office requests.
    ///     This returns false by default and can be overwritten in Startup.cs.
    /// </summary>
    public Func<string, bool> IsBackOfficeRequest { get; set; } = _ => false;
}
