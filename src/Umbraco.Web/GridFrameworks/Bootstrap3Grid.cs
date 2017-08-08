namespace Umbraco.Web.GridFrameworks
{
    public class Bootstrap3Grid : UmbracoGridFramework
    {
        /// <summary>
        /// Gets the column CSS class pre text.
        /// </summary>
        /// <value>
        /// The column CSS class pre text.
        /// </value>
        public override string ColumnCssClassPreText { get { return "col-md-"; } }
    }
}