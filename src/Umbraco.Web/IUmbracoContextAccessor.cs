namespace Umbraco.Web
{
    /// <summary>
    /// Provides access to UmbracoContext.
    /// </summary>
    public interface IUmbracoContextAccessor
    {
        UmbracoContext UmbracoContext { get; set;  }
    }
}
