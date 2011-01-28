using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using umbraco.interfaces;
using System.Collections;
using System.Reflection;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.property;

namespace umbraco.MacroEngines
{
    public class DynamicNode : DynamicObject, IEnumerable
    {
        private DynamicDictionary _properties;

        private readonly INode n;
        public DynamicNode(INode n)
        {
            if (n != null)
                this.n = n;
            else
                throw new ArgumentNullException("n", "A node must be provided to make a dynamic instance");
        }
        public DynamicNode(int NodeId)
        {
            this.n = new NodeFactory.Node(NodeId);
        }
        public DynamicNode()
        {
            //Empty constructor for a special case with Generic Methods
        }
        public void InitializeProperties(Dictionary<string, object> dict)
        {
            _properties = new DynamicDictionary(dict);
        }
        IEnumerable<INode> _children;
        public DynamicNode(IEnumerable<INode> children)
        {
            _children = new List<INode>(children);
        }
        public IEnumerator GetEnumerator()
        {
            return _children.Select(x => new DynamicNode(x)).GetEnumerator();
        }


        public DynamicNode GetChildrenAsList
        {
            get
            {
                return new DynamicNode(n.ChildrenAsList);
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name;

            try
            {
                //Property?
                result = _children.GetType().InvokeMember(binder.Name,
                                                  System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.GetProperty,
                                                  null,
                                                  _children,
                                                  args);
                return true;
            }
            catch (MissingMethodException)
            {
                try
                {
                    //Static or Instance Method?
                    result = _children.GetType().InvokeMember(binder.Name,
                                                  System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.Static |
                                                  System.Reflection.BindingFlags.InvokeMethod,
                                                  null,
                                                  _children,
                                                  args);
                    return true;
                }
                catch (MissingMethodException)
                {
                    try
                    {
                        //Extension method
                        Type tObject = _children.GetType();
                        Type t = tObject.GetGenericArguments()[0];
                        Type tIEnumerable = typeof(IEnumerable<>).MakeGenericType(t);

                        var methods = typeof(Enumerable)
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Where(m => m.Name == name && m.IsGenericMethod && m.GetParameters().Length == args.Length + 1)
                            .ToList();

                        if (methods.Count == 0)
                        {
                            throw new MissingMethodException();
                        }


                        MethodInfo enumerableMethod =
                            methods
                            .First()
                            .MakeGenericMethod(t);

                        var genericArgs = (new[] { _children }).Concat(args);

                        result = enumerableMethod.Invoke(null, genericArgs.ToArray());
                        if (result is IEnumerable<INode>)
                        {
                            result = new DynamicNode((IEnumerable<INode>)result);
                        }
                        if (result is INode)
                        {
                            result = new DynamicNode((INode)result);
                        }
                        return true;
                    }
                    catch (TargetInvocationException)
                    {
                        //We do this to enable error checking of Razor Syntax when a method e.g. ElementAt(2) is used.
                        //When the Script is tested, there's no INode which means no children which means ElementAt(2) is invalid (IndexOutOfRange)
                        //Instead, we are going to return an empty DynamicNode.
                        //This could be improved by checking return type of generic method above
                        //So we could support Generic Methods that return IEnumerable as well as Singular
                        result = new DynamicNode();
                        return true;
                    }
                    catch
                    {
                        result = null;
                        return false;
                    }

                }


            }
            catch
            {
                result = null;
                return false;
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

            if (n != null)
            {
                var data = n.GetProperty(name);
                // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
                if (data == null && name.Substring(0, 1).ToUpper() == name.Substring(0, 1))
                {
                    data = n.GetProperty(name.Substring(0, 1).ToLower() + name.Substring((1)));
                }

                if (data != null)
                {
                    result = data.Value;
                    //special casing for true/false properties
                    //so they can be used like this:
                    //if(@Model.shouldBeVisible)
                    //first check string value
                    if (data.Value == "1" || data.Value == "0")
                    {
                        //I'm aware this code is pretty heavy, which is why it's inside the string check
                        //I think it's important to check the property type because otherwise if you have a field which stores 0 or 1
                        //but isn't a True/False property then DynamicNode would return it as a boolean anyway
                        //The property type is not on IProperty (it's not stored in NodeFactory)
                        Document d = new Document(this.n.Id);
                        if (d != null)
                        {
                            // Get Property Alias from Macro
                            Property prop = d.getProperty(data.Alias);
                            if (prop != null)
                            {

                                PropertyType propType = prop.PropertyType;
                                if (propType != null)
                                {
                                    if (propType.ContentTypeId == 1047) //is there a better way than checking this?
                                    {

                                        bool parseResult;
                                        if (Boolean.TryParse(result.ToString().Replace("1", "true").Replace("0", "false"), out parseResult))
                                        {
                                            result = parseResult;
                                        }

                                    }
                                }
                            }
                        }
                    }

                    return true;
                }

                //check if the alias is that of a child type
                var typeChildren = n.ChildrenAsList
                    .Where(x => MakePluralName(x.NodeTypeAlias) == name || x.NodeTypeAlias == name);
                if (typeChildren.Any())
                {
                    result = new DynamicNode(typeChildren);
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


            result = null;
            return false;
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
        public DynamicNode AncestorOrSelf()
        {
            return AncestorOrSelf(node => node.Level == 1);
        }
        public DynamicNode AncestorOrSelf(Func<DynamicNode, bool> func)
        {
            var node = this;
            while (node != null)
            {
                if (func(node)) return node;
                DynamicNode parent = node.Parent;
                if (parent != null)
                {
                    if (this != parent)
                    {
                        node = parent;
                    }
                    else
                    {
                        return node;
                    }
                }
                else
                {
                    return node;
                }
            }

            return node;
        }

        public DynamicNode Parent
        {
            get
            {
                if (n == null)
                {
                    return null;
                }
                if (n.Parent != null)
                {
                    return new DynamicNode(n.Parent);
                }
                return this;
            }
        }
        public DynamicNode NodeById(int Id)
        {
            return new DynamicNode(Id);
        }
        public int Id
        {
            get { if (n == null) return 0; return n.Id; }
        }

        public int template
        {
            get { if (n == null) return 0; return n.template; }
        }

        public int SortOrder
        {
            get { if (n == null) return 0; return n.SortOrder; }
        }

        public string Name
        {
            get { if (n == null) return null; return n.Name; }
        }

        public string Url
        {
            get { if (n == null) return null; return n.Url; }
        }

        public string UrlName
        {
            get { if (n == null) return null; return n.UrlName; }
        }

        public string NodeTypeAlias
        {
            get { if (n == null) return null; return n.NodeTypeAlias; }
        }

        public string WriterName
        {
            get { if (n == null) return null; return n.WriterName; }
        }

        public string CreatorName
        {
            get { if (n == null) return null; return n.CreatorName; }
        }

        public int WriterID
        {
            get { if (n == null) return 0; return n.WriterID; }
        }

        public int CreatorID
        {
            get { if (n == null) return 0; return n.CreatorID; }
        }

        public string Path
        {
            get { return n.Path; }
        }

        public DateTime CreateDate
        {
            get { if (n == null) return DateTime.MinValue; return n.CreateDate; }
        }

        public DateTime UpdateDate
        {
            get { if (n == null) return DateTime.MinValue; return n.UpdateDate; }
        }

        public Guid Version
        {
            get { if (n == null) return Guid.Empty; return n.Version; }
        }

        public string NiceUrl
        {
            get { if (n == null) return null; return n.NiceUrl; }
        }

        public int Level
        {
            get { if (n == null) return 0; return n.Level; }
        }

        public List<IProperty> PropertiesAsList
        {
            get { if (n == null) return null; return n.PropertiesAsList; }
        }

        public List<INode> ChildrenAsList
        {
            get { if (n == null) return null; return n.ChildrenAsList; }
        }

        public IProperty GetProperty(string alias)
        {
            if (n == null) return null;
            return n.GetProperty(alias);
        }

        public System.Data.DataTable ChildrenAsTable()
        {
            if (n == null) return null;
            return n.ChildrenAsTable();
        }

        public System.Data.DataTable ChildrenAsTable(string nodeTypeAliasFilter)
        {
            if (n == null) return null;
            return n.ChildrenAsTable(nodeTypeAliasFilter);
        }

        public DynamicDictionary Parameters
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }
    }
}
