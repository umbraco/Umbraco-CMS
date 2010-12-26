using System;
using System.Configuration;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.presentation;
using ClientDependency.Core.Controls;
using ClientDependency.Core;

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

        public CheckBoxList tagCheckList = new CheckBoxList();
        public TextBox tagBox = new TextBox();


        public Control Editor { get { return this; } }

        public void Save()
        {

            CheckBoxList items = tagCheckList;
            int _nodeID;
            int.TryParse(_data.NodeId.ToString(), out _nodeID);
            string allTags = "";
            int tagId = 0;


            //first clear out all items associated with this ID...
            umbraco.cms.businesslogic.Tags.Tag.RemoveTagsFromNode(_nodeID, _group);

            //and now we add them again...
            foreach (ListItem li in items.Items)
            {
                if (li.Selected)
                {

                    if (li.Value == "0")
                    {
                        tagId = umbraco.cms.businesslogic.Tags.Tag.AddTag(li.Text, _group);
                        li.Value = tagId.ToString();

                    }
                    else
                    {
                        int.TryParse(li.Value, out tagId);
                    }

                    if (tagId > 0)
                    {

                        umbraco.cms.businesslogic.Tags.Tag.AssociateTagToNode(_nodeID, tagId);

                        tagId = 0;
                        allTags += "," + li.Text;
                    }
                }
            }
            //and just in case, we'll save the tags as plain text on the node itself... 
            _data.Value = allTags.Trim().Trim(',');
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        public void tagBoxTextChange(object sender, EventArgs e)
        {
            try
            {
                if (tagBox.Text.Trim().Length > 0)
                {
                    CheckBoxList items = tagCheckList;

                    string[] tags = tagBox.Text.Trim().Trim(',').Split(',');


                    for (int i = 0; i < tags.Length; i++)
                    {
                        //if not found we'll get zero and handle that onsave instead...
                        int id = getTagId(tags[i], _group);

                        //we don't want 2 of a kind... 
                        if (items.Items.FindByText(tags[i].Trim()) == null)
                        {
                            ListItem li = new ListItem(tags[i], id.ToString());
                            li.Selected = true;
                            items.Items.Add(li);
                        }
                    }

                    //reset the textbox
                    tagBox.Text = "";

                    ScriptManager.GetCurrent(Page).SetFocus(tagBox);
                }

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, ex.ToString());
            }
        }


        [Obsolete("use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public int getTagId(string tag, string group)
        {
            return umbraco.cms.businesslogic.Tags.Tag.GetTagId(tag, group);
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ClientDependencyLoader.Instance.RegisterDependency("Application/JQuery/jquery.autocomplete.js", "UmbracoClient", ClientDependencyType.Javascript);
            ClientDependencyLoader.Instance.RegisterDependency("css/umbracoGui.css", "UmbracoRoot", ClientDependencyType.Css);

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


            tagBox.ID = "tagBox_" + _alias;
            tagBox.AutoPostBack = true;
            tagBox.AutoCompleteType = AutoCompleteType.Disabled;
            tagBox.TextChanged += new System.EventHandler(this.tagBoxTextChange);
            tagBox.CssClass = "umbEditorTextField";

            Button tagButton = new Button();
            tagButton.Click += new System.EventHandler(this.tagBoxTextChange);
            tagButton.Text = "tag";


            tagCheckList.ID = "tagCheckList_" + _alias;

            if (!String.IsNullOrEmpty(pageId))
            {
                var tags = umbraco.cms.businesslogic.Tags.Tag.GetTags(int.Parse(pageId), _group);

                foreach (var t in tags)
                {
                    ListItem li = new ListItem(t.TagCaption, t.Id.ToString());
                    li.Selected = true;
                    tagCheckList.Items.Add(li);
                }
            }

            this.ContentTemplateContainer.Controls.Add(tagBox);
            this.ContentTemplateContainer.Controls.Add(tagButton);
            this.ContentTemplateContainer.Controls.Add(tagCheckList);

            string tagsAutoCompleteScript =
                 "jQuery(\"#"
                 + tagBox.ClientID + "\").autocomplete(\""
                 + umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco)
                 + "/webservices/TagsAutoCompleteHandler.ashx\",{minChars: 2,max: 100, extraParams:{group:\"" + _group + "\",id:\"" + pageId + "\",rnd:\"" + DateTime.Now.Ticks + "\"}}).result(function(e, data){jQuery(\"#" + tagButton.ClientID + "\").trigger('click');});";


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
