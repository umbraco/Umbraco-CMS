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
using Umbraco.Core.Services;
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
        internal Stylesheet StylesheetEntity;

        public static Guid ModuleObjectType = new Guid(Constants.ObjectTypes.Stylesheet);
        
        public string Filename
        {
            get
            {
                var path = StylesheetEntity.Path;
                if (path.IsNullOrWhiteSpace()) return string.Empty;
                return System.IO.Path.GetFileNameWithoutExtension(StylesheetEntity.Path);
            }
            set
            {
                //setting the file name changing it's path
                StylesheetEntity.Path = StylesheetEntity.Path.TrimEnd(StylesheetEntity.Name) + value.EnsureEndsWith(".css");
            }
        }

        public string Content
        {
            get { return StylesheetEntity.Content; }
            set { StylesheetEntity.Content = value; }
        }

        public StylesheetProperty[] Properties
        {
            get { return StylesheetEntity.Properties.Select(x => new StylesheetProperty(StylesheetEntity, x)).ToArray(); }
        }

        internal StyleSheet(Stylesheet stylesheet)
            : base(stylesheet)
        {
            if (stylesheet == null) throw new ArgumentNullException("stylesheet");
            StylesheetEntity = stylesheet;
        }

        public StyleSheet(Guid id)
            : base(id)
        {
        }

        public StyleSheet(int id)
            : base(id)
        {
        }

        [Obsolete("This constructors parameters: setupStyleProperties, loadContentFromFile don't do anything")]
        public StyleSheet(int id, bool setupStyleProperties, bool loadContentFromFile)
            : base(id)
        {
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
        [Obsolete("Level does nothing for Stylesheets")]
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
            get { return StylesheetEntity.CreateDate; }
            set { StylesheetEntity.CreateDate = value; }
        }
        
        /// <summary>
        /// Human readable name/label
        /// </summary>
        public override string Text
        {
            get { return Filename; }
            set { Filename = value; }
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
                ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetEntity);

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Sets up the internal data of the CMSNode, used by the various constructors
        /// </summary>
        protected override void setupNode()
        {
            ThrowNotSupported<StyleSheet>();
        }

        internal static void ThrowNotSupported<T>()
        {
            throw new NotSupportedException("The legacy " + typeof(T) + " API is no longer functional for retrieving stylesheets based on an integer ID. Stylesheets are no longer persisted in database tables. Use the new Umbraco.Core.Services.IFileSystem APIs instead of working with Umbraco stylesheets.");
        }
    
        public static StyleSheet MakeNew(BusinessLogic.User user, string Text, string FileName, string Content)
        {

            if (FileName.IsNullOrWhiteSpace())
            {
                FileName = Text.EnsureEndsWith(".css");
            }

            // validate if node ends with css, if it does we'll remove it as we append it later
            if (Text.ToLowerInvariant().EndsWith(".css"))
            {
                Text = Text.TrimEnd(".css");
            }

            var newSheet = new Stylesheet(FileName)
            {
                Content = Content
            };
            ApplicationContext.Current.Services.FileService.SaveStylesheet(newSheet);

            var newCss = new StyleSheet(newSheet);
            var e = new NewEventArgs();
            newCss.OnNew(e);

            return newCss;
        }

        public static StyleSheet[] GetAll()
        {
            var retval = ApplicationContext.Current.Services.FileService.GetStylesheets()
                //Legacy would appear to have only ever loaded the stylesheets at the root level (no sub folders)
                .Where(x => x.Path.Split(new char[]{'\\'}, StringSplitOptions.RemoveEmptyEntries).Count() == 1)
                .OrderBy(x => x.Alias)
                .Select(x => new StyleSheet(x))
                .ToArray();

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
                ApplicationContext.Current.Services.FileService.DeleteStylesheet(StylesheetEntity.Path);

                FireAfterDelete(e);
            }

        }

        public void saveCssToFile()
        {
            ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetEntity);
        }

        public XmlNode ToXml(XmlDocument xd)
        {
            var serializer = new EntityXmlSerializer();
            var xml = serializer.Serialize(StylesheetEntity);
            return xml.GetXmlNode(xd);
        }

        public static StyleSheet GetStyleSheet(int id, bool setupStyleProperties, bool loadContentFromFile)
        {
            ThrowNotSupported<StyleSheet>();
            return null;
        }

        [Obsolete("Stylesheet cache is automatically invalidated by Umbraco when a stylesheet is saved or deleted")]
        public void InvalidateCache()
        {            
        }

        public static StyleSheet GetByName(string name)
        {
            var found = ApplicationContext.Current.Services.FileService.GetStylesheetByName(name.EnsureEndsWith(".css"));
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
                if (sp == null)
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
