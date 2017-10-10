namespace Umbraco.Web.GridFrameworks
{
    public class Bootstrap3Grid : UmbracoGridFramework
    {
        /// <inheritdoc />
        public override string ColumnCssClassPreText { get { return "col-md-"; } }
    }
}