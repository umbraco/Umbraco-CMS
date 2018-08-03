namespace Umbraco.Web
{
    /// <summary>
    /// Used to retrieve the HttpContext
    /// </summary>
    /// <remarks>
    /// NOTE: This has a singleton lifespan
    /// </remarks>
    public interface IHttpContextAccessor : Core.IHttpContextAccessor
    { }
}