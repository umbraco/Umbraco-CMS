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
    public class DynamicNode : DynamicObject
    {
        private DynamicDictionary _properties;
        private Dictionary<string, Guid> _propertyTypeCache = new Dictionary<string, Guid>();

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
        public DynamicNode(string NodeId)
        {
            int iNodeId = 0;
            if (int.TryParse(NodeId, out iNodeId))
            {
                this.n = new NodeFactory.Node(iNodeId);
            }
        }
        public DynamicNode(object NodeId)
        {
            int iNodeId = 0;
            if (int.TryParse(string.Format("{0}", NodeId), out iNodeId))
            {
                this.n = new NodeFactory.Node(iNodeId);
            }
        }
        public DynamicNode()
        {
            //Empty constructor for a special case with Generic Methods
        }
        public void InitializeProperties(Dictionary<string, object> dict)
        {
            _properties = new DynamicDictionary(dict);
        }

        public DynamicNodeList GetChildrenAsList
        {
            get
            {
                return new DynamicNodeList(n.ChildrenAsList);
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
                    //if (data.Value == "1" || data.Value == "0")
                    //{
                    //I'm aware this code is pretty heavy
                    //but I cant see another way to do it
                    //I was originally checking the string value of data.Value == "0" || data.value == "1" before the property
                    //type check, but this failed when a new True/False property was added to a node after the content was created
                    //sometimes the data.Value was "" for the boolean. Not sure under what case this applies but needless to say, 
                    //the razor template would crash because now an empty string was returned instead of (bool)false.
                    //The type gets checked and cached, which is not going to be very good for performance, but i'm not sure how much
                    //of a difference it will make.
                    //the easiest fix for this (if you want to keep the nice boolean casting stuff) is to get the type into IProperty
                    //I think it's important to check the property type because otherwise if you have a field which stores 0 or 1
                    //but isn't a True/False property then DynamicNode would return it as a boolean anyway
                    //The property type is not on IProperty (it's not stored in NodeFactory)
                    //first check the cache
                    if (_propertyTypeCache != null && _propertyTypeCache.ContainsKey(name))
                    {
                        return ConvertPropertyValueByDataType(ref result, name);
                    }
                    //find the type of the property
                    //heavy :(
                    Document d = new Document(this.n.Id);
                    if (d != null)
                    {
                        // Get Property Alias from Macro
                        Property prop = d.getProperty(data.Alias);
                        if (prop != null)
                        {
                            //get type from property
                            PropertyType propType = prop.PropertyType;
                            if (propType != null)
                            {
                                //got type, add to cache
                                _propertyTypeCache.Add(name, propType.DataTypeDefinition.DataType.Id);
                                return ConvertPropertyValueByDataType(ref result, name);
                            }
                        }
                    }
                    //}

                    return true;
                }

                //check if the alias is that of a child type
                var typeChildren = n.ChildrenAsList
                    .Where(x => MakePluralName(x.NodeTypeAlias) == name || x.NodeTypeAlias == name);
                if (typeChildren.Any())
                {
                    result = new DynamicNodeList(typeChildren);
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

        private bool ConvertPropertyValueByDataType(ref object result, string name)
        {
            //the resulting property is a string, but to support some of the nice linq stuff in .Where
            //we should really check some more types
            umbraco.editorControls.yesno.YesNoDataType yesnoType = new editorControls.yesno.YesNoDataType();

            //boolean
            if (_propertyTypeCache[name] == yesnoType.Id)
            {
                bool parseResult;
                if (result.ToString() == "") result = "0";
                if (Boolean.TryParse(result.ToString().Replace("1", "true").Replace("0", "false"), out parseResult))
                {
                    result = parseResult;
                }
                return true;
            }

            //integer
            int iResult = 0;
            if (int.TryParse(string.Format("{0}", result), out iResult))
            {
                result = iResult;
                return true;
            }

            //decimal
            decimal dResult = 0;
            if (decimal.TryParse(string.Format("{0}", result), out dResult))
            {
                result = dResult;
                return true;
            }

            if (string.Equals("true", string.Format("{0}", result), StringComparison.CurrentCultureIgnoreCase))
            {
                result = true;
                return true;
            }
            if (string.Equals("false", string.Format("{0}", result), StringComparison.CurrentCultureIgnoreCase))
            {
                result = false;
                return false;
            }

            return false;
        }

        public DynamicMedia Media(string propertyAlias)
        {
            if (n != null)
            {
                IProperty prop = n.GetProperty(propertyAlias);
                if (prop != null)
                {
                    int mediaNodeId;
                    if (int.TryParse(prop.Value, out mediaNodeId))
                    {
                        return new DynamicMedia(mediaNodeId);
                    }
                }
                return null;
            }
            return null;
        }
        public string Media(string propertyAlias, string mediaPropertyAlias)
        {
            if (n != null)
            {
                IProperty prop = n.GetProperty(propertyAlias);
                if (prop == null && propertyAlias.Substring(0, 1).ToUpper() == propertyAlias.Substring(0, 1))
                {
                    prop = n.GetProperty(propertyAlias.Substring(0, 1).ToLower() + propertyAlias.Substring((1)));
                }
                if (prop != null)
                {
                    int mediaNodeId;
                    if (int.TryParse(prop.Value, out mediaNodeId))
                    {
                        umbraco.cms.businesslogic.media.Media media = new cms.businesslogic.media.Media(mediaNodeId);
                        if (media != null)
                        {
                            Property mprop = media.getProperty(mediaPropertyAlias);
                            // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
                            if (prop == null && mediaPropertyAlias.Substring(0, 1).ToUpper() == mediaPropertyAlias.Substring(0, 1))
                            {
                                mprop = media.getProperty(mediaPropertyAlias.Substring(0, 1).ToLower() + mediaPropertyAlias.Substring((1)));
                            }
                            if (mprop != null)
                            {
                                return string.Format("{0}", mprop.Value);
                            }
                        }
                    }
                }
            }
            return null;
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
        public DynamicNode NodeById(string Id)
        {
            return new DynamicNode(Id);
        }
        public DynamicNode NodeById(object Id)
        {
            return new DynamicNode(Id);
        }
        public DynamicMedia MediaById(int Id)
        {
            return new DynamicMedia(Id);
        }
        public DynamicMedia MediaById(string Id)
        {
            return new DynamicMedia(Id);
        }
        public DynamicMedia MediaById(object Id)
        {
            return new DynamicMedia(Id);
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
        public bool Visible
        {
            get;
            set;
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
