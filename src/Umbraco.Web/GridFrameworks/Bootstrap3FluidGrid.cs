namespace Umbraco.Web.GridFrameworks
{
    public class Bootstrap3FluidGrid : UmbracoGridFramework
    {
        /// <inheritdoc />
        public override string ContainerCssClass { get { return "container-fluid"; } }

        /// <inheritdoc />
        public override string RowCssClass { get { return "row-fluid"; } }

        /// <inheritdoc />
        public override string ColumnCssClassPreText { get { return "col-md-"; } }
    }
}