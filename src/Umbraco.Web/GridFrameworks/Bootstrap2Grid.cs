namespace Umbraco.Web.GridFrameworks
{
    public class Bootstrap2Grid : UmbracoGridFramework
    {
        /// <inheritdoc />
        public override string ColumnCssClassPreText { get { return "span"; } }
    }
}