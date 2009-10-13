using System;
using System.Collections.Generic;


using System.Data;
using System.Xml;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;
using System.Runtime.CompilerServices;

namespace umbraco.cms.businesslogic
{
	/// <summary>
	/// Content is an intermediate layer between CMSNode and class'es which will use generic data.
	/// 
	/// Content is a datastructure that holds generic data defined in its corresponding ContentType. Content can in some
	/// sence be compared to a row in a database table, it's contenttype hold a definition of the columns and the Content
	/// contains the data
	/// 
	/// Note that Content data in umbraco is *not* tablular but in a treestructure.
	/// 
	/// </summary>
	public class Content : CMSNode
	{
		private Guid _version;
		private DateTime _versionDate;
		private XmlNode _xml;
		private bool _versionDateInitialized;
        private string _contentTypeIcon;
        private ContentType _contentType;
        private bool _propertiesInitialized = false;
        private Properties _properties = new Properties();

        public Content()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public Content(int id) : base(id) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="noSetup"></param>
        protected Content(int id, bool noSetup) : base(id, noSetup) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        protected Content(Guid id) : base(id) { }

		/// <summary>
		/// Sets up the ContentType property for this content item and sets the addition content properties manually.
		/// If the ContentType property is not already set, then this will get the ContentType from Cache.
		/// </summary>
		/// <param name="InitContentType"></param>
		/// <param name="InitVersion"></param>
		/// <param name="InitVersionDate"></param>
		/// <param name="InitContentTypeIcon"></param>
        protected void InitializeContent(int InitContentType, Guid InitVersion, DateTime InitVersionDate, string InitContentTypeIcon)
        {
            if (_contentType == null)
				_contentType = ContentType.GetContentType(InitContentType);
            _version = InitVersion;
            _versionDate = InitVersionDate;
            _contentTypeIcon = InitContentTypeIcon;
        }

	    /// <summary>
        /// The current Content objects ContentType, which defines the Properties of the Content (data)
        /// </summary>
        public ContentType ContentType
        {
            get
            {
				if (_contentType == null)
				{
					object o = SqlHelper.ExecuteScalar<object>(
						"Select ContentType from cmsContent where nodeId=@nodeid",
						SqlHelper.CreateParameter("@nodeid", this.Id));
					if (o == null)
						return null;
					int contentTypeId;
					if(!int.TryParse(o.ToString(), out contentTypeId))
						return null;
					try
					{
						_contentType = new ContentType(contentTypeId);
					}
					catch
					{
						return null;
					}
				}
            	return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

	    public Properties GenericProperties
	    {
            get
            {
                if (!_propertiesInitialized)
                    InitializeProperties();
                    
                return _properties;
            }
	    }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            base.Save();
        }

		/// <summary>
		/// The icon used in the tree - placed in this layer for performance reasons.
		/// </summary>
		/// <remarks>
		/// This is here for performance reasons only. If the _contentTypeIcon is manually set
		/// then a database call is not made to initialize the ContentType.
		/// 
		/// The data layer has slightly changed in 4.1 so that for Document and Media, the ContentType
		/// is automatically initialized with one SQL call when creating the documents/medias so using this
		/// method or the ContentType.IconUrl property when accessing the icon from Media or Document 
		/// won't affect performance.
		/// </remarks>
		public string ContentTypeIcon 
		{
			get 
			{
				if (_contentTypeIcon == null && this.ContentType != null)
					_contentTypeIcon = this.ContentType.IconUrl;
				return _contentTypeIcon;
			}
			set 
			{
				_contentTypeIcon = value;
			}
		}

		/// <summary>
		/// Retrieve a Property given the alias
		/// </summary>
		/// <param name="alias">Propertyalias (defined in the documenttype)</param>
		/// <returns>The property with the given alias</returns>
		public Property getProperty(string alias) 
		{
			ContentType ct = this.ContentType;
			if(ct == null)
				return null;
			propertytype.PropertyType pt = ct.getPropertyType(alias);
			if(pt == null)
				return null;
			return getProperty(pt);
		}

		/// <summary>
		/// Retrieve a property given the propertytype
		/// </summary>
		/// <param name="pt">PropertyType</param>
		/// <returns>The property with the given propertytype</returns>
		public Property getProperty(propertytype.PropertyType pt)
		{
			object o = SqlHelper.ExecuteScalar<object>(
				"select id from cmsPropertyData where versionId=@version and propertyTypeId=@propertyTypeId",
				SqlHelper.CreateParameter("@version", this.Version),
				SqlHelper.CreateParameter("@propertyTypeId", pt.Id));
			if(o == null)
				return null;
			int propertyId;
			if(!int.TryParse(o.ToString(), out propertyId))
				return null;
			try
			{
				return new Property(propertyId, pt);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// The createtimestamp on this version
		/// </summary>
		public DateTime VersionDate
		{
			get
			{
				if(!_versionDateInitialized)
				{
					object o = SqlHelper.ExecuteScalar<object>(
						"select VersionDate from cmsContentVersion where versionId = '" + this.Version.ToString() + "'");
					if(o == null)
					{
						_versionDate = DateTime.Now;
					}
					else
					{
						_versionDateInitialized = DateTime.TryParse(o.ToString(), out _versionDate);
					}
				}
				return _versionDate;
			}
			set
			{
				_versionDate = value;
				_versionDateInitialized = true;
			}
		}


		/// <summary>
		/// Optimized method for bulk deletion of properties´on a Content object.
		/// </summary>
		private void deleteAllProperties() 
		{
			SqlHelper.ExecuteNonQuery("Delete from cmsPropertyData where contentNodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
		}

		/// <summary>
		/// Retrieve a list of generic properties of the content
		/// </summary>
		public Property[] getProperties 
		{
			get
			{
				if (this.ContentType == null)
					return new Property[0];

				List<Property> result = new List<Property>();
				foreach(PropertyType prop in this.ContentType.PropertyTypes)
				{
					if (prop == null)
						continue;
					Property p = getProperty(prop);
					if(p == null)
						continue;
					result.Add(p);
				}

				return result.ToArray();
			}
		}


        private void InitializeProperties()
        {
			using(IRecordsReader dr = SqlHelper.ExecuteReader("select id from cmsPropertyData where versionId = @versionId",
                                                         SqlHelper.CreateParameter("@versionId", Version)))
			{
				while(dr.Read())
					_properties.Add(new Property(dr.GetInt("id")));
			}
        }


		/// <summary>
		/// Retrive a list of Content sharing the ContentType
		/// </summary>
		/// <param name="ct">The ContentType</param>
		/// <returns>A list of Content objects sharing the ContentType defined.</returns>
		public static Content[] getContentOfContentType(ContentType ct) {
			IRecordsReader dr = SqlHelper.ExecuteReader("Select nodeId from  cmsContent INNER JOIN umbracoNode ON cmsContent.nodeId = umbracoNode.id where ContentType = " + ct.Id + " ORDER BY umbracoNode.text ");
			System.Collections.ArrayList tmp = new System.Collections.ArrayList();

			while (dr.Read())  tmp.Add(dr.GetInt("nodeId"));
			dr.Close();
			
			Content[] retval = new Content[tmp.Count];
			for (int i = 0;i < tmp.Count; i++)  retval[i] = new Content((int) tmp[i]);

			return retval;
		}

		/// <summary>
		/// Add a property to the Content
		/// </summary>
		/// <param name="pt">The PropertyType of the Property</param>
		/// <param name="versionId">The version of the document on which the property should be add'ed</param>
		/// <returns>The new Property</returns>
		public property.Property addProperty(propertytype.PropertyType pt, Guid versionId) 	{
			return property.Property.MakeNew(pt, this, versionId);
		}

		/// <summary>
		/// Content is under version control, you are able to programatically create new versions
		/// </summary>
		public Guid Version {
			get{				
				if (_version == Guid.Empty)
				{
					string sql = "Select versionId from cmsContentVersion where contentID = " + this.Id +
					             " order by id desc ";

                    using (IRecordsReader dr = SqlHelper.ExecuteReader(sql))
                    {
                        if (!dr.Read())
                            _version = Guid.Empty;
                        else
                            _version = dr.GetGuid("versionId");
                    }
				}	
				return _version;
			}
			set{_version = value;}
		}

		/// <summary>
		/// Creates a new Content object from the ContentType.
		/// </summary>
		/// <param name="ct"></param>
		protected void CreateContent(ContentType ct) {
			SqlHelper.ExecuteNonQuery("insert into cmsContent (nodeId,ContentType) values ("+this.Id+","+ct.Id+")");
			createNewVersion();
		}

		/// <summary>
		/// Indication if the Content exists in at least one version.
		/// </summary>
		/// <returns>Returns true if the Content has a version</returns>
		private bool hasVersion() 
		{
			int versionCount = SqlHelper.ExecuteScalar<int>("select Count(Id) as tmp from cmsContentVersion where contentId = " + this.Id.ToString());
			return (versionCount > 0);
		}

		/// <summary>
		/// Method for creating a new version of the data associated to the Content.
		/// 
		/// </summary>
		/// <returns>The new version Id</returns>
		protected Guid createNewVersion() {
			Guid newVersion = Guid.NewGuid();
			bool tempHasVersion = hasVersion();
			foreach (propertytype.PropertyType pt in this.ContentType.PropertyTypes) 
			{
				object oldValue = "";
				if (tempHasVersion) 
				{
					try 
					{
						oldValue = this.getProperty(pt.Alias).Value;
					} 
					catch {}
				}
				property.Property p = this.addProperty(pt, newVersion);
				if (oldValue != null && oldValue.ToString() != "") p.Value = oldValue;
			}
			SqlHelper.ExecuteNonQuery("Insert into cmsContentVersion (ContentId,versionId) values ("+ this.Id +",'"+newVersion+"')");
			this.Version = newVersion;
			return newVersion;
		}

		/// <summary>
		/// Deletes the current Content object, must be overridden in the child class.
		/// </summary>
		protected new void delete() {

			// Delete all data associated with this content
			this.deleteAllProperties();

			// Delete version history
			SqlHelper.ExecuteNonQuery("Delete from cmsContentVersion where ContentId = " + this.Id);
			
			// Delete Contentspecific data ()
			SqlHelper.ExecuteNonQuery("Delete from cmsContent where NodeId = " + this.Id);

			// Delete xml
			SqlHelper.ExecuteNonQuery("delete from cmsContentXml where nodeID = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));

			// Delete Nodeinformation!!
			base.delete();
		}

		/// <summary>
		/// Initialize a contentobject given a version.
		/// </summary>
		/// <param name="version">The version identifier</param>
		/// <returns>The Content object from the given version</returns>
		public static Content GetContentFromVersion(Guid version) {
			int tmpContentId = SqlHelper.ExecuteScalar<int>("Select ContentId from cmsContentVersion where versionId = '" + version.ToString() + "'");
			return new Content(tmpContentId);
		}


		private XmlNode importXml() 
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(SqlHelper.ExecuteXmlReader("select xml from cmsContentXml where nodeID = " + this.Id.ToString()));

			return xmlDoc.FirstChild;
		}

		/// <summary>
		/// An Xmlrepresentation of a Content object.
		/// </summary>
		/// <param name="xd">Xmldocument context</param>
		/// <param name="Deep">If true, the Contents children are appended to the Xmlnode recursive</param>
		/// <returns>The Xmlrepresentation of the data on the Content object</returns>
		public new virtual XmlNode ToXml(XmlDocument xd, bool Deep) 
		{
			if (_xml == null) 
			{
				XmlDocument xmlDoc = new XmlDocument();
                // we add a try/catch clause here, as the xmlreader will throw an exception if there's no xml in the table
                // after the empty catch we'll generate the xml which is why we don't do anything in the catch part
                try
                {
                    XmlReader xr = SqlHelper.ExecuteXmlReader("select xml from cmsContentXml where nodeID = " + this.Id.ToString());
                    if (xr.MoveToContent() != System.Xml.XmlNodeType.None)
                    {
                        xmlDoc.Load(xr);
                        _xml = xmlDoc.FirstChild;
                    }
                    xr.Close();
                }
                catch
                {
                }


				// Generate xml if xml still null (then it hasn't been initialized before)
				if (_xml == null) 
				{
					this.XmlGenerate(new XmlDocument());
					_xml = importXml();
				}

			}

			XmlNode x = xd.ImportNode(_xml, true);

			if (Deep) 
			{
                //store children array here because iterating over an Array object is very inneficient.
                var childs = this.Children;
                foreach (BusinessLogic.console.IconI c in childs)
                {
                    try
                    {
                        x.AppendChild(new Content(c.Id).ToXml(xd, true));
                    }
                    catch (Exception mExp)
                    {
                        System.Web.HttpContext.Current.Trace.Warn("Content", "Error adding node to xml: " + mExp.ToString());
                    }
                }					
			}

			return x;

		}

		/// <summary>
		/// Removes the Xml cached in the database - unpublish and cleaning
		/// </summary>
		public virtual void XmlRemoveFromDB() 
		{
			SqlHelper.ExecuteNonQuery("delete from cmsContentXml where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
		}

		/// <summary>
		/// Generates the Content XmlNode
		/// </summary>
		/// <param name="xd"></param>
		public virtual void XmlGenerate(XmlDocument xd) 
		{
			XmlNode node = xd.CreateNode(XmlNodeType.Element, "node", String.Empty);
            XmlPopulate(xd, ref node, false);
            SaveXmlDocument(node);
		}

		public virtual void XmlPopulate(XmlDocument xd, ref XmlNode x, bool Deep) 
		{
            var props = this.getProperties;
			foreach (property.Property p in props)
				if (p != null)
					x.AppendChild(p.ToXml(xd));

			// attributes
			x.Attributes.Append(xmlHelper.addAttribute(xd, "id", this.Id.ToString()));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "version", this.Version.ToString()));
			if (this.Level > 1)
				x.Attributes.Append(xmlHelper.addAttribute(xd, "parentID", this.Parent.Id.ToString()));
			else
				x.Attributes.Append(xmlHelper.addAttribute(xd, "parentID", "-1"));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "level", this.Level.ToString()));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "writerID", this.User.Id.ToString()));
			if (this.ContentType != null)
				x.Attributes.Append(xmlHelper.addAttribute(xd, "nodeType", this.ContentType.Id.ToString()));
			x.Attributes.Append(xmlHelper.addAttribute(	xd, "template", "0"));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "sortOrder", this.sortOrder.ToString()));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "createDate", this.CreateDateTime.ToString("s")));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "updateDate", this.VersionDate.ToString("s")));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "nodeName", this.Text));
			if (this.Text != null)
				x.Attributes.Append(xmlHelper.addAttribute(xd, "urlName", this.Text.Replace(" ", "").ToLower()));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "writerName", this.User.Name));
			if (this.ContentType != null)
				x.Attributes.Append(xmlHelper.addAttribute(xd, "nodeTypeAlias", this.ContentType.Alias));
			x.Attributes.Append(xmlHelper.addAttribute(xd, "path", this.Path));

			if (Deep) 
			{
                //store children array here because iterating over an Array property object is very inneficient.
                var children = this.Children;
                foreach (Content c in children)
					x.AppendChild(c.ToXml(xd, true));
			}
		}

        /// <summary>
        /// Saves the XML document to the data source.
        /// </summary>
        /// <param name="node">The XML Document.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void SaveXmlDocument(XmlNode node)
        {
            // Method is synchronized so exists remains consistent (avoiding race condition)
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @nodeId",
                                           SqlHelper.CreateParameter("@nodeId", Id)) > 0;
            string query;
            if (exists)
                query = "UPDATE cmsContentXml SET xml = @xml WHERE nodeId = @nodeId";
            else
                query = "INSERT INTO cmsContentXml(nodeId, xml) VALUES (@nodeId, @xml)";
            SqlHelper.ExecuteNonQuery(query,
                                      SqlHelper.CreateParameter("@nodeId", Id),
                                      SqlHelper.CreateParameter("@xml", node.OuterXml));
        }
	}

	/// <summary>
	/// Not implemented
	/// </summary>
	public interface ISaveHandlerContents
	{
		/// <summary>
		/// Not implemented
		/// </summary>
		bool Execute(cms.businesslogic.Content contentObject);
	}
}