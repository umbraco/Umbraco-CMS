using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.property;
using umbraco.presentation.nodeFactory;
using System.Data;

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
            NodeFactory.Node baseNode = new NodeFactory.Node(Id);
            //todo: trace this with media
            if (baseNode == null)
            {
                this.media = ExamineBackedMedia.GetUmbracoMedia(Id);
                this.Type = DynamicBackingItemType.Media;
            }
            else
            {
                this.content = baseNode;
                this.Type = DynamicBackingItemType.Content;
            }
        }

        public DynamicBackingItem(CMSNode node)
        {
            this.content = (INode)node;
            this.Type = DynamicBackingItemType.Content;
        }

        public bool IsNull()
        {
            return ((Type == DynamicBackingItemType.Content && content == null) || media == null);
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
                    var children = media.ChildrenAsList;
                    if (children != null)
                    {
                        return children.ToList().ConvertAll(m => new DynamicBackingItem(m));
                    }
                }
                return new List<DynamicBackingItem>();
            }
        }

        public IProperty GetProperty(string alias)
        {
            if (IsNull()) return null;
            return Type == DynamicBackingItemType.Content ? new PropertyResult(content.GetProperty(alias)) : new PropertyResult(media.GetProperty(alias));
        }
        public IProperty GetProperty(string alias, out bool propertyExists)
        {
            if (IsNull())
            {
                propertyExists = false;
                return null;
            }
            if (Type == DynamicBackingItemType.Content)
            {
                return content.GetProperty(alias, out propertyExists);
            }
            else
            {
                return media.GetProperty(alias, out propertyExists);
            }
        }

        public IProperty GetProperty(string alias, bool recursive)
        {
            if (!recursive) return GetProperty(alias);
            if (IsNull()) return null;
            DynamicBackingItem context = this;
            IProperty prop = this.GetProperty(alias);
            while (prop == null)
            {
                context = context.Parent;
                prop = context.GetProperty(alias);
                if (context == null) break;
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
                return Type == DynamicBackingItemType.Content ?
                    new DynamicBackingItem(content.Parent) :
                    new DynamicBackingItem(media.Parent);
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
        public Guid Version
        {
            get { if (IsNull()) return Guid.Empty; return Type == DynamicBackingItemType.Content ? content.Version : media.Version; }
        }

        public string Url
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.Url : null; }
        }

        public string NiceUrl
        {
            get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? content.NiceUrl : null; }
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
                    List<INode> children = media.ChildrenAsList;
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