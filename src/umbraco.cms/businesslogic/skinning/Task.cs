using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Reflection;

namespace umbraco.cms.businesslogic.skinning
{
    public class Task
    {
        public Task()
        {
            Properties = new Dictionary<string, string>();
        }

        public static Task CreateFromXmlNode(XmlNode node)
        {
            Task t = new Task();

            if (node.Attributes["type"] != null)
            {

                if (node.Attributes["assembly"] != null)
                {
                    //custom task type

                    //string assemblyFile =
                    //    HttpContext.Current.Server.MapPath(
                    //    String.Format("{0}/../bin/{1}.dll",
                    //    GlobalSettings.Path,
                    //    node.Attributes["assembly"].Value));

                    //Assembly customAssembly = Assembly.LoadFrom(assemblyFile);
                    Assembly customAssembly = Assembly.Load(node.Attributes["assembly"].Value);

                    t.TaskType = (TaskType)Activator.CreateInstance(
                        customAssembly.GetType(node.Attributes["type"].Value));

                    

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

                    t.TaskType = (TaskType)Activator.CreateInstance(
                        defaultAssembly.GetType(
                            string.Format(
                                "umbraco.cms.businesslogic.skinning.tasks.{0}",
                                node.Attributes["type"].Value)));
                }
            }    

            foreach (XmlNode prop in node.ChildNodes)
            {
                if (prop.Name != "OriginalValue" && prop.Name != "NewValue")
                {
                    if (prop.Name == "Value")
                        t.Value = prop.InnerText;
                    else
                        t.Properties.Add(prop.Name, prop.InnerText);

                    t.TaskType.GetType().InvokeMember(
                        prop.Name,
                        System.Reflection.BindingFlags.SetProperty,
                        null,
                        t.TaskType,
                        new object[] { prop.InnerText });
                }
            }
            return t;
        }

        public TaskType TaskType { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public string Value { get; set; }
    }
}
