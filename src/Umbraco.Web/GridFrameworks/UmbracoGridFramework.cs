using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.GridFrameworks
{
    public abstract class UmbracoGridFramework : IGridFramework
    {
        /// <inheritdoc />
        public GridValue GridValue { get; private set; }

        /// <inheritdoc />
        public virtual string GridCssClass { get { return "umb-grid"; } }

        /// <inheritdoc />
        public virtual string ContainerCssClass { get { return "container"; } }

        /// <inheritdoc />
        public virtual string SectionCssClass { get { return "grid-section"; } }

        /// <inheritdoc />
        public virtual string RowCssClass { get { return "row"; } }

        /// <inheritdoc />
        public virtual string ColumnCssClass { get { return "column"; } }

        /// <inheritdoc />
        public virtual string ColumnCssClassPreText { get { return "col-md-"; } }

        /// <inheritdoc />
        public virtual string PartialViewsPath { get { return "grid/editors/"; } }

        #region GetHtml

        /// <inheritdoc />
        public IHtmlString GetGridHtml(HtmlHelper html, GridValue grid)
        {
            return GetGridHtml(html, grid, null, null);
        }

        /// <inheritdoc />
        public IHtmlString GetGridHtml(HtmlHelper html, GridValue grid, Func<HtmlTagWrapper, HtmlTagWrapper> beforeRowRender)
        {
            return GetGridHtml(html, grid, beforeRowRender, null);
        }

        /// <inheritdoc />
        public IHtmlString GetGridHtml(HtmlHelper html, GridValue grid, Func<HtmlTagWrapper, HtmlTagWrapper> beforeRowRender, Func<HtmlTagWrapper, HtmlTagWrapper> beforeGridRender)
        {
            GridValue = grid;

            Mandate.That<ArgumentNullException>(HasGridValue);
            var gridDiv = GridHelper.GetDivWrapper(GridCssClass);

            if (GridValue.Sections.Count == 1)
            {
                gridDiv.AddChild(GetSectionHtml(html, 0, beforeRowRender).ToHtmlString());
            }
            else if (GridValue.Sections.Count > 1)
            {
                gridDiv.AddChild(GetSectionsHtml(html, beforeRowRender).ToHtmlString());
            }

            return beforeGridRender != null
                ? beforeGridRender(gridDiv).ToHtml()
                : gridDiv.ToHtml();
        }

        /// <inheritdoc />
        public IHtmlString GetSectionHtml(HtmlHelper html, int sectionIndex, Func<HtmlTagWrapper, HtmlTagWrapper> beforeRowRender)
        {
            Mandate.That<ArgumentNullException>(HasGridValue);
            var section = GridValue.Sections[sectionIndex];
            var singleSection = section.Grid < 12;
            var columnDiv = GridHelper.GetDivWrapper(singleSection ? SectionCssClass : ColumnCssClassPreText + section.Grid);
            if (singleSection == false)
            {
                columnDiv.AddClassName(ColumnCssClass);
            }
            foreach (var rowHtml in section.Rows.Select(gridRow => GetRowHtml(html, gridRow, singleSection, beforeRowRender)))
            {
                columnDiv.AddChild(rowHtml.ToHtmlString());
            }
            return columnDiv.ToHtml();
        }

        /// <inheritdoc />
        public IHtmlString GetSectionsHtml(HtmlHelper html, Func<HtmlTagWrapper, HtmlTagWrapper> beforeRowRender)
        {
            Mandate.That<ArgumentNullException>(HasGridValue);
            var sectionsDiv = GridHelper.GetDivWrapper(RowCssClass);
            sectionsDiv.AddClassName("clearfix");

            foreach (var columHtml in GridValue.Sections.Select(section => GetSectionHtml(html, GridValue.Sections.IndexOf(section), beforeRowRender)))
            {
                var sectionDiv = GridHelper.GetDivWrapper(SectionCssClass);
                sectionDiv.AddChild(columHtml.ToHtmlString());
                sectionsDiv.AddChild(sectionDiv);
            }

            var containerDiv = GridHelper.GetDivWrapper(ContainerCssClass);
            return containerDiv.AddChild(sectionsDiv).ToHtml();
        }

        /// <inheritdoc />
        public IHtmlString GetRowHtml(HtmlHelper html, GridValue.GridRow row, bool wrapInContainer, Func<HtmlTagWrapper, HtmlTagWrapper> beforeRowRender)
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
            return beforeRowRender != null
                ? beforeRowRender(rowDivWrapper).ToHtml()
                : rowDivWrapper.ToHtml();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual IHtmlString GetControlHtml(HtmlHelper html, GridValue.GridControl control)
        {
            var view = (control.Editor.Render ?? control.Editor.View).Replace(".html", ".cshtml");
            if (view.Contains("/") == false)
            {
                view = PartialViewsPath + view;
            }
            return html.Partial(view, JToken.FromObject(control));
        }

        #endregion

        /// <inheritdoc />
        public bool HasGridValue
        {
            get { return GridValue != null; }
        }
    }
}