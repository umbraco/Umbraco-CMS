namespace Umbraco.Web
{
    /// <summary>
    /// Used to retrieve the Umbraco context
    /// </summary>
    /// <remarks>
    /// TODO: We could expose this to make working with UmbracoContext easier if we were to use it throughout the codebase
    /// </remarks>
    internal interface IUmbracoContextAccessor
    {
        UmbracoContext Value { get; }
    }
}