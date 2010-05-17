using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Xml;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Umbraco.Test")]

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for DocumentType.
    /// </summary>
    public class DocumentType : ContentType
    {
        #region Constructors

        public DocumentType(int id) : base(id) { }

        public DocumentType(Guid id) : base(id) { }

        public DocumentType(int id, bool noSetup) : base(id, noSetup) { }

        #endregion

        #region Constants and Static members
        
        public static Guid _objectType = new Guid("a2cb7800-f571-4787-9638-bc48539a0efb");

        internal const string m_SQLOptimizedGetAll = @"
            SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text,
                masterContentType,Alias,icon,thumbnail,description,
                templateNodeId, IsDefault
            FROM umbracoNode 
            INNER JOIN cmsContentType ON umbracoNode.id = cmsContentType.nodeId
            LEFT OUTER JOIN cmsDocumentType ON cmsContentType.nodeId = cmsDocumentType.contentTypeNodeId
            WHERE nodeObjectType = @nodeObjectType";

        #endregion

        #region Private Members

        private ArrayList _templateIds = new ArrayList();
        private int _defaultTemplate;
        private bool _hasChildrenInitialized = false;
        private bool _hasChildren;

        #endregion

        #region Static Methods

        /// <summary>
        /// Generates the complete (simplified) XML DTD 
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public static string GenerateDtd()
        {
            StringBuilder dtd = new StringBuilder();
            // Renamed 'umbraco' to 'root' since the top level of the DOCTYPE should specify the name of the root node for it to be valid;
            // there's no mention of 'umbraco' anywhere in the schema that this DOCTYPE governs
            // (Alex N 20100212)
            dtd.AppendLine("<!DOCTYPE root [ ");

            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                dtd.AppendLine("<!ELEMENT node ANY> <!ATTLIST node id ID #REQUIRED>  <!ELEMENT data ANY>");
            }
            else
            {
                // TEMPORARY: Added Try-Catch to this call since trying to generate a DTD against a corrupt db
                // or a broken connection string is not handled yet
                // (Alex N 20100212)
                try
                {
                    StringBuilder strictSchemaBuilder = new StringBuilder();

                    List<DocumentType> dts = GetAllAsList();
                    foreach (DocumentType dt in dts)
                    {
                        string safeAlias = helpers.Casing.SafeAlias(dt.Alias);
                        if (safeAlias != null)
                        {
                            strictSchemaBuilder.AppendLine(String.Format("<!ELEMENT {0} ANY>", safeAlias));
                            strictSchemaBuilder.AppendLine(String.Format("<!ATTLIST {0} id ID #REQUIRED>", safeAlias));
                        }
                    }

                    // Only commit the strong schema to the container if we didn't generate an error building it
                    dtd.Append(strictSchemaBuilder);
                }
                catch (Exception exception)
                {
                    // Note, Log.Add quietly swallows the exception if it can't write to the database
                    Log.Add(LogTypes.System, -1, string.Format("{0} while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception.Message));
                }

            }
            dtd.AppendLine("]>");

            return dtd.ToString();
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
        public new static DocumentType[] GetAll
        {
            get
            {
                return GetAllAsList().ToArray();
            }
        }

        public static List<DocumentType> GetAllAsList()
        {         

            var documentTypes = new List<DocumentType>();

            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(m_SQLOptimizedGetAll, SqlHelper.CreateParameter("@nodeObjectType", DocumentType._objectType)))
            {
                while (dr.Read())
                {
                    //check if the document id has already been added
                    if (documentTypes.Where(x => x.Id == dr.Get<int>("id")).Count() == 0)
                    {
                        //create the DocumentType object without setting up
                        DocumentType dt = new DocumentType(dr.Get<int>("id"), true);
                        //populate it's CMSNode properties
                        dt.PopulateCMSNodeFromReader(dr);
                        //populate it's ContentType properties
                        dt.PopulateContentTypeNodeFromReader(dr);
                        //populate from it's DocumentType properties
                        dt.PopulateDocumentTypeNodeFromReader(dr);

                        documentTypes.Add(dt);
                    }
                    else
                    {
                        //we've already created the document type with this id, so we'll add the rest of it's templates to itself
                        var dt = documentTypes.Where(x => x.Id == dr.Get<int>("id")).Single();
                        dt.PopulateDocumentTypeNodeFromReader(dr);
                    }
                }
            }

            return documentTypes.OrderBy(x => x.Text).ToList();

        } 
        #endregion

        #region Public Properties
        public override bool HasChildren
        {
            get
            {
                if (!_hasChildrenInitialized)
                {
                    HasChildren = SqlHelper.ExecuteScalar<int>("select count(NodeId) as tmp from cmsContentType where masterContentType = " + Id) > 0;
                }
                return _hasChildren;
            }
            set
            {
                _hasChildrenInitialized = true;
                _hasChildren = value;
            }
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
        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException">Throws an exception if trying to delete a document type that is assigned as a master document type</exception>
        public override void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                // check that no document types uses me as a master
                foreach (DocumentType dt in DocumentType.GetAllAsList())
                {
                    if (dt.MasterContentType == this.Id)
                    {
                        //this should be InvalidOperationException (or something other than ArgumentException)!
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

            if (this.MasterContentType > 0)
            {
                DocumentType dt = new DocumentType(this.MasterContentType);

                if (dt != null)
                    info.AppendChild(xmlHelper.addTextNode(xd, "Master", dt.Alias));
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

            foreach (int cc in AllowedChildContentTypeIDs.ToList())
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
            foreach (TabI t in getVirtualTabs.ToList())
            {
                XmlElement tabx = xd.CreateElement("Tab");
                tabx.AppendChild(xmlHelper.addTextNode(xd, "Id", t.Id.ToString()));
                tabx.AppendChild(xmlHelper.addTextNode(xd, "Caption", t.Caption));
                tabs.AppendChild(tabx);
            }
            doc.AppendChild(tabs);
            return doc;
        }

        public void RemoveDefaultTemplate()
        {
            _defaultTemplate = 0;
            SqlHelper.ExecuteNonQuery("update cmsDocumentType set IsDefault = 0 where contentTypeNodeId = " +
                                      Id.ToString());
        }

        public bool HasTemplate()
        {
            return (_templateIds.Count > 0);
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

        #endregion

        #region Protected Methods

        protected void PopulateDocumentTypeNodeFromReader(IRecordsReader dr)
        {
            if (!dr.IsNull("templateNodeId"))
            {
                _templateIds.Add(dr.GetInt("templateNodeId"));
                if (!dr.IsNull("IsDefault"))
                {
                    if (dr.GetBoolean("IsDefault"))
                    {
                        _defaultTemplate = dr.GetInt("templateNodeId");
                    }
                }
            }            
        }

        protected override void setupNode()
        {
            base.setupNode();

            using (IRecordsReader dr = SqlHelper.ExecuteReader("Select templateNodeId, IsDefault from cmsDocumentType where contentTypeNodeId = @id",
                SqlHelper.CreateParameter("@id", Id)))
            {
                while (dr.Read())
                {
                    PopulateDocumentTypeNodeFromReader(dr);
                }
            }
             
        } 

        #endregion

        #region Private Methods

        [Obsolete("Use the overridden setupNode instead. This method now calls the setupNode method")]
        private void setupDocumentType()
        {
            setupNode();
        } 

        #endregion
        
        #region Events
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
        #endregion
    }
}