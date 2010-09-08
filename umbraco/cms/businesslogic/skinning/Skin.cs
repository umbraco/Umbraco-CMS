using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using umbraco.interfaces.skinning;

namespace umbraco.cms.businesslogic.skinning
{
    public class Skin
    {
        public Skin()
        {
            AllowedDocumentTypeAliases = new List<string>();
            Dependencies = new List<Dependency>();
        }

        public static Skin CreateFromFile(string filename)
        {
            XmlDocument manifest = new XmlDocument();

            FileInfo f = new FileInfo(filename);
            
            if (f.Exists)
            {
                manifest.Load(filename);

                Skin s = Skin.CreateFromXmlNode(manifest.SelectSingleNode("//Skin"));
                s.FullFileName = filename;
                s.Alias = f.Directory.Name;

                return s;
            }
            else
                return null;
               
           
        }

        public static Skin CreateFromXmlNode(XmlNode node)
        {
            Skin s = new Skin();


            if(node.SelectSingleNode("/Skin/Name") != null)
                s.Name = node.SelectSingleNode("/Skin/Name").InnerText;

            if(node.SelectSingleNode("/Skin/Author") != null)
                s.Author = node.SelectSingleNode("/Skin/Author").InnerText;

            if(node.SelectSingleNode("/Skin/Version") != null)
                s.Version = node.SelectSingleNode("/Skin/Version").InnerText;

            if(node.SelectSingleNode("/Skin/Description") != null)
                s.Description = node.SelectSingleNode("/Skin/Description").InnerText;

            if (node.SelectSingleNode("/Skin/AllowedDocumentTypes") != null)
            {
                s.AllowedDocumentTypeAliases.AddRange(
                    node.SelectSingleNode("/Skin/AllowedDocumentTypes").InnerText.Split(','));
            }

            if (node.SelectSingleNode("/Skin/AllowedRootTemplate") != null)
                s.AllowedRootTemplate = node.SelectSingleNode("/Skin/AllowedRootTemplate").InnerText;


            foreach (XmlNode depNode in node.SelectNodes("/Skin//Dependency"))
            {
                try
                {
                    s.Dependencies.Add(Dependency.CreateFromXmlNode(depNode));
                }
                catch (Exception ex)
                {
                    umbraco.BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, 0,
                        "Failed to load dependency type " + (depNode.Attributes["type"] != null ? depNode.Attributes["type"].Value : string.Empty) + ex.Message);
                }
            }


            return s;
        }

        public static Skin CreateFromAlias(string alias)
        {
            string manifest = Path.Combine(IO.IOHelper.MapPath( IO.SystemDirectories.Masterpages + "/" + alias), "skin.xml");
            return CreateFromFile(manifest);
        }

        public bool OverridesTemplates()
        {
            return (System.IO.Directory.GetFiles(IO.IOHelper.MapPath(SkinFolder), "*.master").Count() > 0);
        }

        public bool HasBackupFiles()
        {
            return System.IO.Directory.Exists( IO.IOHelper.MapPath(SkinBackupFolder) );
        }
        

        public void SaveOutput()
        {

            XmlDocument manifest = new XmlDocument();
            manifest.Load(FullFileName);

            int i = 1;
            foreach (Dependency d in Dependencies)
            {
                if (d.DependencyType.Values.Count > 0)
                {
                    XmlNode pNode = manifest.SelectSingleNode(
                        string.Format("/Skin/Dependencies/Dependency[{0}]/Properties", i.ToString()));


                    XmlNode outputNode = pNode.SelectSingleNode("./Output");

                    if (outputNode == null)
                    {
                        outputNode = manifest.CreateElement("Output");
                        outputNode.InnerText = d.DependencyType.Values[0].ToString();
                        pNode.AppendChild(outputNode);
                    }
                    else
                    {
                        outputNode.InnerText = d.DependencyType.Values[0].ToString();
                    }

                    
                }

                i++;
            }

            manifest.Save(FullFileName);
        }

        public string FullFileName { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public string Alias { get; set; }

        public List<string> AllowedDocumentTypeAliases { get; set; }
        public string AllowedRootTemplate { get; set; }


        public List<Dependency> Dependencies { get; set; }

        public void RollbackDependencies()
        {
            XmlDocument manifest = new XmlDocument();
            manifest.Load(FullFileName);
            XmlNode hNode = manifest.SelectSingleNode("/Skin/History");

            if (!(hNode == null || hNode.SelectNodes("Task").Count == 0))
            {
                XPathNavigator navigator = manifest.CreateNavigator();
                XPathExpression expression = navigator.Compile("/Skin/History/Task");

                expression.AddSort("@executed", XmlSortOrder.Descending, XmlCaseOrder.UpperFirst,
                        string.Empty, XmlDataType.Text);

                XPathNodeIterator iterator = navigator.Select(expression);

                foreach (XPathNavigator item in iterator)
                {
                    Task t = Task.CreateFromXmlNode(((System.Xml.IHasXmlNode)item).GetNode());
                    t.TaskType.RollBack(((System.Xml.IHasXmlNode)item).GetNode().SelectSingleNode("OriginalValue").InnerText);
                }

                hNode.RemoveAll();
                manifest.Save(FullFileName);
            }      
        }

        public void DeployTemplateFiles()
        {
            DeployTemplateFiles(IO.SystemDirectories.Masterpages);
        }


        public void DeployTemplateFiles(string folder)
        {
            string[] masterFiles = System.IO.Directory.GetFiles(IO.IOHelper.MapPath(SkinFolder), "*.master");

            if (masterFiles.Count() > 0)
            {
                if (!System.IO.Directory.Exists(IO.IOHelper.MapPath(SkinBackupFolder)))
                    System.IO.Directory.CreateDirectory(IO.IOHelper.MapPath(SkinBackupFolder));

                foreach (string master in masterFiles)
                {
                    FileInfo fi = new FileInfo(master);
                    string original = Path.Combine(IO.IOHelper.MapPath(IO.SystemDirectories.Masterpages), fi.Name);
                    string backup = Path.Combine(IO.IOHelper.MapPath(SkinBackupFolder), fi.Name);

                    if (System.IO.File.Exists(original))
                        System.IO.File.Copy(original, backup, true);

                    System.IO.File.Copy(master, original, true);
                }
            }
        }


        public void RollbackTemplateFiles(){
            RollbackTemplateFiles(IO.SystemDirectories.Masterpages);
        }

        public void RollbackTemplateFiles(string folder)
        {
            string[] masterFiles = System.IO.Directory.GetFiles(IO.IOHelper.MapPath(SkinFolder), "*.master");

            //1. take the skin files back into skin folder to store the changes made
            if (masterFiles.Count() > 0)
            {
                foreach (string master in masterFiles)
                {
                    FileInfo fi = new FileInfo(master);
                    string inUse = Path.Combine(IO.IOHelper.MapPath(IO.SystemDirectories.Masterpages), fi.Name);


                    if (System.IO.File.Exists(inUse))
                        System.IO.File.Copy(inUse, master, true);
                }
            }


            //2. copy the /backup files back to the masterpages to restore the templates to the way they were before the skin was applied
            string[] backedUpmasterFiles = System.IO.Directory.GetFiles(IO.IOHelper.MapPath(SkinBackupFolder), "*.master");
            foreach (string backup in backedUpmasterFiles)
            {
                FileInfo fi = new FileInfo(backup);
                string inUse = Path.Combine(IO.IOHelper.MapPath(IO.SystemDirectories.Masterpages), fi.Name);

                System.IO.File.Copy(backup, inUse, true);
                System.IO.File.Delete(backup);
            }

            //3. put on some clothes
        }

        public void ExecuteInstallTasks()
        {
            XmlDocument manifest = new XmlDocument();
            manifest.Load(FullFileName);

            foreach (XmlNode tNode in manifest.SelectNodes("/Skin/Install/Task"))
            {
                Task t = Task.CreateFromXmlNode(tNode);
                TaskExecutionDetails details = t.TaskType.Execute(t.Value);

                if (details.TaskExecutionStatus == TaskExecutionStatus.Completed)
                {
                   AddTaskHistoryNode(
                        t.TaskType.ToXml(details.OriginalValue, details.NewValue));
                }

            }
        }

        public void AddTaskHistoryNode(XmlNode taskNode)
        {
            XmlDocument manifest = new XmlDocument();
            manifest.Load(FullFileName);

            XmlNode hNode = manifest.SelectSingleNode("/Skin/History");

            if (hNode == null)
            {
                hNode = manifest.CreateElement("History");
                manifest.SelectSingleNode("/Skin").AppendChild(hNode);
            }

            hNode.AppendChild(manifest.ImportNode(taskNode, true));

            manifest.Save(FullFileName);
        }

        public string SkinFolder
        {
            get
            {
                return IO.SystemDirectories.Masterpages + "/" + Alias;
            }
        }

        public string SkinBackupFolder
        {
            get
            {
                return SkinFolder + "/backup";
            }
        }

    }
}
