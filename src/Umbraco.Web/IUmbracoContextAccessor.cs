namespace Umbraco.Web
{
    /// <summary>
    /// Used to retrieve the Umbraco context
    /// </summary>
    /// <remarks>
    /// NOTE: This has a singleton lifespan
    /// </remarks>    
    public interface IUmbracoContextAccessor
    {
        UmbracoContext Value { get; }
    }
}