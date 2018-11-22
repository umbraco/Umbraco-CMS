using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco.controls.Images;
using Umbraco.Core.IO;

namespace umbraco.editorControls.MultiNodeTreePicker
{
    /// <summary>
    /// The item template for the selected items repeater
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    internal class SelectedItemsTemplate : ITemplate
    {
        #region ITemplate Members

        /// <summary>
        /// Creates the template for the repeater item
        /// </summary>
        /// <param name="container"></param>
        public void InstantiateIn(Control container)
        {            
            var itemDiv = new HtmlGenericControl("div");
            itemDiv.ID = "Item";
            itemDiv.Attributes.Add("class", "item");

            var page = (Page)HttpContext.Current.CurrentHandler;
            var imgPreview = (ImageViewer)page.LoadControl(
                string.Concat(SystemDirectories.Umbraco, "/controls/Images/ImageViewer.ascx"));

            imgPreview.ID = "ImgPreview";
            imgPreview.Visible = false; //hidden by default
            imgPreview.ViewerStyle = ImageViewer.Style.Basic;
            itemDiv.Controls.Add(imgPreview);

            var infoBtn = new HtmlAnchor();
            infoBtn.ID = "InfoButton";
            infoBtn.HRef = "javascript:void(0);";
            infoBtn.Attributes.Add("class", "info");
            itemDiv.Controls.Add(infoBtn);

            var innerDiv = new HtmlGenericControl("div");
            innerDiv.ID = "InnerItem";
            innerDiv.Attributes.Add("class", "inner");

            innerDiv.Controls.Add(
                new LiteralControl(@"<ul class=""rightNode"">"));

            var liSelectNode = new HtmlGenericControl("li");
            liSelectNode.Attributes.Add("class", "closed");
            liSelectNode.ID = "SelectedNodeListItem";
            innerDiv.Controls.Add(liSelectNode);

            var selectedNodeLink = new HtmlAnchor();
            selectedNodeLink.ID = "SelectedNodeLink";
            selectedNodeLink.Attributes.Add("class", "sprTree");
            selectedNodeLink.Attributes.Add("title", "Sync tree");
            innerDiv.Controls.Add(selectedNodeLink);

            var selectedNodeText = new Literal();
            selectedNodeText.ID = "SelectedNodeText";
            innerDiv.Controls.Add(selectedNodeText);

            selectedNodeLink.Controls.Add(new LiteralControl("<div>"));
            selectedNodeLink.Controls.Add(selectedNodeText);
            selectedNodeLink.Controls.Add(new LiteralControl("</div>"));

            liSelectNode.Controls.Add(selectedNodeLink);

            innerDiv.Controls.Add(
                new LiteralControl(@"</ul><a class='close' title='Remove' href='javascript:void(0);'></a>"));

            itemDiv.Controls.Add(innerDiv);

            container.Controls.Add(itemDiv);
        }

        #endregion
    }
}
