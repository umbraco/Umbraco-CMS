using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.GridFrameworks
{
    public abstract class UmbracoGridFramework : IGridFramework
    {
        /// <summary>
        /// Gets the grid value.
        /// </summary>
        /// <value>
        /// The grid value.
        /// </value>
        public GridValue GridValue { get; set; }

        /// <summary>
        /// Gets the grid CSS class.
        /// </summary>
        /// <value>
        /// The grid CSS class.
        /// </value>
        public virtual string GridCssClass { get { return "umb-grid"; } }

        /// <summary>
        /// Gets the container CSS class.
        /// </summary>
        /// <value>
        /// The container CSS class.
        /// </value>
        public virtual string ContainerCssClass { get { return "container"; } }

        /// <summary>
        /// Gets the section CSS class.
        /// </summary>
        /// <value>
        /// The section CSS class.
        /// </value>
        public virtual string SectionCssClass { get { return "grid-section"; } }

        /// <summary>
        /// Gets the row CSS class.
        /// </summary>
        /// <value>
        /// The row CSS class.
        /// </value>
        public virtual string RowCssClass { get { return "row"; } }

        /// <summary>
        /// Gets the column CSS class.
        /// </summary>
        /// <value>
        /// The column CSS class.
        /// </value>
        public virtual string ColumnCssClass { get { return "column"; } }

        /// <summary>
        /// Gets the column CSS class pre text.
        /// </summary>
        /// <value>
        /// The column CSS class pre text.
        /// </value>
        public abstract string ColumnCssClassPreText { get; }

        /// <summary>
        /// Gets the partial views path.
        /// </summary>
        /// <value>
        /// The partial views path.
        /// </value>
        public virtual string PartialViewsPath { get { return "grid/editors/"; } }

        /// <summary>
        /// Gets the section HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="sectionIndex">Index of the section.</param>
        /// <returns></returns>
        public IHtmlString GetSectionHtml(HtmlHelper html, int sectionIndex)
        {
            Mandate.That<ArgumentNullException>(HasGridValue);
            var section = GridValue.Sections.ToList()[sectionIndex];
            var singleSection = section.Grid < 12;
            var columnDiv = GridHelper.GetDivWrapper(singleSection ? SectionCssClass : ColumnCssClassPreText + section.Grid);
            if (singleSection == false)
            {
                columnDiv.AddClassName(ColumnCssClass);
            }
            foreach (var rowHtml in section.Rows.Select(gridRow => GetRowHtml(html, gridRow, singleSection)))
            {
                columnDiv.AddChild(rowHtml.ToHtmlString());
            }
            return columnDiv.ToHtml();
        }

        /// <summary>
        /// Gets the sections HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public IHtmlString GetSectionsHtml(HtmlHelper html)
        {
            Mandate.That<ArgumentNullException>(HasGridValue);
            var sectionsDiv = GridHelper.GetDivWrapper(RowCssClass);
            sectionsDiv.AddClassName("clearfix");

            foreach (var columHtml in GridValue.Sections.Select(section => GetSectionHtml(html, GridValue.Sections.IndexOf(section))))
            {
                var sectionDiv = GridHelper.GetDivWrapper(SectionCssClass);
                sectionDiv.AddChild(columHtml.ToHtmlString());
                sectionsDiv.AddChild(sectionDiv);
            }

            var containerDiv = GridHelper.GetDivWrapper(ContainerCssClass);
            return containerDiv.AddChild(sectionsDiv).ToHtml();
        }

        /// <summary>
        /// Gets the row HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="row">The grid row.</param>
        /// <param name="wrapInContainer">if set to <c>true</c> [single column].</param>
        /// <returns></returns>
        public IHtmlString GetRowHtml(HtmlHelper html, GridValue.GridRow row, bool wrapInContainer)
        {
            Mandate.That<ArgumentNullException>(row != null);
            var rowDiv = GridHelper.GetDivWrapper(RowCssClass);
            rowDiv.AddClassName("clearfix");

            foreach (var area in row.Areas)
            {
                rowDiv.AddChild(GetAreaHtml(html, area).ToHtmlString());
            }

            var rowDivWrapper = GridHelper.GetDivWrapper(row);
            rowDivWrapper.AddChild(
                wrapInContainer
                    ? GridHelper.GetDivWrapper(ContainerCssClass).AddChild(rowDiv)
                    : rowDiv
            );
            return rowDivWrapper.ToHtml();
        }

        /// <summary>
        /// Gets the area HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="area">The area.</param>
        /// <returns></returns>
        public IHtmlString GetAreaHtml(HtmlHelper html, GridValue.GridArea area)
        {
            Mandate.That<ArgumentNullException>(area != null);
            var areaDivWrapper = GridHelper.GetDivWrapper(area);

            // Render all the controls to HtmlStrings
            foreach (var control in area.Controls.Where(c => c != null && c.Editor != null && c.Editor.View != null))
            {
                areaDivWrapper.AddChild(GetControlHtml(html, control).ToHtmlString());
            }

            var areaColumnDiv = GridHelper.GetDivWrapper(ColumnCssClassPreText + area.Grid);
            areaColumnDiv.AddClassName(ColumnCssClass);
            areaColumnDiv.AddChild(areaDivWrapper);
            return areaColumnDiv.ToHtml();
        }

        /// <summary>
        /// Gets the control HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public virtual IHtmlString GetControlHtml(HtmlHelper html, GridValue.GridControl control)
        {
            var view = (control.Editor.Render ?? control.Editor.View).Replace(".html", ".cshtml");
            if (view.Contains("/") == false)
            {
                view = PartialViewsPath + view;
            }
            return html.Partial(view, JToken.FromObject(control));
        }

        /// <summary>
        /// Gets a value indicating whether this instance has grid value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has grid value; otherwise, <c>false</c>.
        /// </value>
        public bool HasGridValue
        {
            get { return GridValue != null; }
        }
    }
}