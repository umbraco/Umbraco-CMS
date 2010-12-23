using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using umbraco.NodeFactory;

namespace umbraco.MacroEngines
{
    public class DynamicNode : DynamicObject
    {
        private INode n;
        public DynamicNode(INode n)
        {
            this.n = n;
        }

        public List<DynamicNode> GetChildrenAsList
        {
            get
            {
                List<DynamicNode> nodes = new List<DynamicNode>();
                foreach(Node nn in n.ChildrenAsList)
                    nodes.Add(new DynamicNode(nn));

                return nodes;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            var name = binder.Name;

            if (name == "ChildrenAsList" || name == "Children")
            {
                result = GetChildrenAsList;
                return true;
            } else if (name == "Parent")
            {
                result = new DynamicNode(n.Parent);
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
            } else
            {
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
                } catch
                {
                    result = "";
                    return false;
                }
            }


            return base.TryGetMember(binder, out result);
        }
    }
}
