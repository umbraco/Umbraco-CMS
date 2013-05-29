using System;
using System.Collections;
using System.Linq;
using Umbraco.Core.Cache;
using umbraco.DataLayer;
using System.Xml;
using umbraco.cms.businesslogic.media;
using umbraco.interfaces;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;
using Umbraco.Core;

namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// Datatypedefinitions is the basic buildingblocks of umbraco's documents/medias/members generic datastructure 
    /// 
    /// A datatypedefinition encapsulates an object which implements the interface IDataType, and are used when defining
    /// the properties of a document in the documenttype. This extra layer between IDataType and a documenttypes propertytype
    /// are used amongst other for enabling shared prevalues.
    /// 
    /// </summary>
    public class DataTypeDefinition : CMSNode
    {
        #region Private fields
        private Guid _controlId;

        private static Guid _objectType = new Guid(Constants.ObjectTypes.DataType);
	    private string _dbType;

	    #endregion

        #region Constructors

        /// <summary>
        /// Initialization of the datatypedefinition
        /// </summary>
        /// <param name="id">Datattypedefininition id</param>
        public DataTypeDefinition(int id) : base(id) { }

        /// <summary>
        /// Initialization of the datatypedefinition
        /// </summary>
        /// <param name="id">Datattypedefininition id</param>
        public DataTypeDefinition(Guid id) : base(id) { }

        #endregion

        #region Public Properties
        /// <summary>
        /// The associated datatype, which delivers the methods for editing data, editing prevalues see: umbraco.interfaces.IDataType
        /// </summary>
        public IDataType DataType
        {
            get
            {
                if (_controlId == Guid.Empty) 
                    return null;

                var dt = DataTypesResolver.Current.GetById(_controlId);

                if (dt != null)
                    dt.DataTypeDefinitionId = Id;

                return dt;
            }
            set
            {
                if (SqlHelper == null)
                    throw new InvalidOperationException("Cannot execute a SQL command when the SqlHelper is null");
                if (value == null)
                    throw new InvalidOperationException("The value passed in is null. The DataType property cannot be set to a null value");

                SqlHelper.ExecuteNonQuery("update cmsDataType set controlId = @id where nodeID = " + this.Id.ToString(),
                    SqlHelper.CreateParameter("@id", value.Id));
                _controlId = value.Id;
            }
        } 
	    internal string DbType
	    {
            get { return _dbType; }
        } 
        #endregion

        #region Public methods
        public override void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);
            if (!e.Cancel)
            {
                //first clear the prevalues
                PreValues.DeleteByDataTypeDefinition(this.Id);

                //next clear out the property types
                var propTypes = PropertyType.GetByDataTypeDefinition(this.Id);
                foreach (var p in propTypes)
                {
                    p.delete();
                }

                //delete the cmsDataType role, then the umbracoNode
                SqlHelper.ExecuteNonQuery("delete from cmsDataType where nodeId=@nodeId",
                                          SqlHelper.CreateParameter("@nodeId", this.Id));
                base.delete();

                
                FireAfterDelete(e);
            }
        }

        [Obsolete("Use the standard delete() method instead")]
        public void Delete()
        {
            delete();
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            OnSaving(EventArgs.Empty);
        }

        public XmlElement ToXml(XmlDocument xd)
        {
            XmlElement dt = xd.CreateElement("DataType");
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "Name", Text));
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "Id", this.DataType.Id.ToString()));
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "Definition", this.UniqueId.ToString()));
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "DatabaseType", this.DbType));

            // templates
            XmlElement prevalues = xd.CreateElement("PreValues");
            foreach (DictionaryEntry item in PreValues.GetPreValues(this.Id))
            {
                XmlElement prevalue = xd.CreateElement("PreValue");
                prevalue.Attributes.Append(xmlHelper.addAttribute(xd, "Id", ((PreValue)item.Value).Id.ToString()));
                prevalue.Attributes.Append(xmlHelper.addAttribute(xd, "Value", ((PreValue)item.Value).Value));

                prevalues.AppendChild(prevalue);
            }

            dt.AppendChild(prevalues);

            return dt;
        }
        #endregion

        #region Static methods
        public static DataTypeDefinition Import(XmlNode xmlData)
        {
            string _name = xmlData.Attributes["Name"].Value;
            string _id = xmlData.Attributes["Id"].Value;
            string _def = xmlData.Attributes["Definition"].Value;


            //Make sure that the dtd is not already present
            if (IsNode(new Guid(_def)) == false)
            {
                var u = BusinessLogic.User.GetCurrent() ?? BusinessLogic.User.GetUser(0);

                var dtd = MakeNew(u, _name, new Guid(_def));
                var dataType = DataTypesResolver.Current.GetById(new Guid(_id));
                if (dataType == null)
                    throw new NullReferenceException("Could not resolve a data type with id " + _id);

                dtd.DataType = dataType;
                dtd.Save();

                //add prevalues
                foreach (XmlNode xmlPv in xmlData.SelectNodes("PreValues/PreValue"))
                {
                    XmlAttribute val = xmlPv.Attributes["Value"];

                    if (val != null)
                    {
                        PreValue p = new PreValue(0, 0, val.Value);
                        p.DataTypeId = dtd.Id;
                        p.Save();
                    }
                }

                return dtd;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of all datatypedefinitions
        /// </summary>
        /// <returns>A list of all datatypedefinitions</returns>
        public static DataTypeDefinition[] GetAll()
        {
            SortedList retvalSort = new SortedList();
            Guid[] tmp = CMSNode.getAllUniquesFromObjectType(_objectType);
            DataTypeDefinition[] retval = new DataTypeDefinition[tmp.Length];
            for (int i = 0; i < tmp.Length; i++)
            {
                DataTypeDefinition dt = DataTypeDefinition.GetDataTypeDefinition(tmp[i]);
                retvalSort.Add(dt.Text + "|||" + Guid.NewGuid().ToString(), dt);
            }

            IDictionaryEnumerator ide = retvalSort.GetEnumerator();
            int counter = 0;
            while (ide.MoveNext())
            {
                retval[counter] = (DataTypeDefinition)ide.Value;
                counter++;
            }
            return retval;
        }

        /// <summary>
        /// Creates a new datatypedefinition given its name and the user which creates it.
        /// </summary>
        /// <param name="u">The user who creates the datatypedefinition</param>
        /// <param name="Text">The name of the DataTypeDefinition</param>
        /// <returns></returns>
        public static DataTypeDefinition MakeNew(BusinessLogic.User u, string Text)
        {
            return MakeNew(u, Text, Guid.NewGuid());
        }

        /// <summary>
        /// Creates a new datatypedefinition given its name and the user which creates it.
        /// </summary>
        /// <param name="u">The user who creates the datatypedefinition</param>
        /// <param name="Text">The name of the DataTypeDefinition</param>
        /// <param name="UniqueId">Overrides the CMSnodes uniqueID</param>
        /// <returns></returns>
        public static DataTypeDefinition MakeNew(BusinessLogic.User u, string Text, Guid UniqueId)
        {

            var newId = MakeNew(-1, _objectType, u.Id, 1, Text, UniqueId).Id;

            // initial control id changed to empty to ensure that it'll always work no matter if 3rd party configurators fail
            // ref: http://umbraco.codeplex.com/workitem/29788
            var firstcontrolId = Guid.Empty;

            SqlHelper.ExecuteNonQuery("Insert into cmsDataType (nodeId, controlId, dbType) values (" + newId.ToString() + ",@controlId,'Ntext')",
                SqlHelper.CreateParameter("@controlId", firstcontrolId));

            var dtd = new DataTypeDefinition(newId);
            dtd.OnNew(EventArgs.Empty);

            return dtd;
        }

        /// <summary>
        /// Retrieve a list of datatypedefinitions which share the same IDataType datatype
        /// </summary>
        /// <param name="DataTypeId">The unique id of the IDataType</param>
        /// <returns>A list of datatypedefinitions which are based on the IDataType specified</returns>
        public static DataTypeDefinition GetByDataTypeId(Guid DataTypeId)
        {
            int dfId = 0;
            // When creating a datatype and not saving it, it will be null, so we need this check
            foreach (DataTypeDefinition df in DataTypeDefinition.GetAll().Where(x => x.DataType != null))
                if (df.DataType.Id == DataTypeId)
                {
                    dfId = df.Id;
                    break;
                }

            if (dfId == 0)
                return null;
            else
                return new DataTypeDefinition(dfId);
        }

        /// <summary>
        /// Analyzes an object to see if its basetype is umbraco.editorControls.DefaultData
        /// </summary>
        /// <param name="Data">The Data object to analyze</param>
        /// <returns>True if the basetype is the DefaultData class</returns>
        public static bool IsDefaultData(object Data)
        {
            Type typeOfData = Data.GetType();

            while (typeOfData.BaseType != new Object().GetType())
                typeOfData = typeOfData.BaseType;

            return (typeOfData.FullName == "umbraco.cms.businesslogic.datatype.DefaultData");
        }

        public static DataTypeDefinition GetDataTypeDefinition(int id)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                string.Format("{0}{1}", CacheKeys.DataTypeCacheKey, id),
                () => new DataTypeDefinition(id));
        }

        [Obsolete("Use GetDataTypeDefinition(int id) instead", false)]
        public static DataTypeDefinition GetDataTypeDefinition(Guid id)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                string.Format("{0}{1}", CacheKeys.DataTypeCacheKey, id),
                () => new DataTypeDefinition(id));
        }
        #endregion

        #region Protected methods
        protected override void setupNode()
        {
            base.setupNode();

            using (IRecordsReader dr = SqlHelper.ExecuteReader("select dbType, controlId from cmsDataType where nodeId = '" + this.Id.ToString() + "'"))
            {
                if (dr.Read())
                {
                    _controlId = dr.GetGuid("controlId");
                    _dbType = dr.GetString("dbType");
                }
                else
                    throw new ArgumentException("No dataType with id = " + this.Id.ToString() + " found");
            }

        }
        #endregion

        #region Events
        //EVENTS
        public delegate void SaveEventHandler(DataTypeDefinition sender, EventArgs e);
        public delegate void NewEventHandler(DataTypeDefinition sender, EventArgs e);
        public delegate void DeleteEventHandler(DataTypeDefinition sender, EventArgs e);

        /// <summary>
        /// Occurs when a data type is saved.
        /// </summary>
        public static event SaveEventHandler Saving;
        protected virtual void OnSaving(EventArgs e)
        {
            if (Saving != null)
                Saving(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(EventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        [Obsolete("This event is not used! Use the BeforeDelete or AfterDelete events")]
        public static event DeleteEventHandler Deleting;
        protected virtual void OnDeleting(EventArgs e)
        {
            if (Deleting != null)
                Deleting(this, e);
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public new static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireBeforeDelete(DeleteEventArgs e)
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
        protected new virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        #endregion

    }
}