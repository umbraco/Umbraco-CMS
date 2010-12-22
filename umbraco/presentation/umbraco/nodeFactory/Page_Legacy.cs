using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.propertytype;
using System.Collections.Generic;
using umbraco.interfaces;

using NF = umbraco.nodeFactory;

namespace umbraco.presentation.nodeFactory
{
    /// <summary>
    /// Summary description for Node.
    /// </summary>

    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    [Obsolete("This class is obsolete; use class umbraco.nodeFactory.Node instead")]
    public class Node : NF.Node
    {

        public new Node Parent 
        {
            get
            {
                return base.Parent as Node;
            }
        }
        
        public new Properties Properties
        {
            get
            {
               return base.Properties as Properties;
            }
        }


        /*
        public new List<Property> PropertiesAsList
        {
            get { return Properties.Cast<Property>().ToList(); }
        }
        */
     
        public Node(XmlNode NodeXmlNode) : base(NodeXmlNode){}

        public Node(XmlNode NodeXmlNode, bool DisableInitializing) : base(NodeXmlNode, DisableInitializing){}

        public Node(int NodeId) : base(NodeId){
            var p = NodeId;
        }

        public Node(int NodeId, bool forcePublishedXml) : base(NodeId, forcePublishedXml) {}

        public new Property GetProperty(string Alias)
        {
            return base.GetProperty(Alias) as Property;
        }

        public new static Node GetCurrent()
        {
            int id = NF.Node.getCurrentNodeId();
            return new Node(id);
        }
        
    }

    [Obsolete("This class is obsolete; use class umbraco.nodeFactory.Nodes instead")]
    public class Nodes : NF.Nodes {}

    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    [Obsolete("This class is obsolete; use class umbraco.nodeFactory.Property instead")]
    public class Property : NF.Property{}

    [Obsolete("This class is obsolete; use class umbraco.nodeFactory.Properties instead")]
    public class Properties : NF.Properties{}
    
}