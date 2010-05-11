using System;
using System.Data;

using System.Collections;
using umbraco.DataLayer;
using System.Xml;

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
		private Guid _controlId;
		
		private static Guid _objectType = new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c");


		/// <summary>
		/// Initialization of the datatypedefinition
		/// </summary>
		/// <param name="id">Datattypedefininition id</param>
		public DataTypeDefinition(int id) : base(id)
		{
			setupDataTypeDefinition();
		}

        public override void delete()
        {
            //delete the cmsDataType role, then the umbracoNode
            SqlHelper.ExecuteNonQuery("delete from cmsDataType where nodeId=@nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
            base.delete();

            cache.Cache.ClearCacheItem(string.Format("UmbracoDataTypeDefinition{0}", Id));
        }

        [Obsolete("Use the standard delete() method instead")]
        public void Delete()
        {
            delete();
        }

	    /// <summary>
		/// Initialization of the datatypedefinition
		/// </summary>
		/// <param name="id">Datattypedefininition id</param>
		public DataTypeDefinition(Guid id) : base(id)
		{
			setupDataTypeDefinition();
		}


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            OnSaving(EventArgs.Empty);
        }



		private void setupDataTypeDefinition() {
			IRecordsReader dr = SqlHelper.ExecuteReader( "select dbType, controlId from cmsDataType where nodeId = '" + this.Id.ToString() + "'");
			if (dr.Read()) 
			{
				_controlId = dr.GetGuid("controlId");
			} 
			else
				throw new ArgumentException("No dataType with id = " + this.Id.ToString() + " found");
			dr.Close();
		}


		/// <summary>
		/// The associated datatype, which delivers the methods for editing data, editing prevalues see: umbraco.interfaces.IDataType
		/// </summary>
		public interfaces.IDataType DataType
		{
			get {
				cms.businesslogic.datatype.controls.Factory f = new cms.businesslogic.datatype.controls.Factory();
				interfaces.IDataType dt = f.DataType(_controlId);
				dt.DataTypeDefinitionId = Id;
				
				return dt;
				}
			set 
			{
				SqlHelper.ExecuteNonQuery( "update cmsDataType set controlId = @id where nodeID = " + this.Id.ToString(),
                    SqlHelper.CreateParameter("@id",value.Id));
				_controlId = value.Id;
			}
		}


        public XmlElement ToXml(XmlDocument xd) {
            XmlElement dt = xd.CreateElement("DataType");
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "Name", Text));
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "Id", this.DataType.Id.ToString()));
            dt.Attributes.Append(xmlHelper.addAttribute(xd, "Definition", this.UniqueId.ToString()));
            
            // templates
            XmlElement prevalues = xd.CreateElement("PreValues");
            foreach (DictionaryEntry item in PreValues.GetPreValues(this.Id)) {
                XmlElement prevalue = xd.CreateElement("PreValue");
                prevalue.Attributes.Append(xmlHelper.addAttribute(xd, "Id", ((umbraco.cms.businesslogic.datatype.PreValue) item.Value).Id.ToString() ));
                prevalue.Attributes.Append(xmlHelper.addAttribute(xd, "Value", ((umbraco.cms.businesslogic.datatype.PreValue) item.Value).Value ));

                prevalues.AppendChild(prevalue);
            }

            dt.AppendChild(prevalues);
            
            return dt;
        }

        public static DataTypeDefinition Import(XmlNode xmlData) {
            string _name = xmlData.Attributes["Name"].Value;
            string _id = xmlData.Attributes["Id"].Value;
            string _def = xmlData.Attributes["Definition"].Value;


            //Make sure that the dtd is not already present
            if (!CMSNode.IsNode(new Guid(_def))
            ) {
                
                BasePages.UmbracoEnsuredPage uep = new umbraco.BasePages.UmbracoEnsuredPage();
                BusinessLogic.User u = uep.getUser();
                
                if(u == null)
                    u = BusinessLogic.User.GetUser(0);

                cms.businesslogic.datatype.controls.Factory f = new umbraco.cms.businesslogic.datatype.controls.Factory();


                DataTypeDefinition dtd = MakeNew(u, _name, new Guid(_def));
                dtd.DataType = f.DataType(new Guid(_id));
                dtd.Save();

                //add prevalues
                foreach (XmlNode xmlPv in xmlData.SelectNodes("PreValues/PreValue"))
                {


                    XmlAttribute val = xmlPv.Attributes["Value"];

                    if (val != null && !string.IsNullOrEmpty(val.Value)) {
                        PreValue p = new PreValue(0, 0, val.Value);
                        p.DataTypeId = dtd.Id;
                        p.Save();
                    }
                }

                return dtd;  
            }

            return null;
        }


		/*
		public SortedList PreValues {
			get {
				SortedList retVal = new SortedList();
				SqlDataReader dr = SqlHelper.ExecuteReader("select id, value from cmsDataTypePreValues where dataTypeNodeId = @nodeId order by sortOrder", SqlHelper.CreateParameter("@nodeId", this.Id));
				while (dr.Read()) 
				{
					retVal.Add(dr.GetString("id"), dr.GetString("value"));
				}
				dr.Close();

				return retVal;
				}
		}
		*/

		/// <summary>
		/// Retrieves a list of all datatypedefinitions
		/// </summary>
		/// <returns>A list of all datatypedefinitions</returns>
		public static DataTypeDefinition[] GetAll() 
		{
			SortedList retvalSort = new SortedList();
            Guid[] tmp = CMSNode.getAllUniquesFromObjectType(_objectType);
			DataTypeDefinition[] retval = new DataTypeDefinition[tmp.Length];
			for(int i = 0; i < tmp.Length; i++) {
				DataTypeDefinition dt = DataTypeDefinition.GetDataTypeDefinition(tmp[i]);
				retvalSort.Add(dt.Text + "|||" + Guid.NewGuid().ToString(), dt);
			}

			IDictionaryEnumerator ide = retvalSort.GetEnumerator();
			int counter = 0;
			while (ide.MoveNext()) 
			{
				retval[counter] = (DataTypeDefinition) ide.Value;
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
        public static DataTypeDefinition MakeNew(BusinessLogic.User u, string Text, Guid UniqueId) {

            int newId = CMSNode.MakeNew(-1, _objectType, u.Id, 1, Text, UniqueId).Id;
            cms.businesslogic.datatype.controls.Factory f = new cms.businesslogic.datatype.controls.Factory();
            Guid FirstcontrolId = f.GetAll()[0].Id;
            SqlHelper.ExecuteNonQuery("Insert into cmsDataType (nodeId, controlId, dbType) values (" + newId.ToString() + ",@controlId,'Ntext')",
                SqlHelper.CreateParameter("@controlId", FirstcontrolId));

            DataTypeDefinition dtd = new DataTypeDefinition(newId);
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
			foreach(DataTypeDefinition df in DataTypeDefinition.GetAll())
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
            if (System.Web.HttpRuntime.Cache[string.Format("UmbracoDataTypeDefinition{0}", id.ToString())] == null)
            {
                DataTypeDefinition dt = new DataTypeDefinition(id);
                System.Web.HttpRuntime.Cache.Insert(string.Format("UmbracoDataTypeDefinition{0}", id.ToString()), dt);
            }
            return (DataTypeDefinition)System.Web.HttpRuntime.Cache[string.Format("UmbracoDataTypeDefinition{0}", id.ToString())];
        }

        [Obsolete("Use GetDataTypeDefinition(int id) instead", false)]
        public static DataTypeDefinition GetDataTypeDefinition(Guid id)
        {
            if (System.Web.HttpRuntime.Cache[string.Format("UmbracoDataTypeDefinition{0}", id.ToString())] == null)
            {
                DataTypeDefinition dt = new DataTypeDefinition(id);
                System.Web.HttpRuntime.Cache.Insert(string.Format("UmbracoDataTypeDefinition{0}", id.ToString()), dt);
            }
            return (DataTypeDefinition)System.Web.HttpRuntime.Cache[string.Format("UmbracoDataTypeDefinition{0}", id.ToString())];
        }


        //EVENTS
        public delegate void SaveEventHandler(DataTypeDefinition sender, EventArgs e);
        public delegate void NewEventHandler(DataTypeDefinition sender, EventArgs e);
        public delegate void DeleteEventHandler(DataTypeDefinition sender, EventArgs e);

        /// <summary>
        /// Occurs when a macro is saved.
        /// </summary>
        public static event SaveEventHandler Saving;
        protected virtual void OnSaving(EventArgs e) {
            if (Saving != null)
                Saving(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(EventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler Deleting;
        protected virtual void OnDeleting(EventArgs e) {
            if (Deleting != null)
                Deleting(this, e);
        }

    }
}