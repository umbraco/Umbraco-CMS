namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to IFacade.
    /// </summary>
    public interface IFacadeAccessor
    {
        IFacade Facade { get; set; }
    }
}
