using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Umbraco.Core.Logging;
using System.Linq;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;

namespace umbraco.cms.businesslogic.member
{
	/// MemberType
	/// 
	/// Due to the inheritance of the ContentType class, it enables definition of generic datafields on a Members.
    [Obsolete("Obsolete, Use the MemberTypeService and Umbraco.Core.Models.MemberType", false)]
	public class MemberType : ContentType
    {
        #region Private Members

        internal static readonly Guid ObjectType = new Guid(Constants.ObjectTypes.MemberType);
        internal IMemberType MemberTypeItem
        {
            get { return base.ContentTypeItem as IMemberType; }
            set { base.ContentTypeItem = value; }
        }

        #endregion

        #region Constructors

        internal MemberType(IMemberType memberType)
            : base(memberType)
        {
            SetupNode(memberType);
        }

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

        #region Regenerate Xml Structures

        /// <summary>
        /// Rebuilds the xml structure for the member item by id
        /// </summary>
        /// <param name="contentId"></param>
        /// <remarks>
        /// This is not thread safe
        /// </remarks>
        internal override void RebuildXmlStructureForContentItem(int contentId)
        {
            var xd = new XmlDocument();
            try
            {
                new Member(contentId).XmlGenerate(xd);
            }
            catch (Exception ee)
            {
                LogHelper.Error<MemberType>("Error generating xml", ee);
            }
        }

        #endregion

        #endregion

        #region Public Methods
        
        /// <summary>
		/// Get an true/false if the Member can edit the given data defined in the propertytype
		/// </summary>
		/// <param name="pt">Propertytype to edit</param>
		/// <returns>True if the Member can edit the data</returns>
        public bool MemberCanEdit(PropertyType pt)
        {
            return MemberTypeItem.MemberCanEditProperty(pt.Alias);
        }
        
		/// <summary>
		/// Get an true/false if the given data defined in the propertytype, should be visible on the members profile page
		/// </summary>
		/// <param name="pt">Propertytype</param>
		/// <returns>True if the data should be displayed on the profilepage</returns>
		public bool ViewOnProfile(PropertyType pt) 
		{
            return MemberTypeItem.MemberCanViewProperty(pt.Alias);
		}
		
		/// <summary>
		/// Set if the member should be able to edit the data defined by its propertytype
		/// </summary>
		/// <param name="pt">PropertyType</param>
		/// <param name="value">True/False if Members of the type shoúld be able to edit the data</param>
        public void setMemberCanEdit(PropertyType pt, bool value)
		{
		    MemberTypeItem.SetMemberCanEditProperty(pt.Alias, value);            
        }
        
		/// <summary>
		/// Set if the data should be displayed on members of this type's profilepage
		/// </summary>
		/// <param name="pt">PropertyType</param>
		/// <param name="value">True/False if the data should be displayed</param>
        public void setMemberViewOnProfile(PropertyType pt, bool value) 
		{
            MemberTypeItem.SetMemberCanViewProperty(pt.Alias, value);            
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
            var e = new DeleteEventArgs();

            FireBeforeDelete(e);

            if (e.Cancel == false) {

                ApplicationContext.Current.Services.MemberTypeService.Delete(MemberTypeItem);

                // delete all documents of this type
                FireAfterDelete(e);
            }
        }

        /// <summary>
        /// Used to persist object changes to the database
        /// </summary>
        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (e.Cancel == false)
            {
                ApplicationContext.Current.Services.MemberTypeService.Save(MemberTypeItem);
                base.Save();
                FireAfterSave(e);
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

        #endregion

        #region Public Static Methods
        /// <summary>
        /// Get a MemberType by it's alias
        /// </summary>
        /// <param name="Alias">The alias of the MemberType</param>
        /// <returns>The MemberType with the given Alias</returns>
        public new static MemberType GetByAlias(string Alias)
        {
            var result = ApplicationContext.Current.Services.MemberTypeService.Get(Alias);
            return result == null ? null : new MemberType(result);
        }

        /// <summary>
        /// Create a new MemberType
        /// </summary>
        /// <param name="Text">The name of the MemberType</param>
        /// <param name="u">Creator of the MemberType</param>
        public static MemberType MakeNew(User u, string Text)
        {
            var alias = helpers.Casing.SafeAliasWithForcingCheck(Text);
            //special case, if it stars with an underscore, we have to re-add it for member types
            if (Text.StartsWith("_")) alias = "_" + alias;
            var mt = new Umbraco.Core.Models.MemberType(-1)
            {
                Level = 1,
                Name = Text,
                Icon = "icon-user",
                Alias = alias
            };
            ApplicationContext.Current.Services.MemberTypeService.Save(mt);
            var legacy = new MemberType(mt);
            var e = new NewEventArgs();
            legacy.OnNew(e);
            return legacy;
        }

        /// <summary>
        /// Retrieve a list of all MemberTypes
        /// </summary>
        public new static MemberType[] GetAll
        {
            get
            {
                var result = ApplicationContext.Current.Services.MemberTypeService.GetAll();
                return result.Select(x => new MemberType(x)).ToArray();
            }
        }
        #endregion

        #region Protected Methods

        protected override void setupNode()
        {
            var memberType = ApplicationContext.Current.Services.MemberTypeService.Get(Id);
            SetupNode(memberType);
        }

        #endregion

        #region Private Methods

        private void SetupNode(IMemberType contentType)
        {
            MemberTypeItem = contentType;            

            base.PopulateContentTypeFromContentTypeBase(MemberTypeItem);
            base.PopulateCMSNodeFromUmbracoEntity(MemberTypeItem, ObjectType);
        }

        #endregion

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