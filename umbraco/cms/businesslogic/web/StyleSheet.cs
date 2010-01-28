using System;
using System.Collections;
using System.IO;
using System.Data;
using System.Xml;
using umbraco.DataLayer;
using System.Linq;
using System.Text.RegularExpressions;
using umbraco.cms.businesslogic.cache;
using umbraco.IO;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for StyleSheet.
    /// </summary>
    public class StyleSheet : CMSNode
    {

        private string _filename = "";
        private string _content = "";
        private StylesheetProperty[] m_properties;
        public static Guid ModuleObjectType = new Guid("9f68da4f-a3a8-44c2-8226-dcbd125e4840");

        private static object stylesheetCacheSyncLock = new object();
        private static readonly string UmbracoStylesheetCacheKey = "UmbracoStylesheet";

        public string Filename
        {
            get { return _filename; }
            set
            {

                //move old file
                _filename = value;
                SqlHelper.ExecuteNonQuery("update cmsStylesheet set filename = '" + _filename + "' where nodeId = " + base.Id.ToString());
                InvalidateCache();

            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                SqlHelper.ExecuteNonQuery("update cmsStylesheet set content = @content where nodeId = @id", SqlHelper.CreateParameter("@content", this.Content), SqlHelper.CreateParameter("@id", this.Id));
                InvalidateCache();
            }
        }

        public StylesheetProperty[] Properties
        {
            get
            {
                if (m_properties == null)
                {
                    BusinessLogic.console.IconI[] tmp = this.ChildrenOfAllObjectTypes;

                    StylesheetProperty[] retVal = new StylesheetProperty[tmp.Length];
                    for (int i = 0; i < tmp.Length; i++)
                        retVal[i] = StylesheetProperty.GetStyleSheetProperty(this.ChildrenOfAllObjectTypes[i].Id);
                    m_properties = retVal;
                }
                return m_properties;
            }
        }

        //static bool isInitialized = false;

        public StyleSheet(Guid id)
            : base(id)
        {
            setupStyleSheet(true, true);
        }

        public StyleSheet(int id)
            : base(id)
        {
            setupStyleSheet(true, true);
        }

        public StyleSheet(int id, bool setupStyleProperties, bool loadContentFromFile)
            : base(id)
        {
            setupStyleSheet(loadContentFromFile, setupStyleProperties);
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);
            if (!e.Cancel)
            {
                base.Save();
                FireAfterSave(e);
            }
        }


        private void setupStyleSheet(bool loadFileData, bool updateStyleProperties)
        {
            // Get stylesheet data
            IRecordsReader dr = SqlHelper.ExecuteReader("select filename, content from cmsStylesheet where nodeid = " + base.Id.ToString());
            if (dr.Read())
            {
                if (!dr.IsNull("filename"))
                    _filename = dr.GetString("filename");
                // Get Content from db or file 
                if (!loadFileData)
                {
                    if (!dr.IsNull("content"))
                        _content = dr.GetString("content");
                }
                else
                    if (File.Exists(IOHelper.MapPath(SystemDirectories.Css + "/" + this.Text + ".css")))
                    {

                        StreamReader re = File.OpenText(IOHelper.MapPath(SystemDirectories.Css + "/" + this.Text + ".css"));
                        string input = null;
                        bool read = true;
                        _content = string.Empty;
                        // NH: Updates the reader to support properties
                        bool readingProperties = false;
                        string propertiesContent = String.Empty;
                        while ((input = re.ReadLine()) != null && read)
                        {
                            if (input.Contains("EDITOR PROPERTIES"))
                            {
                                readingProperties = true;
                            }
                            else if (readingProperties)
                            {
                                propertiesContent += input.Replace("\n", "") + "\n";
                            }
                            else
                            {
                                _content += input.Replace("\n", "") + "\n";

                            }
                        }
                        re.Close();

                        // update properties
                        if (updateStyleProperties)
                        {
                            if (propertiesContent != String.Empty)
                            {
                                parseProperties(propertiesContent);
                            }
                        }
                    }

            }
            dr.Close();

        }

        private void parseProperties(string propertiesContent)
        {
            MatchCollection m =
    Regex.Matches(propertiesContent, "([^{]*){([^}]*)}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in m)
            {
                GroupCollection groups = match.Groups;
                string cssClass = groups[1].Value.Replace("\n", "").Replace("\r", "").Trim().Trim(Environment.NewLine.ToCharArray());
                string cssCode = groups[2].Value.Trim(Environment.NewLine.ToCharArray());
                foreach (StylesheetProperty sp in this.Properties)
                    if (sp.Alias == cssClass)
                        sp.value = cssCode;
            }
        }

        public static StyleSheet MakeNew(BusinessLogic.User user, string Text, string FileName, string Content)
        {

            // Create the umbraco node
            CMSNode newNode = CMSNode.MakeNew(-1, ModuleObjectType, user.Id, 1, Text, Guid.NewGuid());

            // Create the stylesheet data
            SqlHelper.ExecuteNonQuery("insert into cmsStylesheet (nodeId, filename, content) values ('" + newNode.Id.ToString() + "','" + FileName + "',@content)", SqlHelper.CreateParameter("@content", Content));

            // save to file to avoid file coherency issues
            StyleSheet newCss = new StyleSheet(newNode.Id, false, false);
            NewEventArgs e = new NewEventArgs();
            newCss.OnNew(e);

            return newCss;
        }

        public static StyleSheet[] GetAll()
        {

            ArrayList dbStylesheets = new ArrayList();

            Guid[] topNodeIds = CMSNode.TopMostNodeIds(ModuleObjectType);
            //StyleSheet[] retval = new StyleSheet[topNodeIds.Length];
            for (int i = 0; i < topNodeIds.Length; i++)
            {
                //retval[i] = new StyleSheet(topNodeIds[i]);
                dbStylesheets.Add(new StyleSheet(topNodeIds[i]).Text.ToLower());
            }

            ArrayList fileStylesheets = new ArrayList();
            DirectoryInfo fileListing = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Css + "/"));

            foreach (FileInfo file in fileListing.GetFiles("*.css"))
            {
                if (!dbStylesheets.Contains(file.Name.Replace(file.Extension, "").ToLower()))
                {
                    fileStylesheets.Add(file.Name.Replace(file.Extension, ""));
                }
            }

            StyleSheet[] retval = new StyleSheet[dbStylesheets.Count + fileStylesheets.Count];
            for (int i = 0; i < topNodeIds.Length; i++)
            {
                retval[i] = new StyleSheet(topNodeIds[i]);

            }

            for (int i = 0; i < fileStylesheets.Count; i++)
            {

                string content = string.Empty;

                using (StreamReader re = File.OpenText(IOHelper.MapPath(SystemDirectories.Css + "/" + fileStylesheets[i].ToString() + ".css")))
                {
                    content = re.ReadToEnd();
                }

                retval[dbStylesheets.Count + i] = StyleSheet.MakeNew(new umbraco.BusinessLogic.User(0), fileStylesheets[i].ToString(), fileStylesheets[i].ToString(), content);
            }


            Array.Sort(retval, 0, retval.Length, new StyleSheetComparer());

            return retval;
        }



        public StylesheetProperty AddProperty(string Alias, BusinessLogic.User u)
        {
            return StylesheetProperty.MakeNew(Alias, this, u);
        }

        new public void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                File.Delete(IOHelper.MapPath(SystemDirectories.Css + "/" + this.Text + ".css"));
                foreach (StylesheetProperty p in this.Properties)
                    p.delete();
                SqlHelper.ExecuteNonQuery("delete from cmsStylesheet where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
                base.delete();

                FireAfterDelete(e);
            }

        }

        public void saveCssToFile()
        {
            StreamWriter SW;
            SW = File.CreateText(IOHelper.MapPath(SystemDirectories.Css + "/" + this.Text + ".css"));
            string tmpCss;
            //tmpCss = "/* GENERAL STYLES */\n";
            tmpCss = this.Content + "\n\n";
            tmpCss += "/* EDITOR PROPERTIES - PLEASE DON'T DELETE THIS LINE TO AVOID DUPLICATE PROPERTIES */\n";
            foreach (StylesheetProperty p in this.Properties)
            {
                tmpCss += p.ToString() + "\n";
            }
            SW.Write(tmpCss);
            SW.Close();
        }

        public XmlNode ToXml(XmlDocument xd)
        {
            XmlNode doc = xd.CreateElement("Stylesheet");
            doc.AppendChild(xmlHelper.addTextNode(xd, "Name", this.Text));
            doc.AppendChild(xmlHelper.addTextNode(xd, "FileName", this.Filename));
            doc.AppendChild(xmlHelper.addCDataNode(xd, "Content", this.Content));

            if (this.Properties.Length > 0)
            {
                XmlNode properties = xd.CreateElement("Properties");
                foreach (StylesheetProperty sp in this.Properties)
                {
                    XmlElement prop = xd.CreateElement("Property");
                    prop.AppendChild(xmlHelper.addTextNode(xd, "Name", sp.Text));
                    prop.AppendChild(xmlHelper.addTextNode(xd, "Alias", sp.Alias));
                    prop.AppendChild(xmlHelper.addTextNode(xd, "Value", sp.value));
                    properties.AppendChild(prop);
                }
                doc.AppendChild(properties);
            }

            return doc;
        }


        public static StyleSheet GetStyleSheet(int id, bool setupStyleProperties, bool loadContentFromFile)
        {
            return Cache.GetCacheItem<StyleSheet>(GetCacheKey(id), stylesheetCacheSyncLock,
    TimeSpan.FromMinutes(30),
    delegate
    {
        try
        {
            return new StyleSheet(id, setupStyleProperties, loadContentFromFile);
        }
        catch
        {
            return null;
        }
    });

        }

        private void InvalidateCache()
        {
            Cache.ClearCacheItem(GetCacheKey(this.Id));
        }

        private static string GetCacheKey(int id)
        {
            return UmbracoStylesheetCacheKey + id;
        }


        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(StyleSheet sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(StyleSheet sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(StyleSheet sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        /// <summary>
        /// Occurs when [new].
        /// </summary>
        public static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        public static StyleSheet GetByName(string name)
        {
            try
            {
                int id = SqlHelper.ExecuteScalar<int>("SELECT id FROM umbracoNode WHERE text = @text AND nodeObjectType = @objType ", SqlHelper.CreateParameter("@text", name), SqlHelper.CreateParameter("@objType", StyleSheet.ModuleObjectType));
                return new StyleSheet(id);
            }
            catch
            {
                return null;
            }
        }

        public static StyleSheet Import(XmlNode n, umbraco.BusinessLogic.User u)
        {
            string stylesheetName = xmlHelper.GetNodeValue(n.SelectSingleNode("Name"));
            StyleSheet s = GetByName(stylesheetName);
            if (s == null)
            {
                s = StyleSheet.MakeNew(
                    u,
                    stylesheetName,
                    xmlHelper.GetNodeValue(n.SelectSingleNode("FileName")),
                    xmlHelper.GetNodeValue(n.SelectSingleNode("Content")));
            }

            foreach (XmlNode prop in n.SelectNodes("Properties/Property"))
            {
                string alias = xmlHelper.GetNodeValue(prop.SelectSingleNode("Alias"));
                var sp = s.Properties.SingleOrDefault(p => p != null && p.Alias == alias);
                string name = xmlHelper.GetNodeValue(prop.SelectSingleNode("Name"));
                if (sp == default(StylesheetProperty))
                {
                    sp = StylesheetProperty.MakeNew(
                        name,
                        s,
                        u);
                }
                else
                {
                    sp.Text = name;
                }
                sp.Alias = alias;
                sp.value = xmlHelper.GetNodeValue(prop.SelectSingleNode("Value"));
            }
            s.saveCssToFile();

            return s;
        }
    }


    public class StyleSheetComparer : IComparer
    {
        public StyleSheetComparer()
        {
            //default constructor
        }

        public Int32 Compare(Object pFirstObject, Object pObjectToCompare)
        {
            if (pFirstObject is StyleSheet)
            {
                return String.Compare(((StyleSheet)pFirstObject).Text, ((StyleSheet)pObjectToCompare).Text);
            }
            else
            {
                return 0;
            }
        }
    }

}
