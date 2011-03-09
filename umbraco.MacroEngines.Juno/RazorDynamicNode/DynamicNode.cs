using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using umbraco.interfaces;
using System.Collections;
using System.Reflection;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.property;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.cms.businesslogic;
using System.Xml;
using System.Xml.Linq;


namespace umbraco.MacroEngines
{
    public class DynamicNode : DynamicObject
    {
        #region consts
        // these are private readonlys as const can't be Guids
        private readonly Guid DATATYPE_YESNO_GUID = new Guid("38b352c1-e9f8-4fd8-9324-9a2eab06d97a");
        private readonly Guid DATATYPE_TINYMCE_GUID = new Guid("5e9b75ae-face-41c8-b47e-5f4b0fd82f83");
        #endregion

        internal DynamicNodeList ownerList;

        internal readonly INode n;
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

        public DynamicNode Up()
        {
            return DynamicNodeWalker.Up(this);
        }
        public DynamicNode Up(int number)
        {
            return DynamicNodeWalker.Up(this, number);
        }
        public DynamicNode Down()
        {
            return DynamicNodeWalker.Down(this);
        }
        public DynamicNode Down(int number)
        {
            return DynamicNodeWalker.Down(this, number);
        }
        public DynamicNode Next()
        {
            return DynamicNodeWalker.Next(this);
        }
        public DynamicNode Next(int number)
        {
            return DynamicNodeWalker.Next(this, number);
        }
        public DynamicNode Previous()
        {
            return DynamicNodeWalker.Previous(this);
        }
        public DynamicNode Previous(int number)
        {
            return DynamicNodeWalker.Previous(this, number);
        }

        public DynamicNodeList GetChildrenAsList
        {
            get
            {
                List<INode> children = n.ChildrenAsList;
                //testing
                if (children.Count == 0 && n.Id == 0)
                {
                    return new DynamicNodeList(new List<INode> { this.n });
                }
                return new DynamicNodeList(n.ChildrenAsList);
            }
        }
        public DynamicNodeList XPath(string xPath)
        {
            //if this DN was initialized with an underlying NodeFactory.Node
            if (n != null)
            {
                //get the underlying xml content
                XmlDocument doc = umbraco.content.Instance.XmlContent;
                if (doc != null)
                {
                    //get n as a XmlNode (to be used as the context point for the xpath)
                    //rather than just applying the xPath to the root node, this lets us use .. etc from the DynamicNode point


                    //in test mode, n.Id is 0, let this always succeed
                    if (n.Id == 0)
                    {
                        List<DynamicNode> selfList = new List<DynamicNode>() { this };
                        return new DynamicNodeList(selfList);
                    }
                    XmlNode node = doc.SelectSingleNode(string.Format("//*[@id='{0}']", n.Id));
                    if (node != null)
                    {
                        //got the current node (within the XmlContent instance)
                        XmlNodeList nodes = node.SelectNodes(xPath);
                        if (nodes.Count > 0)
                        {
                            //we got some resulting nodes
                            List<NodeFactory.Node> nodeFactoryNodeList = new List<NodeFactory.Node>();
                            //attempt to convert each node in the set to a NodeFactory.Node
                            foreach (XmlNode nodeXmlNode in nodes)
                            {
                                try
                                {
                                    nodeFactoryNodeList.Add(new NodeFactory.Node(nodeXmlNode));
                                }
                                catch (Exception) { } //swallow the exceptions - the returned nodes might not be full nodes, e.g. property
                            }
                            //Wanted to do this, but because we return DynamicNodeList here, the only
                            //common parent class is DynamicObject
                            //maybe some future refactoring will solve this?
                            //if (nodeFactoryNodeList.Count == 0)
                            //{
                            //    //if the xpath resulted in a node set, but none of them could be converted to NodeFactory.Node
                            //    XElement xElement = XElement.Parse(node.OuterXml);
                            //    //return 
                            //    return new DynamicXml(xElement);
                            //}
                            //convert the NodeFactory nodelist to IEnumerable<DynamicNode> and return it as a DynamicNodeList
                            return new DynamicNodeList(nodeFactoryNodeList.ConvertAll(nfNode => new DynamicNode(nfNode)));
                        }
                        else
                        {
                            throw new NullReferenceException("XPath returned no nodes");
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("Couldn't locate the DynamicNode within the XmlContent");
                    }
                }
                else
                {
                    throw new NullReferenceException("umbraco.content.Instance.XmlContent is null");
                }
            }
            else
            {
                throw new NullReferenceException("DynamicNode wasn't initialized with an underlying NodeFactory.Node");
            }
        }
        public bool HasProperty(string name)
        {
            if (n != null)
            {
                try
                {
                    IProperty prop = n.GetProperty(name);
                    if (prop == null)
                    {
                        // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
                        if (prop == null && name.Substring(0, 1).ToUpper() == name.Substring(0, 1))
                        {
                            prop = n.GetProperty(name.Substring(0, 1).ToLower() + name.Substring((1)));
                        }
                    }
                    return (prop != null);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            var name = binder.Name;
            result = null; //this will never be returned

            if (name == "ChildrenAsList" || name == "Children")
            {
                result = GetChildrenAsList;
                return true;
            }
            bool propertyExists = false;
            if (n != null)
            {
                var data = n.GetProperty(name, out propertyExists);
                // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
                if (data == null && name.Substring(0, 1).ToUpper() == name.Substring(0, 1) && !propertyExists)
                {
                    data = n.GetProperty(name.Substring(0, 1).ToLower() + name.Substring((1)), out propertyExists);
                }

                if (data != null)
                {
                    result = data.Value;
                    //special casing for true/false properties
                    //int/decimal are handled by ConvertPropertyValueByDataType
                    //fallback is stringT

                    Guid dataType = ContentType.GetDataType(n.NodeTypeAlias, data.Alias);

                    //convert the string value to a known type
                    return ConvertPropertyValueByDataType(ref result, name, dataType);

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
                    //result = null;
                    //return false;
                }
            }

            //if property access, type lookup and member invoke all failed
            //at this point, we're going to return null
            //instead, we return a DynamicNull - see comments in that file
            //this will let things like Model.ChildItem work and return nothing instead of crashing
            if (!propertyExists && result == null)
            {
                //.Where explictly checks for this type
                //and will make it false
                //which means backwards equality (&& property != true) will pass
                //forwwards equality (&& property or && property == true) will fail
                result = new DynamicNull();
                return true;
            }
            return true;
        }

        private bool ConvertPropertyValueByDataType(ref object result, string name, Guid dataType)
        {
            //the resulting property is a string, but to support some of the nice linq stuff in .Where
            //we should really check some more types

            //boolean
            if (dataType == DATATYPE_YESNO_GUID)
            {
                bool parseResult;
                if (string.Format("{0}", result) == "") result = "0";
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

            //date
            DateTime dtResult = DateTime.MinValue;
            if (DateTime.TryParse(string.Format("{0}", result), out dtResult))
            {
                result = dtResult;
                return true;
            }

            // Rich text editor (return IHtmlString so devs doesn't need to decode html
            if (dataType == DATATYPE_TINYMCE_GUID)
            {
                result = new HtmlString(result.ToString());
            }


            if (string.Equals("true", string.Format("{0}", result), StringComparison.CurrentCultureIgnoreCase))
            {
                result = true;
                return true;
            }
            if (string.Equals("false", string.Format("{0}", result), StringComparison.CurrentCultureIgnoreCase))
            {
                result = false;
                return true;
            }

            if (result != null)
            {
                string sResult = string.Format("{0}", result).Trim();
                //a really rough check to see if this may be valid xml
                if (sResult.StartsWith("<") && sResult.EndsWith(">") && sResult.Contains("/"))
                {
                    try
                    {
                        XElement e = XElement.Parse(sResult, LoadOptions.None);
                        if (e != null)
                        {
                            //check that the document element is not one of the disallowed elements
                            //allows RTE to still return as html if it's valid xhtml
                            string documentElement = e.Name.LocalName;
                            if (!UmbracoSettings.NotDynamicXmlDocumentElements.Any(tag =>
                                string.Equals(tag, documentElement, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                result = new DynamicXml(e);
                                return true;
                            }
                            else
                            {
                                //we will just return this as a string
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //we will just return this as a string
                        return true;
                    }

                }
            }

            return true;
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
        public bool IsProtected
        {
            get
            {
                if (n != null)
                {
                    return umbraco.library.IsProtected(n.Id, n.Path);
                }
                return false;
            }
        }
        public bool HasAccess
        {
            get
            {
                if (n != null)
                {
                    return umbraco.library.HasAccess(n.Id, n.Path);
                }
                return true;
            }
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
        public DynamicNode AncestorOrSelf(int level)
        {
            return AncestorOrSelf(node => node.Level == level);
        }
        public DynamicNode AncestorOrSelf(string nodeTypeAlias)
        {
            return AncestorOrSelf(node => node.NodeTypeAlias == nodeTypeAlias);
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
        public DynamicNodeList AncestorsOrSelf(Func<DynamicNode, bool> func)
        {
            List<DynamicNode> ancestorList = new List<DynamicNode>();
            var node = this;
            ancestorList.Add(node);
            while (node != null)
            {
                if (node.Level == 1) break;
                DynamicNode parent = node.Parent;
                if (parent != null)
                {
                    if (this != parent)
                    {
                        node = parent;
                        if (func(node))
                        {
                            ancestorList.Add(node);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            ancestorList.Reverse();
            return new DynamicNodeList(ancestorList);
        }
        public DynamicNodeList AncestorsOrSelf()
        {
            return AncestorsOrSelf(n => true);
        }
        public DynamicNodeList AncestorsOrSelf(string nodeTypeAlias)
        {
            return AncestorsOrSelf(n => n.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicNodeList AncestorsOrSelf(int level)
        {
            return AncestorsOrSelf(n => n.Level <= level);
        }
        public DynamicNodeList Descendants(string nodeTypeAlias)
        {
            return Descendants(p => p.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicNodeList Descendants(int level)
        {
            return Descendants(p => p.Level >= level);
        }
        public DynamicNodeList Descendants()
        {
            return Descendants(n => true);
        }
        public DynamicNodeList Descendants(Func<INode, bool> func)
        {
            var flattenedNodes = this.n.ChildrenAsList.Map(func, (INode n) => { return n.ChildrenAsList; });
            return new DynamicNodeList(flattenedNodes.ToList().ConvertAll(iNode => new DynamicNode(iNode)));
        }
        public DynamicNodeList DescendantsOrSelf(int level)
        {
            return DescendantsOrSelf(p => p.Level >= level);
        }
        public DynamicNodeList DescendantsOrSelf(string nodeTypeAlias)
        {
            return DescendantsOrSelf(p => p.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicNodeList DescendantsOrSelf()
        {
            return DescendantsOrSelf(p => true);
        }
        public DynamicNodeList DescendantsOrSelf(Func<INode, bool> func)
        {
            if (this.n != null)
            {
                var thisNode = new List<INode>();
                if (func(this.n))
                {
                    thisNode.Add(this.n);
                }
                var flattenedNodes = this.n.ChildrenAsList.Map(func, (INode n) => { return n.ChildrenAsList; });
                return new DynamicNodeList(thisNode.Concat(flattenedNodes).ToList().ConvertAll(iNode => new DynamicNode(iNode)));
            }
            return new DynamicNodeList(new List<INode>());
        }
        public DynamicNodeList Ancestors(int level)
        {
            return Ancestors(n => n.Level <= level);
        }
        public DynamicNodeList Ancestors(string nodeTypeAlias)
        {
            return Ancestors(n => n.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicNodeList Ancestors()
        {
            return Ancestors(n => true);
        }
        public DynamicNodeList Ancestors(Func<DynamicNode, bool> func)
        {
            List<DynamicNode> ancestorList = new List<DynamicNode>();
            var node = this;
            while (node != null)
            {
                if (node.Level == 1) break;
                DynamicNode parent = node.Parent;
                if (parent != null)
                {
                    if (this != parent)
                    {
                        node = parent;
                        if (func(node))
                        {
                            ancestorList.Add(node);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            ancestorList.Reverse();
            return new DynamicNodeList(ancestorList);
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
                if (n != null && n.Id == 0)
                {
                    return this;
                }
                return null;
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
        public DynamicNodeList NodeById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public DynamicNodeList NodeById(params object[] Ids)
        {
            return NodeById(Ids.ToList());
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
        public DynamicMediaList MediaById(List<object> Ids)
        {
            List<DynamicMedia> nodes = new List<DynamicMedia>();
            foreach (object eachId in Ids)
                nodes.Add(new DynamicMedia(eachId));
            return new DynamicMediaList(nodes);
        }
        public DynamicMediaList MediaById(params object[] Ids)
        {
            return MediaById(Ids.ToList());
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
            get
            {
                if (n == null) return true;
                IProperty umbracoNaviHide = n.GetProperty("umbracoNaviHide");
                if (umbracoNaviHide != null)
                {
                    return umbracoNaviHide.Value != "1";
                }
                return true;
            }
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

    }
}
