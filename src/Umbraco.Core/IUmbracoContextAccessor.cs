namespace Umbraco.Web
{
    /// <summary>
    /// Provides access to UmbracoContext.
    /// </summary>
    public interface IUmbracoContextAccessor
    {
        IUmbracoContext UmbracoContext { get; set;  }
    }
}
