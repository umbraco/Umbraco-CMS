namespace Umbraco.Web.WebApi
{
    internal class WebApiVersionCheck
    {
        public static System.Version WebApiVersion
        {
            get { return typeof(System.Web.Http.ApiController).Assembly.GetName().Version; }
        }
    }
}