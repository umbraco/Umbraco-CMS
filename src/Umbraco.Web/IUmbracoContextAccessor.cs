namespace Umbraco.Web
{
    /// <summary>
    /// Used to retrieve the Umbraco context
    /// </summary>    
    public interface IUmbracoContextAccessor
    {
        UmbracoContext Value { get; }
    }
}