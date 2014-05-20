using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using Umbraco.Core;
using Media = umbraco.cms.businesslogic.media.Media;
using Property = umbraco.cms.businesslogic.property.Property;

namespace umbraco.cms.presentation.Trees
{

    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public abstract class BaseMediaTree : BaseTree
    {
        private User _user;

        public BaseMediaTree(string application)
            : base(application)
        {

        }

        /// <summary>
        /// Returns the current User. This ensures that we don't instantiate a new User object 
        /// each time.
        /// </summary>
        protected User CurrentUser
        {
            get
            {
                return (_user == null ? (_user = UmbracoEnsuredPage.CurrentUser) : _user);
            }
        }

        public override void RenderJS(ref StringBuilder Javascript)
        {
            if (!string.IsNullOrEmpty(this.FunctionToCall))
            {
                Javascript.Append("function openMedia(id) {\n");
                Javascript.Append(this.FunctionToCall + "(id);\n");
                Javascript.Append("}\n");
            }
            else if (!this.IsDialog)
            {
                Javascript.Append(
                    @"
function openMedia(id) {
	" + ClientTools.Scripts.GetContentFrame() + ".location.href = 'editMedia.aspx?id=' + id;" + @"
}
");
            }
        }

        public override void Render(ref XmlTree tree)
        {

            if (UseOptimizedRendering == false)
            {
                //We cannot run optimized mode since there are subscribers to events/methods that require document instances
                // so we'll render the original way by looking up the docs.

                var docs = new Media(m_id).Children;

                var args = new TreeEventArgs(tree);
                OnBeforeTreeRender(docs, args);

                foreach (var dd in docs)
                {
                    var e = dd;
                    var xNode = PerformNodeRender(e.Id, e.Text, e.HasChildren, e.ContentType.IconUrl, e.ContentType.Alias, () => GetLinkValue(e, e.Id.ToString(CultureInfo.InvariantCulture)));


                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                        OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    }
                }

                OnAfterTreeRender(docs, args);
            }
            else
            {
                //We ARE running in optmized mode, this means we will NOT be raising the BeforeTreeRender or AfterTreeRender 
                // events  - we've already detected that there are not subscribers or implementations
                // to call so that is fine.

                var entities = Services.EntityService.GetChildren(m_id, UmbracoObjectTypes.Media).ToArray();

                foreach (UmbracoEntity entity in entities)
                {
                    var e = entity;
                    var xNode = PerformNodeRender(e.Id, entity.Name, e.HasChildren, e.ContentTypeIcon, e.ContentTypeAlias, () => GetLinkValue(e));

                    OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    if (xNode != null)
                    {
                        tree.Add(xNode);
                        OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                    }
                }
            }
        }

        private XmlTreeNode PerformNodeRender(int nodeId, string nodeName, bool hasChildren, string icon, string contentTypeAlias, Func<string> getLinkValue)
        {
            var xNode = XmlTreeNode.Create(this);
            xNode.NodeID = nodeId.ToString(CultureInfo.InvariantCulture);
            xNode.Text = nodeName;

            xNode.HasChildren = hasChildren;
            xNode.Source = this.IsDialog ? GetTreeDialogUrl(nodeId) : GetTreeServiceUrl(nodeId);

            xNode.Icon = icon;
            xNode.OpenIcon = icon;

            if (IsDialog == false)
            {
                if (this.ShowContextMenu == false)
                    xNode.Menu = null;
                xNode.Action = "javascript:openMedia(" + nodeId + ");";
            }
            else
            {
                xNode.Menu = this.ShowContextMenu ? new List<IAction>(new IAction[] { ActionRefresh.Instance }) : null;
                if (this.DialogMode == TreeDialogModes.fulllink)
                {
                    string nodeLink = getLinkValue();
                    if (string.IsNullOrEmpty(nodeLink) == false)
                    {
                        xNode.Action = "javascript:openMedia('" + nodeLink + "');";
                    }
                    else
                    {
                        if (string.Equals(contentTypeAlias, Constants.Conventions.MediaTypes.Folder, StringComparison.OrdinalIgnoreCase))
                        {
                            //#U4-2254 - Inspiration to use void from here: http://stackoverflow.com/questions/4924383/jquery-object-object-error
                            xNode.Action = "javascript:void jQuery('.umbTree #" + nodeId.ToString(CultureInfo.InvariantCulture) + "').click();";
                        }
                        else
                        {
                            xNode.Action = null;
                            xNode.Style.DimNode();
                        }
                    }
                }
                else
                {
                    xNode.Action = "javascript:openMedia('" + nodeId.ToString(CultureInfo.InvariantCulture) + "');";
                }
            }

            return xNode;
        }


        /// <summary>
        /// Returns the value for a link in WYSIWYG mode, by default only media items that have a 
        /// DataTypeUploadField are linkable, however, a custom tree can be created which overrides
        /// this method, or another GUID for a custom data type can be added to the LinkableMediaDataTypes
        /// list on application startup.
        /// </summary>
        /// <param name="dd"></param>
        /// <param name="nodeLink"></param>
        /// <returns></returns>
        public virtual string GetLinkValue(Media dd, string nodeLink)
        {
            var props = dd.GenericProperties;
            foreach (Property p in props)
            {
                Guid currId = p.PropertyType.DataTypeDefinition.DataType.Id;
                if (LinkableMediaDataTypes.Contains(currId) && string.IsNullOrEmpty(p.Value.ToString()) == false)
                {
                    return p.Value.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// NOTE: New implementation of the legacy GetLinkValue. This is however a bit quirky as a media item can have multiple "Linkable DataTypes".
        /// Returns the value for a link in WYSIWYG mode, by default only media items that have a 
        /// DataTypeUploadField are linkable, however, a custom tree can be created which overrides
        /// this method, or another GUID for a custom data type can be added to the LinkableMediaDataTypes
        /// list on application startup.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal virtual string GetLinkValue(UmbracoEntity entity)
        {
            foreach (var property in entity.AdditionalData
                .Select(x => x.Value as UmbracoEntity.EntityProperty)
                .Where(x => x != null))
            {


                //required for backwards compatibility with v7 with changing the GUID -> alias
                var controlId = LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(property.PropertyEditorAlias, LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.ReturnNull);
                if (controlId != null)
                {
                    if (LinkableMediaDataTypes.Contains(controlId.Value) 
                        && string.IsNullOrEmpty((string)property.Value) == false)
                        
                        return property.Value.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// By default, any media type that is to be "linkable" in the WYSIWYG editor must contain
        /// a DataTypeUploadField data type which will ouput the value for the link, however, if 
        /// a developer wants the WYSIWYG editor to link to a custom media type, they will either have
        /// to create their own media tree and inherit from this one and override the GetLinkValue 
        /// or add another GUID to the LinkableMediaDataType list on application startup that matches
        /// the GUID of a custom data type. The order of property types on the media item definition will determine the output value.
        /// </summary>
        public static List<Guid> LinkableMediaDataTypes { get; protected set; }

        /// <summary>
        /// Returns true if we can use the EntityService to render the tree or revert to the original way 
        /// using normal documents
        /// </summary>
        /// <remarks>
        /// We determine this by:
        /// * If there are any subscribers to the events: BeforeTreeRender or AfterTreeRender - then we cannot run optimized
        /// </remarks>
        internal bool UseOptimizedRendering
        {
            get
            {
                if (HasEntityBasedEventSubscribers)
                {
                    return false;
                }

                return true;
            }
        }

    }
}
