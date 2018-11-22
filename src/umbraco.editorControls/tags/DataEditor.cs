using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;
using ClientDependency.Core;
using umbraco.interfaces;

namespace umbraco.editorControls.tags
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class DataEditor : TextBox, IDataEditor, IUseTags
    {
        #region IDataEditor Members

        private readonly cms.businesslogic.datatype.DefaultData _data;
        readonly string _group = "";

        public DataEditor(IData Data, SortedList Prevalues)
        {
            _data = (cms.businesslogic.datatype.DefaultData)Data;

            if (Prevalues["group"] != null)
                _group = Prevalues["group"].ToString();
        }

        public Control Editor { get { return this; } }

        public void Save()
        {
            int nodeId = _data.NodeId;

            //first clear out all items associated with this ID...
            cms.businesslogic.Tags.Tag.RemoveTagsFromNode(nodeId, _group);

            UpdateOrAddTags(nodeId);

            //and just in case, we'll save the tags as plain text on the node itself... 
            _data.Value = this.Text;
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
            return cms.businesslogic.Tags.Tag.GetTagId(tag, group);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // register all dependencies - only registered once
            ClientDependencyLoader.Instance.RegisterDependency("css/umbracoGui.css", "UmbracoRoot", ClientDependencyType.Css);
            ClientDependencyLoader.Instance.RegisterDependency("tags/css/jquery.tagsinput.css", "UmbracoClient", ClientDependencyType.Css);
            ClientDependencyLoader.Instance.RegisterDependency("tags/js/jquery.tagsinput.min.js", "UmbracoClient", ClientDependencyType.Javascript);

            var pageId = GetPageId();
            if (!Page.IsPostBack && !String.IsNullOrEmpty(pageId) && string.IsNullOrWhiteSpace(this.Text))
            {
                var tags = cms.businesslogic.Tags.Tag.GetTags(int.Parse(pageId), _group);
                this.Text = string.Join(",", tags.Select(x => x.TagCaption));
            }

            var tagsAutoCompleteScript = TagsAutocompleteScript(pageId);
            var tagsAutoCompleteInitScript = string.Format("jQuery(document).ready(function(){{{0}}});", tagsAutoCompleteScript);

            // register it as a startup script
            Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "_tagsinit", tagsAutoCompleteInitScript, true);
        }

        private string TagsAutocompleteScript(string pageId)
        {
            var tagsAutoCompleteScript = string.Format("jQuery('#{0}').tagsInput({{ width: '400px', defaultText: 'Add a tag', minChars: 2, autocomplete_url: '{1}/webservices/TagsAutoCompleteHandler.ashx?group={2}&id={3}&rnd={4}&format=json' }});", 
                this.ClientID, 
                Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco), 
                _group, 
                pageId, 
                DateTime.Now.Ticks);

            return tagsAutoCompleteScript;
        }

        private static string GetPageId()
        {
            var pageId = Umbraco.Web.UmbracoContext.Current.HttpContext.Request["id"];
            if (string.IsNullOrEmpty(pageId))
            {
                // we need an empty try/catch as Node.GetCurrent() will throw an exception if we're outside of Umbraco Context
                try
                {
                    var currentNode = NodeFactory.Node.GetCurrent();
                    if (currentNode != null)
                    {
                        pageId = currentNode.Id.ToString();
                    }
                }
                catch
                {
                }
            }
            if (pageId != null) pageId = pageId.Trim();
            return pageId;
        }

        private void UpdateOrAddTags(int nodeId)
        {
            var items = this.Text.Trim().Split(',');
            foreach (var item in items)
            {
                var tagName = item.Trim();
                if (string.IsNullOrEmpty(tagName))
                    continue;

                var tagId = cms.businesslogic.Tags.Tag.GetTagId(tagName, _group);
                if (tagId == 0)
                    tagId = cms.businesslogic.Tags.Tag.AddTag(tagName, _group);

                if (tagId > 0)
                {
                    cms.businesslogic.Tags.Tag.AssociateTagToNode(nodeId, tagId);
                }
            }
        }

        #endregion

        #region IUseTags Members

        public string Group
        {
            get { return _group; }
        }

        public void RemoveTag(int nodeId, string tag)
        {
            cms.businesslogic.Tags.Tag.RemoveTagFromNode(nodeId, tag, _group);
        }

        public System.Collections.Generic.List<ITag> GetTagsFromNode(int nodeId)
        {
            return cms.businesslogic.Tags.Tag.GetTags(nodeId).Cast<ITag>().ToList();
        }

        public System.Collections.Generic.List<ITag> GetAllTags()
        {
            return cms.businesslogic.Tags.Tag.GetTags(_group).Cast<ITag>().ToList();
        }

        public void AddTag(string tag)
        {
            cms.businesslogic.Tags.Tag.AddTag(tag, _group);
        }

        public void AddTagToNode(int nodeId, string tag)
        {
            cms.businesslogic.Tags.Tag.AddTagsToNode(nodeId, tag, _group);
        }

        public void RemoveTagsFromNode(int nodeId)
        {
            foreach (var tag in cms.businesslogic.Tags.Tag.GetTags(nodeId).Cast<ITag>().ToList())
                cms.businesslogic.Tags.Tag.RemoveTagFromNode(nodeId, tag.TagCaption, tag.Group);
        }

        #endregion
    }
}
