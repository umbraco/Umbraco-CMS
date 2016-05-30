namespace Umbraco.Web
{
    internal class SingletonUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public UmbracoContext UmbracoContext => UmbracoContext.Current;
    }
}