namespace Umbraco.Web
{
    internal class SingletonUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public UmbracoContext Value
        {
            get { return UmbracoContext.Current; }           
        }
    }
}