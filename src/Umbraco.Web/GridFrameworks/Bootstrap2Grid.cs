namespace Umbraco.Web.GridFrameworks
{
    public class Bootstrap2Grid : UmbracoGridFramework
    {
        /// <summary>
        /// Gets the column CSS class pre text.
        /// </summary>
        /// <value>
        /// The column CSS class pre text.
        /// </value>
        public override string ColumnCssClassPreText { get { return "span"; } }
    }
}