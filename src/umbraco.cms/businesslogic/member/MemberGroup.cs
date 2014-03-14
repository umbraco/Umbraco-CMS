using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using umbraco.DataLayer;
using System.Collections;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

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
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);
            if (e.Cancel) return;

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
            FireAfterDelete(e);
        }


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);
            if (e.Cancel) return;

            ApplicationContext.Current.Services.MemberGroupService.Save(_memberGroupItem);

            FireAfterSave(e);
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
		public static MemberGroup MakeNew(string Name, BusinessLogic.User u) 
		{
		    var group = new global::Umbraco.Core.Models.MemberGroup {Name = Name};
		    ApplicationContext.Current.Services.MemberGroupService.Save(group);

            var mg = new MemberGroup(group);
            var e = new NewEventArgs();
            mg.OnNew(e);
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

	    //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(MemberGroup sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(MemberGroup sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(MemberGroup sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when a language is saved.
        /// </summary>
        public new static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public new static event SaveEventHandler AfterSave;
        protected new virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
	}
}
