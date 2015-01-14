using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using umbraco.BusinessLogic.console;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.cache;
using Umbraco.Core.Models;
using umbraco.DataLayer;
using Umbraco.Core;
using File = System.IO.File;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for StyleSheet.
    /// </summary>
    [Obsolete("Use Umbraco.Core.Services.IFileService instead")]
    public class StyleSheet : CMSNode
    {
        internal Stylesheet StylesheetItem;

        //private string _filename = "";
        //private string _content = "";
        //private StylesheetProperty[] _properties;
        public static Guid ModuleObjectType = new Guid(Constants.ObjectTypes.Stylesheet);

        public string Filename
        {
            get
            {
                var path = StylesheetItem.Path;
                if (path.IsNullOrWhiteSpace()) return string.Empty;
                return System.IO.Path.GetFileNameWithoutExtension(StylesheetItem.Path);
            }
            set
            {
                //NOTE: This has zero affect

                ////move old file
                //_filename = value;
                //SqlHelper.ExecuteNonQuery(string.Format("update cmsStylesheet set filename = '{0}' where nodeId = {1}", _filename, Id));
                //InvalidateCache();
            }
        }

        public string Content
        {
            get { return StylesheetItem.Content; }
            set
            {
                StylesheetItem.Content = value;
                //_content = value;
                //SqlHelper.ExecuteNonQuery("update cmsStylesheet set content = @content where nodeId = @id", SqlHelper.CreateParameter("@content", this.Content), SqlHelper.CreateParameter("@id", this.Id));
                //InvalidateCache();
            }
        }

        public StylesheetProperty[] Properties
        {
            get
            {
                //if (_properties == null)
                //{
                //    var tmp = this.ChildrenOfAllObjectTypes;
                //    var retVal = new StylesheetProperty[tmp.Length];
                //    for (var i = 0; i < tmp.Length; i++)
                //    {
                //        //So this will go get cached properties but yet the above call to ChildrenOfAllObjectTypes is not cached :/
                //        retVal[i] = StylesheetProperty.GetStyleSheetProperty(tmp[i].Id);
                //    }
                //    _properties = retVal;
                //}
                //return _properties;

                var tmp = this.ChildrenOfAllObjectTypes;
                var retVal = new StylesheetProperty[tmp.Length];
                for (var i = 0; i < tmp.Length; i++)
                {
                    //So this will go get cached properties but yet the above call to ChildrenOfAllObjectTypes is not cached :/
                    retVal[i] = StylesheetProperty.GetStyleSheetProperty(tmp[i].Id);
                }
                return retVal;
            }
        }

        //static bool isInitialized = false;

        internal StyleSheet(Stylesheet stylesheet)
            : base(stylesheet)
        {
            if (stylesheet == null) throw new ArgumentNullException("stylesheet");
            StylesheetItem = stylesheet;
        }

        public StyleSheet(Guid id)
            : base(id)
        {
            //SetupStyleSheet(true, true);
        }

        public StyleSheet(int id)
            : base(id)
        {
            //SetupStyleSheet(true, true);
        }

        [Obsolete("This constructors parameters: setupStyleProperties, loadContentFromFile don't do anything")]
        public StyleSheet(int id, bool setupStyleProperties, bool loadContentFromFile)
            : base(id)
        {
            //SetupStyleSheet(loadContentFromFile, setupStyleProperties);
        }

        /// <summary>
        /// Sort order does nothing for Stylesheets
        /// </summary>
        [Obsolete("Sort order does nothing for Stylesheets")]
        public override int sortOrder
        {
            get { return 0; }
            set { /* do nothing; */}
        }

        /// <summary>
        /// Sort order does nothing for Stylesheets
        /// </summary>
        [Obsolete("Sort order does nothing for Stylesheets")]
        public override int Level
        {
            get { return 1; }
            set { /* do nothing; */}
        }

        /// <summary>
        /// Gets or sets the create date time.
        /// </summary>
        /// <value>The create date time.</value>
        public override DateTime CreateDateTime
        {
            get { return StylesheetItem.CreateDate; }
            set { StylesheetItem.CreateDate = value; }
        }
        
        /// <summary>
        /// Human readable name/label
        /// </summary>
        public override string Text
        {
            get { return Filename; }
            set
            {
                var currFileName = System.IO.Path.GetFileName(StylesheetItem.Path);
                var newFilePath = StylesheetItem.Path.TrimEnd(currFileName) + value;
                StylesheetItem.Path = newFilePath;
            }
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);
            if (!e.Cancel)
            {
                ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetItem);

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Sets up the internal data of the CMSNode, used by the various constructors
        /// </summary>
        protected override void setupNode()
        {
            var found = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<string>(
                "SELECT TOP 1 filename FROM cmsStylesheet WHERE nodeid=" + Id);
            if (found.IsNullOrWhiteSpace()) throw new ArgumentException(string.Format("No stylesheet exists with id '{0}'", Id));

            StylesheetItem = ApplicationContext.Current.Services.FileService.GetStylesheetByName(found + ".css");
            if (StylesheetItem == null) throw new ArgumentException(string.Format("No stylesheet exists with name '{0}.css'", found));
        }

        //private void SetupStyleSheet(bool loadFileData, bool updateStyleProperties)
        //{
        //    // Get stylesheet data
        //    using (var dr = SqlHelper.ExecuteReader("select filename, content from cmsStylesheet where nodeid = " + Id))
        //    {
        //        if (dr.Read())
        //        {
        //            if (!dr.IsNull("filename"))
        //                _filename = dr.GetString("filename");
        //            // Get Content from db or file 
        //            if (!loadFileData)
        //            {
        //                if (!dr.IsNull("content"))
        //                    _content = dr.GetString("content");
        //            }
        //            else if (File.Exists(IOHelper.MapPath(String.Format("{0}/{1}.css", SystemDirectories.Css, this.Text))))
        //            {
        //                var propertiesContent = String.Empty;

        //                using (var re = File.OpenText(IOHelper.MapPath(String.Format("{0}/{1}.css", SystemDirectories.Css, this.Text))))
        //                {
        //                    string input = null;
        //                    _content = string.Empty;
        //                    // NH: Updates the reader to support properties
        //                    var readingProperties = false;

        //                    while ((input = re.ReadLine()) != null && true)
        //                    {
        //                        if (input.Contains("EDITOR PROPERTIES"))
        //                        {
        //                            readingProperties = true;
        //                        }
        //                        else
        //                            if (readingProperties)
        //                            {
        //                                propertiesContent += input.Replace("\n", "") + "\n";
        //                            }
        //                            else
        //                            {
        //                                _content += input.Replace("\n", "") + "\n";
        //                            }
        //                    }
        //                }

        //                // update properties
        //                if (updateStyleProperties)
        //                {
        //                    if (propertiesContent != String.Empty)
        //                    {
        //                        ParseProperties(propertiesContent);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private void ParseProperties(string propertiesContent)
        //{
        //    var m = Regex.Matches(propertiesContent, "([^{]*){([^}]*)}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        //    foreach (Match match in m)
        //    {
        //        var groups = match.Groups;
        //        var cssClass = groups[1].Value.Replace("\n", "").Replace("\r", "").Trim().Trim(Environment.NewLine.ToCharArray());
        //        var cssCode = groups[2].Value.Trim(Environment.NewLine.ToCharArray());
        //        foreach (StylesheetProperty sp in this.Properties)
        //            if (sp.Alias == cssClass && sp.value != cssCode) // check before setting to avoid invalidating cache unecessarily
        //                sp.value = cssCode;
        //    }
        //}

        public static StyleSheet MakeNew(BusinessLogic.User user, string Text, string FileName, string Content)
        {

            if (FileName.IsNullOrWhiteSpace())
            {
                FileName = Text;
            }

            // validate if node ends with css, if it does we'll remove it as we append it later
            if (Text.ToLowerInvariant().EndsWith(".css"))
            {
                Text = Text.Substring(0, Text.Length - 4);
            }

            var newSheet = new Stylesheet(FileName)
            {
                Content = Content
            };
            ApplicationContext.Current.Services.FileService.SaveStylesheet(newSheet);

            //// Create the umbraco node
            //var newNode = CMSNode.MakeNew(-1, ModuleObjectType, user.Id, 1, Text, Guid.NewGuid());

            //// Create the stylesheet data
            //SqlHelper.ExecuteNonQuery(string.Format("insert into cmsStylesheet (nodeId, filename, content) values ('{0}','{1}',@content)", newNode.Id, FileName), SqlHelper.CreateParameter("@content", Content));

            var newCss = new StyleSheet(newSheet);
            var e = new NewEventArgs();
            newCss.OnNew(e);

            return newCss;
        }

        public static StyleSheet[] GetAll()
        {
            var retval = ApplicationContext.Current.Services.FileService.GetStylesheets()
                .OrderBy(x => x.Alias)
                .Select(x => new StyleSheet(x))
                .ToArray();

            //var dbStylesheets = new ArrayList();

            //var topNodeIds = CMSNode.TopMostNodeIds(ModuleObjectType);
            ////StyleSheet[] retval = new StyleSheet[topNodeIds.Length];
            //for (int i = 0; i < topNodeIds.Length; i++)
            //{
            //    //retval[i] = new StyleSheet(topNodeIds[i]);
            //    dbStylesheets.Add(new StyleSheet(topNodeIds[i]).Text.ToLower());
            //}

            //var fileStylesheets = new ArrayList();
            //var fileListing = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Css + "/"));

            //foreach (var file in fileListing.GetFiles("*.css"))
            //{
            //    if (!dbStylesheets.Contains(file.Name.Replace(file.Extension, "").ToLower()))
            //    {
            //        fileStylesheets.Add(file.Name.Replace(file.Extension, ""));
            //    }
            //}

            //var retval = new StyleSheet[dbStylesheets.Count + fileStylesheets.Count];
            //for (int i = 0; i < topNodeIds.Length; i++)
            //{
            //    retval[i] = new StyleSheet(topNodeIds[i]);

            //}

            //for (int i = 0; i < fileStylesheets.Count; i++)
            //{

            //    string content = string.Empty;

            //    using (StreamReader re = File.OpenText(IOHelper.MapPath(string.Format("{0}/{1}.css", SystemDirectories.Css, fileStylesheets[i]))))
            //    {
            //        content = re.ReadToEnd();
            //    }

            //    retval[dbStylesheets.Count + i] = StyleSheet.MakeNew(new umbraco.BusinessLogic.User(0), fileStylesheets[i].ToString(), fileStylesheets[i].ToString(), content);
            //}


            //Array.Sort(retval, 0, retval.Length, new StyleSheetComparer());

            return retval;
        }
        
        public StylesheetProperty AddProperty(string Alias, BusinessLogic.User u)
        {
            return StylesheetProperty.MakeNew(Alias, this, u);
        }

        public override void delete()
        {
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);
            
            if (!e.Cancel)
            {
                ApplicationContext.Current.Services.FileService.DeleteStylesheet(StylesheetItem.Path);

                //File.Delete(IOHelper.MapPath(String.Format("{0}/{1}.css", SystemDirectories.Css, this.Text)));
                //foreach (var p in Properties.Where(p => p != null))
                //{
                //    p.delete();
                //}
                //SqlHelper.ExecuteNonQuery("delete from cmsStylesheet where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
                //base.delete();

                FireAfterDelete(e);
            }

        }

        public void saveCssToFile()
        {
            ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetItem);

            //if (this.Text.Contains('/'))
            //{
            //    var dir = string.Format("{0}/{1}", IOHelper.MapPath(SystemDirectories.Css), this.Text.Substring(0, this.Text.LastIndexOf('/')));
            //    if (!Directory.Exists(dir))
            //        Directory.CreateDirectory(dir);
            //}

            //using (StreamWriter SW = File.CreateText(IOHelper.MapPath(string.Format("{0}/{1}.css", SystemDirectories.Css, this.Text))))
            //{
            //    string tmpCss = this.Content ;
            //    tmpCss += "/* EDITOR PROPERTIES - PLEASE DON'T DELETE THIS LINE TO AVOID DUPLICATE PROPERTIES */\n";
            //    foreach (StylesheetProperty p in this.Properties)
            //    {
            //        tmpCss += p + "\n";
            //    }
            //    SW.Write(tmpCss);
            //}
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
            var found = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<string>(
                "SELECT TOP 1 filename FROM cmsStylesheet WHERE nodeid=" + id);
            if (found.IsNullOrWhiteSpace()) return null;

            var s = ApplicationContext.Current.Services.FileService.GetStylesheetByName(found);
            if (s == null) return null;

            return new StyleSheet(s);

            //return ApplicationContext.Current.ApplicationCache.GetCacheItem(
            //    GetCacheKey(id),
            //    TimeSpan.FromMinutes(30), () =>
            //        {
            //            try
            //            {
            //                return new StyleSheet(id, setupStyleProperties, loadContentFromFile);
            //            }
            //            catch
            //            {
            //                return null;
            //            }
            //        });
        }

        [Obsolete("Stylesheet cache is automatically invalidated by Umbraco when a stylesheet is saved or deleted")]
        public void InvalidateCache()
        {            
        }

        //private static string GetCacheKey(int id)
        //{
        //    return CacheKeys.StylesheetCacheKey + id;
        //}

        public static StyleSheet GetByName(string name)
        {
            var found = ApplicationContext.Current.Services.FileService.GetStylesheetByName(name.EndsWith(".css") ? name : name + ".css");
            if (found == null) return null;
            return new StyleSheet(found);
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
        public new static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public new static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterSave(SaveEventArgs e)
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
        public new static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public new static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        
    }

    [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
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
