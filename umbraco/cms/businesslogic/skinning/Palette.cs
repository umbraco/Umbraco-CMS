//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Xml;

//namespace umbraco.cms.businesslogic.skinning
//{
//    public class Palette
//    {
//        public Palette()
//        {

//        }

//        public static Palette CreateFromXmlNode(XmlNode node)
//        {
//            Palette d = new Palette();

//            if(node.Attributes["name"] != null)
//                d.Name = node.Attributes["name"].Value;

//            if (node.Attributes["stylesheet"] != null)
//                d.Stylesheet = node.Attributes["stylesheet"].Value;

//            return d;

//        }

//        public string Name { get; set; }
//        public string Stylesheet { get; set; }
//    }
//}
