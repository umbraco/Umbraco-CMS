using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
    public interface IGridFramework
    {
        /// <summary>
        /// Gets the grid value.
        /// </summary>
        /// <value>
        /// The grid value.
        /// </value>
        GridValue GridValue { get; set; }

        /// <summary>
        /// Gets the grid CSS class.
        /// </summary>
        /// <value>
        /// The grid CSS class.
        /// </value>
        string GridCssClass { get; }

        /// <summary>
        /// Gets the container CSS class.
        /// </summary>
        /// <value>
        /// The container CSS class.
        /// </value>
        string ContainerCssClass { get; }

        /// <summary>
        /// Gets the section CSS class.
        /// </summary>
        /// <value>
        /// The section CSS class.
        /// </value>
        string SectionCssClass { get; }

        /// <summary>
        /// Gets the row CSS class.
        /// </summary>
        /// <value>
        /// The row CSS class.
        /// </value>
        string RowCssClass { get; }

        /// <summary>
        /// Gets the column CSS class.
        /// </summary>
        /// <value>
        /// The column CSS class.
        /// </value>
        string ColumnCssClass { get; }

        /// <summary>
        /// Gets the column CSS class pre text.
        /// </summary>
        /// <value>
        /// The column CSS class pre text.
        /// </value>
        string ColumnCssClassPreText { get; }

        /// <summary>
        /// Gets the partial views path.
        /// </summary>
        /// <value>
        /// The partial views path.
        /// </value>
        string PartialViewsPath { get; }

        /// <summary>
        /// Gets the section HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="sectionIndex">Index of the section.</param>
        /// <returns></returns>
        IHtmlString GetSectionHtml(HtmlHelper html, int sectionIndex);

        /// <summary>
        /// Gets the sections HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        IHtmlString GetSectionsHtml(HtmlHelper html);

        /// <summary>
        /// Gets the row HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="row">The grid row.</param>
        /// <param name="wrapInContainer">if set to <c>true</c> [wrap in container].</param>
        /// <returns></returns>
        IHtmlString GetRowHtml(HtmlHelper html, GridValue.GridRow row, bool wrapInContainer);

        /// <summary>
        /// Gets the area HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="area">The area.</param>
        /// <returns></returns>
        IHtmlString GetAreaHtml(HtmlHelper html, GridValue.GridArea area);

        /// <summary>
        /// Gets the control HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        IHtmlString GetControlHtml(HtmlHelper html, GridValue.GridControl control);

        /// <summary>
        /// Gets a value indicating whether this instance has grid value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has grid value; otherwise, <c>false</c>.
        /// </value>
        bool HasGridValue { get; }
    }
}