namespace Umbraco.Cms.Web.Common.Constants;

/// <summary>
///     constants
/// </summary>
internal static class ViewConstants
{
    internal const string ViewLocation = "~/Views";

    internal const string DataTokenCurrentViewContext = "umbraco-current-view-context";

    internal static class ReservedAdditionalKeys
    {
        internal const string Controller = "c";
        internal const string Action = "a";
        internal const string Area = "ar";
    }
}
