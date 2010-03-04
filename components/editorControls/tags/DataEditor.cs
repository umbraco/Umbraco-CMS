using System;
using System.Configuration;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.presentation.nodeFactory;
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


        public static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }

        public Control Editor { get { return this; } }

        public void Save()
        {

            CheckBoxList items = tagCheckList;
            int _nodeID;
            int.TryParse(_data.NodeId.ToString(), out _nodeID);
            string allTags = "";
            int tagId = 0;
            //first clear out all items associated with this ID...
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE (nodeId = @nodeId) AND EXISTS (SELECT id FROM cmsTags WHERE (cmsTagRelationship.tagId = id) AND ([group] = @group));",
                SqlHelper.CreateParameter("@nodeId", _nodeID),
                SqlHelper.CreateParameter("@group", _group));

            //and now we add them again...
            foreach (ListItem li in items.Items)
            {
                if (li.Selected)
                {

                    if (li.Value == "0")
                    {

                        tagId = saveTag(li.Text);
                        li.Value = tagId.ToString();

                    }
                    else
                    {
                        int.TryParse(li.Value, out tagId);
                    }

                    if (tagId > 0)
                    {

                        SqlHelper.ExecuteNonQuery("INSERT INTO cmsTagRelationShip(nodeId,tagId) VALUES (@nodeId, @tagId)",
                            SqlHelper.CreateParameter("@nodeId", _nodeID),
                            SqlHelper.CreateParameter("@tagId", tagId)
                        );

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

        private int saveTag(string tag)
        {
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsTags(tag,[group]) VALUES (@tag,@group)",
                SqlHelper.CreateParameter("@tag", tag.Trim()),
                SqlHelper.CreateParameter("@group", _group));
            return getTagId(tag, _group);
        }

        public int getTagId(string tag, string group)
        {
            int retval = 0;
            try
            {
                string sql = "SELECT id FROM cmsTags where tag=@tag AND [group]=@group;";
                object result = SqlHelper.ExecuteScalar<object>(sql,
                    SqlHelper.CreateParameter("@tag", tag),
                    SqlHelper.CreateParameter("@group", _group));

                if (result != null)
                    retval = int.Parse(result.ToString());
            }
            catch (Exception ex)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Error, -1, ex.ToString());
            }

            return retval;
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ClientDependencyLoader.Instance.RegisterDependency("Application/JQuery/jquery.autocomplete.js", "UmbracoClient", ClientDependencyType.Javascript);
            ClientDependencyLoader.Instance.RegisterDependency("css/umbracoGui.css", "UmbracoRoot", ClientDependencyType.Css);
          
            string _alias = ((umbraco.cms.businesslogic.datatype.DefaultData)_data).PropertyId.ToString();

            //making sure that we have a ID for context
            string pageId = UmbracoContext.Current.Request["id"];

            if (string.IsNullOrEmpty(pageId.Trim()))
            {
                Node currentNode = Node.GetCurrent();
                if (currentNode != null)
                {
                    pageId = currentNode.Id.ToString();
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

            //fetch the current tags
            IRecordsReader rr = SqlHelper.ExecuteReader(
                @"SELECT * FROM cmsTags
                  INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id
                  WHERE cmsTags.[group] = @group AND cmsTagRelationship.nodeid = @nodeid",
                SqlHelper.CreateParameter("@group", _group),
                SqlHelper.CreateParameter("@nodeid", pageId));

            while (rr.Read())
            {
                ListItem li = new ListItem(rr.GetString("tag"), rr.GetInt("id").ToString());
                li.Selected = true;
                tagCheckList.Items.Add(li);
            }
            rr.Close();

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
                +"});";

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
