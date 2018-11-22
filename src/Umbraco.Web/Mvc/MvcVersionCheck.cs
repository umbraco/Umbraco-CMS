namespace Umbraco.Web.Mvc
{
    internal class MvcVersionCheck
    {
        public static System.Version MvcVersion
        {
            get { return typeof (System.Web.Mvc.Controller).Assembly.GetName().Version; }
        }
    }
}