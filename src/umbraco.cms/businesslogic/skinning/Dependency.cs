using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Reflection;

namespace umbraco.cms.businesslogic.skinning
{
    public class Dependency
    {
        public Dependency()
        {
            Tasks = new List<Task>();
            Properties = new Dictionary<string, string>();
        }

        public static Dependency CreateFromXmlNode(XmlNode node)
        {
            Dependency d = new Dependency();



            if (node.SelectSingleNode("Properties") != null)
            {
                foreach (XmlNode prop in node.SelectSingleNode("Properties").ChildNodes)
                {
                   if(prop.Name != "Output" && prop.Name != "#comment")
                       d.Properties.Add(prop.Name, prop.InnerText);
                }
            }


            if(node.Attributes["label"] != null)
                d.Label = node.Attributes["label"].Value;

            if (node.Attributes["type"] != null)
            {

                if (node.Attributes["assembly"] != null)
                {
                    //custom dependency type

                    //string assemblyFile =
                    //    HttpContext.Current.Server.MapPath(
                    //    String.Format("{0}/../bin/{1}.dll",
                    //    GlobalSettings.Path,
                    //    node.Attributes["assembly"].Value));

                    //Assembly customAssembly = Assembly.LoadFrom(assemblyFile);

                    Assembly customAssembly = Assembly.Load(node.Attributes["assembly"].Value);

                    d.DependencyType = (DependencyType)Activator.CreateInstance(
                        customAssembly.GetType(node.Attributes["type"].Value),d);

                    foreach (var prop in d.Properties)
                    {
                        d.DependencyType.GetType().InvokeMember(prop.Key, System.Reflection.BindingFlags.SetProperty, null, d.DependencyType, new object[] { prop.Value });
                    }
                    //d.DependencyType.Properties = d.Properties;

                }
                else
                {
                    //internal dependency type

                    //string assemblyFile =
                    //    HttpContext.Current.Server.MapPath(
                    //    String.Format("{0}/../bin/{1}.dll",
                    //    GlobalSettings.Path,
                    //    "cms"));

                    //Assembly defaultAssembly = Assembly.LoadFrom(assemblyFile);

                    Assembly defaultAssembly = Assembly.Load("cms");

                    d.DependencyType = (DependencyType)Activator.CreateInstance(
                        defaultAssembly.GetType(
                            string.Format(
                                "umbraco.cms.businesslogic.skinning.dependencies.{0}",
                                node.Attributes["type"].Value)));


                    foreach (var prop in d.Properties)
                    {
                        d.DependencyType.GetType().InvokeMember(prop.Key, System.Reflection.BindingFlags.SetProperty, null, d.DependencyType, new object[] { prop.Value });
                    }

                    //d.DependencyType.Properties = d.Properties;
                }

                XmlNode outputNode = node.SelectSingleNode("Properties/Output");

                if (outputNode != null)
                {
                    d.DependencyType.Values.Add(outputNode.InnerText);
                }

                if (node.Attributes["variable"] != null)
                    d.Variable = node.Attributes["variable"].Value;
            }        


          

            foreach (XmlNode taskNode in node.SelectNodes("Tasks/Task"))
            {
                try
                {
                    d.Tasks.Add(Task.CreateFromXmlNode(taskNode));
                }
                catch (Exception ex)
                {
                    umbraco.BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, 0,
                        "Failed to load task type " + (taskNode.Attributes["type"] != null ? taskNode.Attributes["type"].Value : string.Empty) + ex.Message);
                }
            }


            return d;
        }


        public DependencyType DependencyType { get; set; }

        public string Label { get; set; }

        public Dictionary<string,string> Properties {get; set;}

        public List<Task> Tasks { get; set; }

        public string Variable { get; set; }
    }
}
