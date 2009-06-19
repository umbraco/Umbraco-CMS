using System;
using System.Collections;
using System.Data;
using System.Xml;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for DocumentType.
    /// </summary>
    public class DocumentType : ContentType
    {
        private static Guid _objectType = new Guid("a2cb7800-f571-4787-9638-bc48539a0efb");
        private ArrayList _templateIds = new ArrayList();
        private int _defaultTemplate;

        public DocumentType(int id)
            : base(id)
        {
            setupDocumentType();
        }

        public DocumentType(Guid id)
            : base(id)
        {
            setupDocumentType();
        }

        public DocumentType(int id, bool UseOptimizedMode)
            : base(id, UseOptimizedMode)
        {
            // Only called if analyze hasn't happend yet
            AnalyzeContentTypes(_objectType, false);

            // Check if this document type can run in optimized mode
            if (IsOptimized())
            {
                OptimizedMode = true;

                // Run optimized sql query here 
            }
            else
            {
                base.setupNode();
                base.setupContentType();
                OptimizedMode = false;
            }
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) {
                base.Save();
                FireAfterSave(e);
            }
        }

        public new static DocumentType GetByAlias(string Alias)
        {
            try
            {
                return
                    new DocumentType(
                            SqlHelper.ExecuteScalar<int>("SELECT nodeid from cmsContentType where alias = @alias",
                                                         SqlHelper.CreateParameter("@alias", Alias)));
            }
            catch
            {
                return null;
            }
        }

        private void setupDocumentType()
        {
            if (SqlHelper.ExecuteScalar<int>("select count(TemplateNodeId) as tmp from cmsDocumentType where contentTypeNodeId =" + Id) > 0)
            {
                IRecordsReader dr =
                    SqlHelper.ExecuteReader(
                                            "Select templateNodeId, IsDefault from cmsDocumentType where contentTypeNodeId =" +
                                            Id);
                while (dr.Read())
                {
                    if (template.Template.IsNode(dr.GetInt("templateNodeId")))
                    {
                        _templateIds.Add(dr.GetInt("templateNodeId"));
                        if (dr.GetBoolean("IsDefault"))
                            _defaultTemplate = dr.GetInt("templateNodeId");
                    }
                }
                dr.Close();
            }
        }

        public override bool HasChildren
        {
            get
            {
                return
                    SqlHelper.ExecuteScalar<int>("select count(NodeId) as tmp from cmsContentType where masterContentType = " + Id) > 0;

            }
            set
            {
                base.HasChildren = value;
            }
        }

        public static DocumentType MakeNew(User u, string Text)
        {
            int ParentId = -1;
            int level = 1;
            Guid uniqueId = Guid.NewGuid();
            CMSNode n = MakeNew(ParentId, _objectType, u.Id, level, Text, uniqueId);

            Create(n.Id, Text, "");
            DocumentType newDt = new DocumentType(n.Id);

            //event
            NewEventArgs e = new NewEventArgs();
            newDt.OnNew(e);

            return newDt;
        }

        [Obsolete("Use GetAllAsList() method call instead", true)]
        public new static DocumentType[] GetAll {
            get {
                return GetAllAsList().ToArray();
            }
        }
        
        public static List<DocumentType> GetAllAsList()
        {
            List<DocumentType> retVal = new List<DocumentType>();
            Guid[] Ids = getAllUniquesFromObjectType(_objectType);
            for (int i = 0; i < Ids.Length; i++)
            {
                retVal.Add(new DocumentType(Ids[i]));
            }

            retVal.Sort(delegate(DocumentType dt1, DocumentType dt2) { return dt1.Text.CompareTo(dt2.Text); });
            return retVal;
        }

        public bool HasTemplate()
        {
            return (_templateIds.Count > 0);
        }

        public int DefaultTemplate
        {
            get { return _defaultTemplate; }
            set
            {
                RemoveDefaultTemplate();
                _defaultTemplate = value;
                if (_defaultTemplate != 0)
                    SqlHelper.ExecuteNonQuery("update cmsDocumentType set IsDefault = 1 where contentTypeNodeId = " +
                                              Id.ToString() + " and TemplateNodeId = " + value.ToString());
            }
        }

        public void RemoveDefaultTemplate()
        {
            _defaultTemplate = 0;
            SqlHelper.ExecuteNonQuery("update cmsDocumentType set IsDefault = 0 where contentTypeNodeId = " +
                                      Id.ToString());
        }

        public template.Template[] allowedTemplates
        {
            get
            {
                if (HasTemplate())
                {
                    template.Template[] retval = new template.Template[_templateIds.Count];
                    for (int i = 0; i < _templateIds.Count; i++)
                    {
                        retval[i] = template.Template.GetTemplate((int)_templateIds[i]);
                    }
                    return retval;
                }
                return new template.Template[0];
            }
            set
            {
                clearTemplates();
                foreach (template.Template t in value)
                {
                    SqlHelper.ExecuteNonQuery("Insert into cmsDocumentType (contentTypeNodeId, templateNodeId) values (" +
                                              Id + "," + t.Id + ")");
                    _templateIds.Add(t.Id);
                }
            }
        }

        public void clearTemplates()
        {
            SqlHelper.ExecuteNonQuery("Delete from cmsDocumentType where contentTypeNodeId =" + Id);
            _templateIds.Clear();
        }

        public XmlElement ToXml(XmlDocument xd)
        {
            XmlElement doc = xd.CreateElement("DocumentType");

            // info section
            XmlElement info = xd.CreateElement("Info");
            doc.AppendChild(info);
            info.AppendChild(xmlHelper.addTextNode(xd, "Name", Text));
            info.AppendChild(xmlHelper.addTextNode(xd, "Alias", Alias));
            info.AppendChild(xmlHelper.addTextNode(xd, "Icon", IconUrl));
            info.AppendChild(xmlHelper.addTextNode(xd, "Thumbnail", Thumbnail));
            info.AppendChild(xmlHelper.addTextNode(xd, "Description", Description));

            if (this.MasterContentType > 0) {
                DocumentType dt = new DocumentType(this.MasterContentType);

                if(dt != null)
                    info.AppendChild( xmlHelper.addTextNode(xd, "Master", dt.Alias));
            }
            
            
            // templates
            XmlElement allowed = xd.CreateElement("AllowedTemplates");
            foreach (template.Template t in allowedTemplates)
                allowed.AppendChild(xmlHelper.addTextNode(xd, "Template", t.Alias));
            info.AppendChild(allowed);
            if (DefaultTemplate != 0)
                info.AppendChild(
                    xmlHelper.addTextNode(xd, "DefaultTemplate", new template.Template(DefaultTemplate).Alias));
            else
                info.AppendChild(xmlHelper.addTextNode(xd, "DefaultTemplate", ""));

            // structure
            XmlElement structure = xd.CreateElement("Structure");
            doc.AppendChild(structure);

            foreach (int cc in AllowedChildContentTypeIDs)
                structure.AppendChild(xmlHelper.addTextNode(xd, "DocumentType", new DocumentType(cc).Alias));

            // generic properties
            XmlElement pts = xd.CreateElement("GenericProperties");
            foreach (PropertyType pt in PropertyTypes)
            {
                XmlElement ptx = xd.CreateElement("GenericProperty");
                ptx.AppendChild(xmlHelper.addTextNode(xd, "Name", pt.Name));
                ptx.AppendChild(xmlHelper.addTextNode(xd, "Alias", pt.Alias));
                ptx.AppendChild(xmlHelper.addTextNode(xd, "Type", pt.DataTypeDefinition.DataType.Id.ToString()));

                //Datatype definition guid was added in v4 to enable datatype imports
                ptx.AppendChild(xmlHelper.addTextNode(xd, "Definition", pt.DataTypeDefinition.UniqueId.ToString()));

                ptx.AppendChild(xmlHelper.addTextNode(xd, "Tab", Tab.GetCaptionById(pt.TabId)));
                ptx.AppendChild(xmlHelper.addTextNode(xd, "Mandatory", pt.Mandatory.ToString()));
                ptx.AppendChild(xmlHelper.addTextNode(xd, "Validation", pt.ValidationRegExp));
                ptx.AppendChild(xmlHelper.addCDataNode(xd, "Description", pt.Description));
                pts.AppendChild(ptx);
            }
            doc.AppendChild(pts);

            // tabs
            XmlElement tabs = xd.CreateElement("Tabs");
            foreach (TabI t in getVirtualTabs)
            {
                XmlElement tabx = xd.CreateElement("Tab");
                tabx.AppendChild(xmlHelper.addTextNode(xd, "Id", t.Id.ToString()));
                tabx.AppendChild(xmlHelper.addTextNode(xd, "Caption", t.Caption));
                tabs.AppendChild(tabx);
            }
            doc.AppendChild(tabs);
            return doc;
        }

        public new void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel) {
                // check that no document types uses me as a master
                foreach (DocumentType dt in DocumentType.GetAllAsList()) {
                    if (dt.MasterContentType == this.Id) {
                        throw new ArgumentException("Can't delete a Document Type used as a Master Content Type. Please remove all references first!");
                    }
                }

                // delete all documents of this type
                Document.DeleteFromType(this);
                // Delete contentType
                base.delete();

                FireAfterDelete(e);
            }
        }

        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(DocumentType sender, SaveEventArgs e);
        /// <summary>
        /// The New event handler
        /// </summary>
        public delegate void NewEventHandler(DocumentType sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(DocumentType sender, DeleteEventArgs e);


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
    }
}