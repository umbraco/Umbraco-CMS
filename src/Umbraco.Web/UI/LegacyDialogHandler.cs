using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.interfaces;

namespace Umbraco.Web.UI
{
    /// <summary>
    /// This is used to replace the old umbraco.presentation.create.dialogHandler_temp class which is used
    /// to handle sections create/delete actions.
    /// </summary>
    /// <remarks>
    /// We need to overhaul how all of this is handled which is why this is a legacy class
    /// http://issues.umbraco.org/issue/U4-1373
    /// </remarks>
    public static class LegacyDialogHandler
    {
        public static void Delete(string nodeType, int nodeId, string text)
        {
            // Load task settings
            var createDef = GetXmlDoc();

            // Create an instance of the type by loading it from the assembly
            var def = createDef.SelectSingleNode("//nodeType [@alias = '" + nodeType + "']");
            var taskAssembly = def.SelectSingleNode("./tasks/delete").Attributes.GetNamedItem("assembly").Value;
            var taskType = def.SelectSingleNode("./tasks/delete").Attributes.GetNamedItem("type").Value;
            var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + taskAssembly + ".dll"));
            var type = assembly.GetType(taskAssembly + "." + taskType);
            var typeInstance = Activator.CreateInstance(type) as ITask;
            if (typeInstance == null) return;
            typeInstance.ParentID = nodeId;
            typeInstance.Alias = text;
            typeInstance.Delete();
        }

        public static string Create(string nodeType, int nodeId, string text, int typeId = 0)
        {
            // Load task settings
            var createDef = GetXmlDoc();

            // Create an instance of the type by loading it from the assembly
            var def = createDef.SelectSingleNode("//nodeType [@alias = '" + nodeType + "']");
            var taskAssembly = def.SelectSingleNode("./tasks/create").Attributes.GetNamedItem("assembly").Value;
            var taskType = def.SelectSingleNode("./tasks/create").Attributes.GetNamedItem("type").Value;

            var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + taskAssembly + ".dll"));
            var type = assembly.GetType(taskAssembly + "." + taskType);
            var typeInstance = Activator.CreateInstance(type) as ITask;
            if (typeInstance != null)
            {
                typeInstance.TypeID = typeId;
                typeInstance.ParentID = nodeId;
                typeInstance.Alias = text;
                typeInstance.UserId = BasePage.GetUserId(BasePage.umbracoUserContextID);
                typeInstance.Save();

                // check for returning url
                var returnUrlTask = typeInstance as ITaskReturnUrl;
                return returnUrlTask != null 
                    ? returnUrlTask.ReturnUrl 
                    : "";
            }

            return "";
        }

        private static XmlDocument GetXmlDoc()
        {
            // Load task settings
            var createDef = new XmlDocument();
            using (var defReader = new XmlTextReader(IOHelper.MapPath(SystemFiles.CreateUiXml)))
            {
                createDef.Load(defReader);
                defReader.Close();
                return createDef;
            }            
        }
    }
}
