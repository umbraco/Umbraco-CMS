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
            if (n != null)
                this.n = n;
            else
                throw new ArgumentNullException("n", "A node must be provided to make a dynamic instance");
        }

        public IEnumerable<DynamicNode> GetChildrenAsList
        {
            get
            {
                return from nn in n.ChildrenAsList
                       select new DynamicNode(nn);
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

            //check if the alias is that of a child type
            var typeChildren = n.ChildrenAsList
                .Where(x => MakePluralName(x.NodeTypeAlias) == name || x.NodeTypeAlias == name);
            if (typeChildren.Any())
            {
                result = typeChildren
                    .Select(x => new DynamicNode(x));
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

        //this is from SqlMetal and just makes it a bit of fun to allow pluralisation
        private static string MakePluralName(string name)
        {
            if ((name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase)) || (name.EndsWith("ss", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase)))
            {
                name = name + "es";
                return name;
            }
            if ((name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && (name.Length > 1)) && !IsVowel(name[name.Length - 2]))
            {
                name = name.Remove(name.Length - 1, 1);
                name = name + "ies";
                return name;
            }
            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name = name + "s";
            }
            return name;
        }

        private static bool IsVowel(char c)
        {
            switch (c)
            {
                case 'O':
                case 'U':
                case 'Y':
                case 'A':
                case 'E':
                case 'I':
                case 'o':
                case 'u':
                case 'y':
                case 'a':
                case 'e':
                case 'i':
                    return true;
            }
            return false;
        }

        public DynamicNode AncestorOrSelf(Func<DynamicNode, bool> func)
        {
            var node = this;
            while (node != null)
            {
                if (func(node)) return node;
                node = node.Parent;
            }
        }

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

        public IProperty GetProperty(string alias)
        {
            return n.GetProperty(alias);
        }

        public System.Data.DataTable ChildrenAsTable()
        {
            return n.ChildrenAsTable();
        }

        public System.Data.DataTable ChildrenAsTable(string nodeTypeAliasFilter)
        {
            return n.ChildrenAsTable(nodeTypeAliasFilter);
        }
    }
}
