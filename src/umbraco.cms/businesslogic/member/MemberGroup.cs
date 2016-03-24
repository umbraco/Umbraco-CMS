using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.businesslogic.member
{
	/// <summary>
	/// Membergroups are used for grouping Umbraco Members
	/// 
	/// A Member can exist in multiple groups.
	/// 
	/// It's possible to protect webpages/documents by membergroup.
	/// </summary>
	public class MemberGroup : CMSNode
	{
        private static readonly Guid MemberGroupObjectType = new Guid(Constants.ObjectTypes.MemberGroup);

	    private IMemberGroup _memberGroupItem;

        internal MemberGroup(IMemberGroup memberGroup)
            : base(memberGroup)
        {
            _memberGroupItem = memberGroup;
        }

		/// <summary>
		/// Initialize a new object of the MemberGroup class
		/// </summary>
		/// <param name="id">Membergroup id</param>
		public MemberGroup(int id): base(id)
		{

		}

		/// <summary>
		/// Initialize a new object of the MemberGroup class
		/// </summary>
		/// <param name="id">Membergroup id</param>
		public MemberGroup(Guid id) : base(id)
		{
			
		}

        public override string Text
        {
            get
            {
                return _memberGroupItem.Name;
            }
            set
            {
                _memberGroupItem.Name = value;
            }
        }

        /// <summary>
        /// Deltes the current membergroup
        /// </summary>
        [Obsolete("Use System.Web.Security.Role.DeleteRole")]
        public override void delete()
        {

            if (_memberGroupItem != null)
            {
                ApplicationContext.Current.Services.MemberGroupService.Delete(_memberGroupItem);
            }
            else
            {
                var memberGroup = ApplicationContext.Current.Services.MemberGroupService.GetById(Id);
                ApplicationContext.Current.Services.MemberGroupService.Delete(memberGroup);
            }
            
            // Delete all content and cmsnode specific data!
            base.delete();
        }


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {

            ApplicationContext.Current.Services.MemberGroupService.Save(_memberGroupItem);

        }

		/// <summary>
		/// Retrieve a list of all existing MemberGroups
		/// </summary>
		[Obsolete("Use System.Web.Security.Role.GetAllRoles")]
        public static MemberGroup[] GetAll 
		{
			get
			{
			    var result = ApplicationContext.Current.Services.MemberGroupService.GetAll();
			    return result.Select(x => new MemberGroup(x)).ToArray();
			}
		}

	    public int[] GetMembersAsIds()
	    {
            var result = ApplicationContext.Current.Services.MemberService.GetMembersInRole(_memberGroupItem.Name);
	        return result.Select(x => x.Id).ToArray();
	    }

	    [Obsolete("Use System.Web.Security.Roles.FindUsersInRole")]
	    public Member[] GetMembers()
	    {
	        var result = ApplicationContext.Current.Services.MemberService.GetMembersInRole(_memberGroupItem.Name);
            return result.Select(x => new Member(x)).ToArray();
	    }

	    public Member[] GetMembers(string usernameToMatch)
	    {

	        var result = ApplicationContext.Current.Services.MemberService.FindMembersInRole(
	            _memberGroupItem.Name, usernameToMatch, StringPropertyMatchType.StartsWith);

	        return result.Select(x => new Member(x)).ToArray();
	    }

	    public bool HasMember(int memberId)
	    {
	        return SqlHelper.ExecuteScalar<int>("select count(member) from cmsMember2MemberGroup where member = @member and memberGroup = @memberGroup",
	            SqlHelper.CreateParameter("@member", memberId),
	            SqlHelper.CreateParameter("@memberGroup", Id)) > 0;
	    }

	    /// <summary>
		/// Get a membergroup by it's name
		/// </summary>
		/// <param name="name">Name of the membergroup</param>
		/// <returns>If a MemberGroup with the given name exists, it will return this, else: null</returns>
        public static MemberGroup GetByName(string name)
	    {
	        var found = ApplicationContext.Current.Services.MemberGroupService.GetByName(name);
	        if (found == null) return null;
            return new MemberGroup(found);
	    }


		/// <summary>
		/// Create a new MemberGroup
		/// </summary>
		/// <param name="Name">The name of the MemberGroup</param>
		/// <param name="u">The creator of the MemberGroup</param>
		/// <returns>The new MemberGroup</returns>
		public static MemberGroup MakeNew(string Name, IUser u) 
		{
		    var group = new global::Umbraco.Core.Models.MemberGroup {Name = Name};
		    ApplicationContext.Current.Services.MemberGroupService.Save(group);

            var mg = new MemberGroup(group);
            return mg;
		}

	    protected override void setupNode()
	    {
            if (Id == -1)
            {
                base.setupNode();
                return;
            }

            var group = ApplicationContext.Current.Services.MemberGroupService.GetById(Id);

            if (group == null)
                throw new ArgumentException(string.Format("No Member exists with id '{0}'", Id));

            SetupNode(group);
        }

        private void SetupNode(IMemberGroup group)
        {
            _memberGroupItem = group;            

            //Setting private properties 
            base.PopulateCMSNodeFromUmbracoEntity(_memberGroupItem, MemberGroupObjectType);
        }

	  
	}
}
