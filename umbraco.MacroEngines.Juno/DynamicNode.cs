using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public class DynamicNode : DynamicObject
    {
        private readonly INode n;
        public DynamicNode(INode n)
        {
            this.n = n;
        }

        public List<DynamicNode> GetChildrenAsList
        {
            get
            {
                return (from nn in n.ChildrenAsList 
                        select new DynamicNode(nn)).ToList();
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            var name = binder.Name;

            if (name == "ChildrenAsList" || name == "Children")
            {
                result = GetChildrenAsList;
                return true;
            } 

            var data = n.GetProperty(name);
            // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
            if (data == null && name.Substring(0,1).ToUpper() == name.Substring(0,1))
            {
                data = n.GetProperty(name.Substring(0, 1).ToLower() + name.Substring((1)));
            }

            if (data != null)
            {
                result = data.Value;
                return true;
            }
            try
            {
                result = n.GetType().InvokeMember(binder.Name,
                                                  System.Reflection.BindingFlags.GetProperty |
                                                  System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic,
                                                  null,
                                                  n,
                                                  null);
                return true;
            } 
            catch
            {
                result = null;
                return false;
            }
        }

        #region INode Members

        public DynamicNode Parent
        {
            get { return new DynamicNode(n.Parent); }
        }

        public int Id
        {
            get { return n.Id; }
        }

        public int template
        {
            get { return n.template; }
        }

        public int SortOrder
        {
            get { return n.SortOrder; }
        }

        public string Name
        {
            get { return n.Name; }
        }

        public string Url
        {
            get { return n.Url; }
        }

        public string UrlName
        {
            get { return n.UrlName; }
        }

        public string NodeTypeAlias
        {
            get { return n.NodeTypeAlias; }
        }

        public string WriterName
        {
            get { return n.WriterName; }
        }

        public string CreatorName
        {
            get { return n.CreatorName; }
        }

        public int WriterID
        {
            get { return n.WriterID; }
        }

        public int CreatorID
        {
            get { return n.CreatorID; }
        }

        public string Path
        {
            get { return n.Path; }
        }

        public DateTime CreateDate
        {
            get { return n.CreateDate; }
        }

        public DateTime UpdateDate
        {
            get { return n.UpdateDate; }
        }

        public Guid Version
        {
            get { return n.Version; }
        }

        public string NiceUrl
        {
            get { return n.NiceUrl; }
        }

        public int Level
        {
            get { return n.Level; }
        }

        public List<IProperty> PropertiesAsList
        {
            get { return n.PropertiesAsList; }
        }

        public List<INode> ChildrenAsList
        {
            get { return n.ChildrenAsList; }
        }

        public IProperty GetProperty(string Alias)
        {
            return n.GetProperty(Alias);
        }

        public System.Data.DataTable ChildrenAsTable()
        {
            return n.ChildrenAsTable();
        }

        public System.Data.DataTable ChildrenAsTable(string nodeTypeAliasFilter)
        {
            return n.ChildrenAsTable(nodeTypeAliasFilter);
        }

        #endregion
    }
}
