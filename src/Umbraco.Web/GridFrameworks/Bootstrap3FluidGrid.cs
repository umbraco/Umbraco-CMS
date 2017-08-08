namespace Umbraco.Web.GridFrameworks
{
    public class Bootstrap3FluidGrid : UmbracoGridFramework
    {
        /// <summary>
        /// Gets the container CSS class.
        /// </summary>
        /// <value>
        /// The container CSS class.
        /// </value>
        public override string ContainerCssClass { get { return "container-fluid"; } }

        /// <summary>
        /// Gets the row CSS class.
        /// </summary>
        /// <value>
        /// The row CSS class.
        /// </value>
        public override string RowCssClass { get { return "row-fluid"; } }

        /// <summary>
        /// Gets the column CSS class pre text.
        /// </summary>
        /// <value>
        /// The column CSS class pre text.
        /// </value>
        public override string ColumnCssClassPreText { get { return "col-md-"; } }
    }
}