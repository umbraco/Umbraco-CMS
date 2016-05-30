namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to IFacade.
    /// </summary>
    interface IFacadeAccessor
    {
        IFacade Facade { get; set; }
    }
}
