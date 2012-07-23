using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using ClientDependency.Core.Controls;
using ClientDependency.Core;
using umbraco.presentation;

namespace umbraco.editorControls.tags
{
    public class DataEditor : System.Web.UI.UpdatePanel, umbraco.interfaces.IDataEditor, umbraco.interfaces.IUseTags
    {
        #region IDataEditor Members

        cms.businesslogic.datatype.DefaultData _data;
        string _group = "";

        public DataEditor(umbraco.interfaces.IData Data, SortedList Prevalues)
        {
            _data = (cms.businesslogic.datatype.DefaultData)Data;

            if (Prevalues["group"] != null)
                _group = Prevalues["group"].ToString();
        }

        public TextBox tagBox = new TextBox();


        public Control Editor { get { return this; } }

        public void Save()
        {
            int _nodeID;
            int.TryParse(_data.NodeId.ToString(), out _nodeID);
            string allTags = "";

            //first clear out all items associated with this ID...
            umbraco.cms.businesslogic.Tags.Tag.RemoveTagsFromNode(_nodeID, _group);

            var items = tagBox.Text.Trim().Split(',');
            foreach (var item in items)
            {
                var tagName = item.Trim();
                if(string.IsNullOrEmpty(tagName))
                    continue;

                var tagId = cms.businesslogic.Tags.Tag.GetTagId(tagName, _group);
                if(tagId == 0)
                    tagId = cms.businesslogic.Tags.Tag.AddTag(tagName, _group);

                if (tagId > 0)
                {
                    umbraco.cms.businesslogic.Tags.Tag.AssociateTagToNode(_nodeID, tagId);
                }
            }

            //and just in case, we'll save the tags as plain text on the node itself... 
            _data.Value = tagBox.Text;
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public bool TreatAsRichTextEditor
        {
            get { return false; }
        }


        [Obsolete("use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public int getTagId(string tag, string group)
        {
            return umbraco.cms.businesslogic.Tags.Tag.GetTagId(tag, group);
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //ClientDependencyLoader.Instance.RegisterDependency("Application/JQuery/jquery.autocomplete.js", "UmbracoClient", ClientDependencyType.Javascript);
            ClientDependencyLoader.Instance.RegisterDependency("ui/ui-lightness/jquery-ui.custom.css", "UmbracoClient", ClientDependencyType.Css);
            ClientDependencyLoader.Instance.RegisterDependency("css/umbracoGui.css", "UmbracoRoot", ClientDependencyType.Css);

            ClientDependencyLoader.Instance.RegisterDependency("tags/css/jquery.tagsinput.css", "UmbracoClient", ClientDependencyType.Css);
            ClientDependencyLoader.Instance.RegisterDependency("tags/js/jquery.tagsinput.min.js", "UmbracoClient", ClientDependencyType.Javascript);
            

            string _alias = ((umbraco.cms.businesslogic.datatype.DefaultData)_data).PropertyId.ToString();

            //making sure that we have a ID for context
            string pageId = UmbracoContext.Current.Request["id"];

            if (pageId != null) pageId = pageId.Trim();

            if (string.IsNullOrEmpty(pageId))
            {
                // we need an empty try/catch as Node.GetCurrent() will throw an exception if we're outside of Umbraco Context
                try
                {
                    NodeFactory.Node currentNode = NodeFactory.Node.GetCurrent();
                    if (currentNode != null)
                    {
                        pageId = currentNode.Id.ToString();
                    }
                }
                catch
                {
                }
            }

            tagBox.ID = "tagBox2_" + _alias;
            tagBox.CssClass = "umbEditorTextField umbTagBox";

            if (!String.IsNullOrEmpty(pageId))
            {
                var tags = umbraco.cms.businesslogic.Tags.Tag.GetTags(int.Parse(pageId), _group);

                tagBox.Text = string.Join(",", tags.Select(x => x.TagCaption));
            }

            this.ContentTemplateContainer.Controls.Add(tagBox);

            string tagsAutoCompleteScript =
                "jQuery('.umbTagBox').tagsInput({ width: '400px', defaultText: 'Add a tag', minChars: 2, autocomplete_url: '" +
                umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco)
                + "/webservices/TagsAutoCompleteHandler.ashx?group=" + _group + "&id=" + pageId + "&rnd=" +
                DateTime.Now.Ticks + "&format=json' });";


            string tagsAutoCompleteInitScript =
                "jQuery(document).ready(function(){"
                + tagsAutoCompleteScript
                + "});";

            Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "_tagsinit", tagsAutoCompleteInitScript, true);

            if (Page.IsPostBack)
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(), ClientID + "_tags", tagsAutoCompleteScript, true);
            }

        }

        #endregion

        #region IUseTags Members

        public string Group
        {
            get { return _group; }
        }

        #endregion

        #region IUseTags Members


        public void RemoveTag(int nodeId, string tag)
        {
            library.RemoveTagFromNode(nodeId, tag, _group);
        }

        public System.Collections.Generic.List<umbraco.interfaces.ITag> GetTagsFromNode(int nodeId)
        {
            return library.GetTagsFromNodeAsITags(nodeId);
        }

        public System.Collections.Generic.List<umbraco.interfaces.ITag> GetAllTags()
        {
            return library.GetTagsFromGroupAsITags(_group);
        }

        #endregion

        #region IUseTags Members


        public void AddTag(string tag)
        {
            library.AddTag(tag, _group);
        }

        public void AddTagToNode(int nodeId, string tag)
        {
            library.addTagsToNode(nodeId, tag, _group);
        }

        #endregion

        #region IUseTags Members


        public void RemoveTagsFromNode(int nodeId)
        {
            foreach (umbraco.interfaces.ITag t in library.GetTagsFromNodeAsITags(nodeId))
                library.RemoveTagFromNode(nodeId, t.TagCaption, t.Group);
        }

        #endregion
    }
}
