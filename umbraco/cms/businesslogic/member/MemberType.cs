using System;
using System.Data;
using System.Xml;
using umbraco.cms.businesslogic.propertytype;
using System.Linq;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.member
{
	/// MemberType
	/// 
	/// Due to the inheritance of the ContentType class, it enables definition of generic datafields on a Members.
	/// 
	public class MemberType : ContentType
	{
		private static Guid _objectType = new Guid("9b5416fb-e72f-45a9-a07b-5a9a2709ce43");

		/// <summary>
		/// Initializes a new instance of the MemberType class.
		/// </summary>
		/// <param name="id">MemberType id</param>
        public MemberType(int id) : base(id) { }

		/// <summary>
		/// Initializes a new instance of the MemberType class.
		/// </summary>
		/// <param name="id">MemberType id</param>
        public MemberType(Guid id) : base(id) { }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if(!e.Cancel){
                FireAfterSave(e);
            }
        }
        
		/// <summary>
		/// Create a new MemberType
		/// </summary>
		/// <param name="Text">The name of the MemberType</param>
		/// <param name="u">Creator of the MemberType</param>
		public static MemberType MakeNew(User u,string Text) 
		{		
			int ParentId= -1;
			int level = 1;
			Guid uniqueId = Guid.NewGuid();
			CMSNode n = CMSNode.MakeNew(ParentId, _objectType, u.Id, level,Text, uniqueId);

			ContentType.Create(n.Id, Text,"");
	        MemberType mt = new MemberType(n.Id);
		    mt.IconUrl = "member.gif";
            NewEventArgs e = new NewEventArgs();
            mt.OnNew(e);

            return mt; 
		}

		/// <summary>
		/// Retrieve a list of all MemberTypes
		/// </summary>
        public new static MemberType[] GetAll
        {
            get
            {
                Guid[] Ids = CMSNode.getAllUniquesFromObjectType(_objectType);

                MemberType[] retVal = new MemberType[Ids.Length];
                for (int i = 0; i < Ids.Length; i++) retVal[i] = new MemberType(Ids[i]);
                return retVal;
            }
        }

		/// <summary>
		/// Get an true/false if the Member can edit the given data defined in the propertytype
		/// </summary>
		/// <param name="pt">Propertytype to edit</param>
		/// <returns>True if the Member can edit the data</returns>
        public bool MemberCanEdit(PropertyType pt)
        {
            if (propertyTypeRegistered(pt))
            {
                var memberCanEdit = SqlHelper.ExecuteScalar<object>("Select memberCanEdit from cmsMemberType where NodeId = " + this.Id + " And propertytypeId = " + pt.Id);
                return (Convert.ToBoolean(memberCanEdit));
            }
            return false;
        }

		/// <summary>
		/// Get a MemberType by it's alias
		/// </summary>
		/// <param name="Alias">The alias of the MemberType</param>
		/// <returns>The MemberType with the given Alias</returns>
        public new static MemberType GetByAlias(string Alias)
        {            
            try
            {
                return
                    new MemberType(
                            SqlHelper.ExecuteScalar<int>(@"SELECT nodeid from cmsContentType INNER JOIN umbracoNode on cmsContentType.nodeId = umbracoNode.id WHERE nodeObjectType=@nodeObjectType AND alias=@alias",
                                SqlHelper.CreateParameter("@nodeObjectType", MemberType._objectType),
                                SqlHelper.CreateParameter("@alias", Alias)));
            }
            catch
            {
                return null;
            }
        }


		/// <summary>
		/// Get an true/false if the given data defined in the propertytype, should be visible on the members profile page
		/// </summary>
		/// <param name="pt">Propertytype</param>
		/// <returns>True if the data should be displayed on the profilepage</returns>
		public bool ViewOnProfile(PropertyType pt) 
		{
			if(propertyTypeRegistered(pt)) 
			{
                return Convert.ToBoolean(SqlHelper.ExecuteScalar<object>("Select viewOnProfile from cmsMemberType where NodeId = " + this.Id + " And propertytypeId = " + pt.Id));
			}
			return false;
		}
		
		/// <summary>
		/// Set if the member should be able to edit the data defined by its propertytype
		/// </summary>
		/// <param name="pt">PropertyType</param>
		/// <param name="value">True/False if Members of the type shoúld be able to edit the data</param>
        public void setMemberCanEdit(PropertyType pt, bool value)
        {
            int tmpval = 0;
            if (value) tmpval = 1;
            if (propertyTypeRegistered(pt))
                SqlHelper.ExecuteNonQuery("Update cmsMemberType set memberCanEdit = " + tmpval + " where NodeId = " + this.Id + " And propertytypeId = " + pt.Id);
            else
                SqlHelper.ExecuteNonQuery("insert into cmsMemberType (NodeId, propertytypeid, memberCanEdit,viewOnProfile) values (" + this.Id + "," + pt.Id + ", " + tmpval + ",0)");

        }

        private bool propertyTypeRegistered(PropertyType pt)
        {
            return (SqlHelper.ExecuteScalar<int>("Select count(pk) as tmp from cmsMemberType where NodeId = " + this.Id + " And propertytypeId = " + pt.Id) > 0);
        }


		/// <summary>
		/// Set if the data should be displayed on members of this type's profilepage
		/// </summary>
		/// <param name="pt">PropertyType</param>
		/// <param name="value">True/False if the data should be displayed</param>
        public void setMemberViewOnProfile(PropertyType pt, bool value) 
		{
			int tmpval = 0;
			if (value) tmpval = 1;
			if (propertyTypeRegistered(pt))
				SqlHelper.ExecuteNonQuery("Update cmsMemberType set viewOnProfile = " + tmpval + " where NodeId = " + this.Id +" And propertytypeId = "+pt.Id);
			else
				SqlHelper.ExecuteNonQuery("insert into cmsMemberType (NodeId, propertytypeid, viewOnProfile) values ("+this.Id+","+pt.Id+", "+tmpval+")");
		}

		/// <summary>
		/// Delete the current MemberType.
		/// 
		/// Deletes all Members of the type
		/// 
		/// Use with care
		/// </summary>
		public override void delete() 
		{
            DeleteEventArgs e = new DeleteEventArgs();

            FireBeforeDelete(e);

            if (!e.Cancel) {

                // delete all documents of this type
                Member.DeleteFromType(this);

                // delete membertype specific data
                SqlHelper.ExecuteNonQuery("Delete from cmsMemberType where nodeId = " + this.Id);

                // Delete contentType
                base.delete();
                FireAfterDelete(e);
            }
        }
        
        public XmlElement ToXml(XmlDocument xd)
        {
            XmlElement root = xd.CreateElement("MemberType");

            var info = xd.CreateElement("Info");
            root.AppendChild(info);
            info.AppendChild(xmlHelper.addTextNode(xd, "Name", this.Text));
            info.AppendChild(xmlHelper.addTextNode(xd, "Alias", this.Alias));
            info.AppendChild(xmlHelper.addTextNode(xd, "Icon", this.IconUrl));
            info.AppendChild(xmlHelper.addTextNode(xd, "Thumbnail", this.Thumbnail));
            info.AppendChild(xmlHelper.addTextNode(xd, "Description", this.Description));

            XmlElement pts = xd.CreateElement("GenericProperties");
            foreach (PropertyType pt in this.PropertyTypes)
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
            root.AppendChild(pts);

            // tabs
            XmlElement tabs = xd.CreateElement("Tabs");
            foreach (TabI t in getVirtualTabs.ToList())
            {
                XmlElement tabx = xd.CreateElement("Tab");
                tabx.AppendChild(xmlHelper.addTextNode(xd, "Id", t.Id.ToString()));
                tabx.AppendChild(xmlHelper.addTextNode(xd, "Caption", t.Caption));
                tabs.AppendChild(tabx);
            }
            root.AppendChild(tabs);
            return root;
        }

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(MemberType sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(MemberType sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(MemberType sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when a language is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        } 
        #endregion
	}
}