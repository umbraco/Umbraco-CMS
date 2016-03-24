using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
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
		    ApplicationContext.Current.Services.MemberTypeService.Delete(MemberTypeItem);

		}

        /// <summary>
        /// Used to persist object changes to the database
        /// </summary>
        public override void Save()
        {
            ApplicationContext.Current.Services.MemberTypeService.Save(MemberTypeItem);
            base.Save();
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
        public static MemberType MakeNew(IUser u, string Text)
        {
            var alias = Text.ToSafeAliasWithForcingCheck();
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

	}
}