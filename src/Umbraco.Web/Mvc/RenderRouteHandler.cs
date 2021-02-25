namespace Umbraco.Web.Mvc
{
    // NOTE: already migrated to netcore, just here since the below is referenced still
    public class RenderRouteHandler
    {
        // Define reserved dictionary keys for controller, action and area specified in route additional values data
        internal static class ReservedAdditionalKeys
        {
            internal const string Controller = "c";
            internal const string Action = "a";
            internal const string Area = "ar";
        }
    }
}
