using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.property;
using System.Data;
using Umbraco.Core;
using umbraco.MacroEngines.Library;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.umbraco.presentation;

namespace umbraco.MacroEngines
{
    public class DynamicBackingItem
    {
        internal INode content;
        internal ExamineBackedMedia media;
        public DynamicBackingItemType Type;

        public DynamicBackingItem(INode iNode)
        {
            this.content = iNode;
            this.Type = DynamicBackingItemType.Content;
        }
        public DynamicBackingItem(ExamineBackedMedia media)
        {
            this.media = media;
            this.Type = DynamicBackingItemType.Media;
        }
        public DynamicBackingItem(int Id)
        {
            if (Id == -1)
            {
                // passing in -1 needs to return a real node, the "root" node, which has no 
                // properties (defaults apply) but can be used to access descendants, children, etc.
                
                this.content = new NodeFactory.Node(Id);
                return;
            }

            var n = LegacyNodeHelper.ConvertToNode(UmbracoContext.Current.ContentCache.GetById(Id));
           
            this.content = n;
            this.Type = DynamicBackingItemType.Content;
            if (n.Id == 0 && Id != 0)
            {
                this.media = ExamineBackedMedia.GetUmbracoMedia(Id);
                this.Type = DynamicBackingItemType.Media;
                if (this.media == null)
                {
                    this.Type = DynamicBackingItemType.Content;
                }
                return;
            }

        }
        public DynamicBackingItem(int Id, DynamicBackingItemType Type)
        {
            if (Type == DynamicBackingItemType.Media)
            {
                this.media = ExamineBackedMedia.GetUmbracoMedia(Id);
                this.Type = Type;
            }
            else
            {
                if (Id == -1)
                {
                    // passing in -1 needs to return a real node, the "root" node, which has no 
                    // properties (defaults apply) but can be used to access descendants, children, etc.

                    this.content = new NodeFactory.Node(Id);
                }
                else
                {
                    this.content = LegacyNodeHelper.ConvertToNode(UmbracoContext.Current.ContentCache.GetById(Id));    
                }
                
                this.Type = Type;
            }
        }

        public DynamicBackingItem(CMSNode node)
        {
            this.content = (INode)node;
            this.Type = DynamicBackingItemType.Content;
        }

        public bool IsNull()
        {
            return (content == null && media == null);
        }
        public List<DynamicBackingItem> ChildrenAsList
        {
            get
            {
                if (IsNull()) return null;
                if (Type == DynamicBackingItemType.Content)
                {
                    var children = content.ChildrenAsList;
                    if (children != null)
                    {
                        return children.ConvertAll(c => new DynamicBackingItem(c));
                    }
                }
                else
                {
                    var children = media.ChildrenAsList.Value;
                    if (children != null)
                    {
                        return children.ConvertAll(m => new DynamicBackingItem(m));
                    }
                }
                return new List<DynamicBackingItem>();
            }
        }

        public PropertyResult GetProperty(string alias)
        {
            if (IsNull()) return null;
            if (Type == DynamicBackingItemType.Content)
            {
                return GetPropertyInternal(alias, content);
            }
            else
            {
                return GetPropertyInternal(alias, media);
            }
        }

        private PropertyResult GetPropertyInternal(string alias, INode content)
        {
            bool propertyExists = false;
            var prop = content.GetProperty(alias, out propertyExists);
            if (prop != null)
            {
                return new PropertyResult(prop) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
            }
            else
            {
                if (alias.Substring(0, 1).ToUpper() == alias.Substring(0, 1) && !propertyExists)
                {
                    prop = content.GetProperty(alias.Substring(0, 1).ToLower() + alias.Substring((1)), out propertyExists);
                    if (prop != null)
                    {
                        return new PropertyResult(prop) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
                    }
                    else
                    {
                        //reflect
                        object result = null;
                        try
                        {
                            result = content.GetType().InvokeMember(alias,
                                                      System.Reflection.BindingFlags.GetProperty |
                                                      System.Reflection.BindingFlags.Instance |
                                                      System.Reflection.BindingFlags.Public,
                                                      null,
                                                      content,
                                                      null);
                        }
                        catch (MissingMethodException)
                        {

                        }
                        if (result != null)
                        {
                            return new PropertyResult(alias, string.Format("{0}", result)) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
                        }
                    }
                }
            }
            return null;
        }
        private PropertyResult GetPropertyInternal(string alias, ExamineBackedMedia content)
        {
            bool propertyExists = false;
            var prop = content.GetProperty(alias, out propertyExists);
            if (prop != null)
            {
                return new PropertyResult(prop) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
            }
            else
            {
                if (alias.Substring(0, 1).ToUpper() == alias.Substring(0, 1) && !propertyExists)
                {
                    prop = content.GetProperty(alias.Substring(0, 1).ToLower() + alias.Substring((1)), out propertyExists);
                    if (prop != null)
                    {
                        return new PropertyResult(prop) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
                    }
                    else
                    {
                        object result = null;
                        try
                        {
                            result = content.GetType().InvokeMember(alias,
                                                      System.Reflection.BindingFlags.GetProperty |
                                                      System.Reflection.BindingFlags.Instance |
                                                      System.Reflection.BindingFlags.Public,
                                                      null,
                                                      content,
                                                      null);
                        }
                        catch (MissingMethodException)
                        {
                        }
                        if (result != null)
                        {
                            return new PropertyResult(alias, string.Format("{0}", result)) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
                        }
                    }
                }
            }
            return null;
        }
        public PropertyResult GetProperty(string alias, out bool propertyExists)
        {
            if (IsNull())
            {
                propertyExists = false;
                return null;
            }
            PropertyResult property = null;
            IProperty innerProperty = null;
            if (Type == DynamicBackingItemType.Content)
            {
                innerProperty = content.GetProperty(alias, out propertyExists);
                if (innerProperty != null)
                {
                    property = new PropertyResult(innerProperty);
                    property.ContextAlias = content.NodeTypeAlias;
                    property.ContextId = content.Id;
                }
            }
            else
            {
                string[] internalProperties = new string[] {
                    "id", "nodeName", "updateDate", "writerName", "path", "nodeTypeAlias",
                    "parentID", "__NodeId", "__IndexType", "__Path", "__NodeTypeAlias", 
                    "__nodeName", Constants.Conventions.Media.Bytes, Constants.Conventions.Media.Extension, Constants.Conventions.Media.File, Constants.Conventions.Media.Width,
                    Constants.Conventions.Media.Height
                };
                if (media.WasLoadedFromExamine && !internalProperties.Contains(alias) && !media.Values.ContainsKey(alias))
                {
                    //examine doesn't load custom properties
                    innerProperty = media.LoadCustomPropertyNotFoundInExamine(alias, out propertyExists);
                    if (innerProperty != null)
                    {
                        property = new PropertyResult(innerProperty);
                        property.ContextAlias = media.NodeTypeAlias;
                        property.ContextId = media.Id;
                    }
                }
                else
                {
                    innerProperty = media.GetProperty(alias, out propertyExists);
                    if (innerProperty != null)
                    {
                        property = new PropertyResult(innerProperty);
                        property.ContextAlias = media.NodeTypeAlias;
                        property.ContextId = media.Id;
                    }
                }
            }
            return property;
        }

        public PropertyResult GetProperty(string alias, bool recursive)
        {
            bool propertyExists = false;
            return GetProperty(alias, recursive, out propertyExists);
        }
        public PropertyResult GetProperty(string alias, bool recursive, out bool propertyExists)
        {
            if (!recursive)
            {
                return GetProperty(alias, out propertyExists);
            }
            if (IsNull())
            {
                propertyExists = false;
                return null;
            }
            DynamicBackingItem context = this;
            PropertyResult prop = this.GetProperty(alias, out propertyExists);
            while (prop == null || string.IsNullOrEmpty(prop.Value))
            {
                context = context.Parent;
                if (context == null) break;
                prop = context.GetProperty(alias, out propertyExists);
            }
            if (prop != null)
            {
                return prop;
            }
            return null;
        }
        public string GetPropertyValue(string alias)
        {
            var prop = GetProperty(alias);
            if (prop != null) return prop.Value;
            return null;
        }
        public string GetPropertyValue(string alias, bool recursive)
        {
            var prop = GetProperty(alias, recursive);
            if (prop != null) return prop.Value;
            return null;
        }
        public List<IProperty> PropertiesAsList
        {
            get
            {
                if (IsNull()) return null;
                if (Type == DynamicBackingItemType.Content)
                {
                    return content.PropertiesAsList;
                }
                else
                {
                    return media.PropertiesAsList;
                }
            }
        }
        public DataTable ChildrenAsTable()
        {
            if (IsNull()) return null;
            if (Type == DynamicBackingItemType.Content)
            {
                return content.ChildrenAsTable();
            }
            else
            {
                //sorry
                return null;
            }

        }
        public DataTable ChildrenAsTable(string nodeTypeAlias)
        {
            if (IsNull()) return null;
            if (Type == DynamicBackingItemType.Content)
            {
                return content.ChildrenAsTable(nodeTypeAlias);
            }
            else
            {
                //sorry
                return null;
            }

        }
        public int Level
        {
            get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? content.Level : media.Level; }
        }


        public int Id
        {
            get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? content.Id : media.Id; }
        }

        public string NodeTypeAlias
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.NodeTypeAlias : media.NodeTypeAlias; }
        }

        public DynamicBackingItem Parent
        {
            get
            {
                if (IsNull()) return null;
                if (Type == DynamicBackingItemType.Content)
                {
                    var parent = content.Parent;
                    if (parent != null)
                    {

                        return new DynamicBackingItem(parent);
                    }

                }
                else
                {
                    var parent = media.Parent;
                    if (parent != null && parent.Value != null)
                    {
                        return new DynamicBackingItem(parent.Value);
                    }
                }
                return null;
            }
        }
        public DateTime CreateDate
        {
            get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? content.CreateDate : media.CreateDate; }
        }
        public DateTime UpdateDate
        {
            get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? content.UpdateDate : media.UpdateDate; }
        }

        public string WriterName
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.WriterName : null; }
        }

        public string Name
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.Name : media.Name; }
        }
        public string nodeName
        {
            get { return Name; }
        }
        public string pageName
        {
            get { return Name; }
        }
        public Guid Version
        {
            get { if (IsNull()) return Guid.Empty; return Type == DynamicBackingItemType.Content ? content.Version : media.Version; }
        }

        public string Url
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.Url : media.Url; }
        }

        public string NiceUrl
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.NiceUrl : media.NiceUrl; }
        }

        public string UrlName
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.UrlName : null; }
        }

        public int template
        {
            get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? content.template : 0; }
        }

        public int SortOrder
        {
            get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? content.SortOrder : media.SortOrder; }
        }


        public string CreatorName
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.CreatorName : media.CreatorName; }
        }

        public int WriterID
        {
            get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? content.WriterID : 0; }
        }

        public int CreatorID
        {
            get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? content.CreatorID : media.CreatorID; }
        }

        public string Path
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.Path : media.Path; }
        }

        public List<DynamicBackingItem> GetChildrenAsList
        {
            get
            {
                if (Type == DynamicBackingItemType.Content)
                {
                    List<INode> children = content.ChildrenAsList;
                    //testing
                    if (children.Count == 0 && content.Id == 0)
                    {
                        return new List<DynamicBackingItem>(new DynamicBackingItem[] { this });
                    }
                    return children.ConvertAll(n => new DynamicBackingItem(n));
                }
                else
                {
                    List<ExamineBackedMedia> children = media.ChildrenAsList.Value;
                    //testing
                    if (children.Count == 0 && content.Id == 0)
                    {
                        return new List<DynamicBackingItem>(new DynamicBackingItem[] { this });
                    }
                    return children.ConvertAll(n => new DynamicBackingItem(n));
                }
            }
        }


    }
}